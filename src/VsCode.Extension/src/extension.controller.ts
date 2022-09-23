import {
  Disposable,
  ExtensionContext,
  TreeView,
  TreeViewSelectionChangeEvent,
} from "vscode";
import * as vscode from "vscode";
import { SignalRConnection } from "./signalr-connection";
import { LoggerViewTreeDataProvider } from "./tree-providers/logger-view-tree-data-provider";
import { COMMANDS, VIEWS, WEBVIEWS } from "./constants";
import { ILoggerViewTreeDataItem } from "./models/logger-view-tree-data-item";
import { IBaseWindow } from "./windows/base.window";
import { HttpRequestWindow } from "./windows/http-request.window";
import { firstValueFrom, map, take, takeLast } from "rxjs";
import {
  execFile,
  ChildProcess,
} from "child_process";
import path = require("path");

export class DotnetIoLoggerExtensionContoller implements Disposable {
  private readonly socket: SignalRConnection;
  private readonly httpRequestsLoggerTreeProvider: LoggerViewTreeDataProvider;
  private readonly httpRequestsLoggerTreeView: TreeView<ILoggerViewTreeDataItem>;
  private readonly aspnetRequestsLoggerTreeProvider: LoggerViewTreeDataProvider;
  private readonly aspnetRequestsLoggerTreeView: TreeView<ILoggerViewTreeDataItem>;
  private readonly detailWindows: IBaseWindow[] = [];
  private server?: ChildProcess;

  constructor(private readonly context: ExtensionContext) {
    

    this.socket = new SignalRConnection("http://localhost:3231/logsHub");
    
    this.httpRequestsLoggerTreeProvider = new LoggerViewTreeDataProvider(context);
    this.httpRequestsLoggerTreeView = vscode.window.createTreeView(VIEWS.httpRequestsView, {
      treeDataProvider: this.httpRequestsLoggerTreeProvider,
    });
    this.httpRequestsLoggerTreeView.onDidChangeSelection(
      this.onLoggerTreeViewSelectionChange.bind(this)
    );
    context.subscriptions.push(this.httpRequestsLoggerTreeView);

    this.aspnetRequestsLoggerTreeProvider = new LoggerViewTreeDataProvider(context);
    this.aspnetRequestsLoggerTreeView = vscode.window.createTreeView(VIEWS.aspnetRequestsView, {
      treeDataProvider: this.aspnetRequestsLoggerTreeProvider,
    });
    this.aspnetRequestsLoggerTreeView.onDidChangeSelection(
      this.onLoggerTreeViewSelectionChange.bind(this)
    );
    context.subscriptions.push(this.aspnetRequestsLoggerTreeView);

    context.subscriptions.push(
      vscode.commands.registerCommand(
        COMMANDS.clearHistory,
        this.clearHistory,
        this
      ),
      vscode.commands.registerCommand(
        COMMANDS.connect,
        this.handleConnectToDotnetProcessCommand,
        this
      )
    );

    this.server = execFile(
      path.join(
        context.extensionPath,
        "media",
        "mac-arm",
        "Microsoft.IoLogger.Server"
      ),
      ["--urls=http://localhost:3231"],
    );
    
    this.server.on("close", (code) => {
      delete this.server;
    });

    this.subscribeToEvents();

    context.subscriptions.push(this);
  }
  
  private subscribeToEvents() {
    this.socket.httpRequestRecieved$.subscribe((request) => {
      this.httpRequestsLoggerTreeProvider.requestRecieved(request);
    });
    this.socket.httpResponseRecieved$.subscribe((response) => {
      this.httpRequestsLoggerTreeProvider.responseRecieved(response);
    });

    this.socket.aspnetRequestRecieved$.subscribe((request) => {
      console.log('aspnet request:', request);
      this.aspnetRequestsLoggerTreeProvider.requestRecieved(request);
    });
    this.socket.aspnetResponseRecieved$.subscribe((response) => {
      console.log('aspnet response:', response);
      this.aspnetRequestsLoggerTreeProvider.responseRecieved(response);
    });
  }

  private async handleConnectToDotnetProcessCommand() {
    const quickPick = vscode.window.showQuickPick(
      firstValueFrom(
        this.socket.processesRecieved$.pipe(
          take(1),
          map((processes) => processes.map((i) => i.toString()))
        )
      ),
      { canPickMany: false }
    );
    await this.socket.getProcesses();
    const process = await quickPick;
    if (process) {
      await this.connectToDotnetProcess(process);
    }
  }

  private async connectToDotnetProcess(process: string) {
    await this.socket.connectToProcess(+process);
  }

  private clearHistory() {
    this.httpRequestsLoggerTreeProvider.clearHistory();
  }

  private onLoggerTreeViewSelectionChange({
    selection,
  }: TreeViewSelectionChangeEvent<ILoggerViewTreeDataItem>) {
    if (selection.length !== 1) {
      return null;
    }
    const item = selection[0];

    const id = item.id;

    const detailsWindow = this.detailWindows.find((w) => w.id === id);
    if (detailsWindow) {
      detailsWindow.webview.reveal();
      return null;
    }

    const webview = new HttpRequestWindow(id, item, this.context);
    this.detailWindows.push(webview);

    webview.webview.onDidDispose(() => {
      this.detailWindows.splice(
        this.detailWindows.findIndex((i) => i.id === id),
        1
      );
    });

    this.context.subscriptions.push(webview);
  }

  async activate() {}

  dispose() {
    this.socket.dispose();
    this.server && this.server.kill();
  }
}
