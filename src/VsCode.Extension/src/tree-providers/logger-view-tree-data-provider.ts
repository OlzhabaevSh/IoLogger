import {
  CancellationToken,
  Event,
  EventEmitter,
  ExtensionContext,
  ProviderResult,
  TreeDataProvider,
  TreeItem,
  TreeItemCollapsibleState,
} from "vscode";
import { IHttpRequest } from "../models/http-request";
import { IHttpResponse } from "../models/http-response";

import { IOOperationStatus } from "../models/io-operation-status";
import { ILoggerViewTreeDataItem } from "../models/logger-view-tree-data-item";
import { SupportedHttpMethod } from "../models/supported-http-methods";

export class LoggerViewTreeDataProvider
  implements TreeDataProvider<ILoggerViewTreeDataItem>
{
  
  constructor(private readonly context: ExtensionContext) {}

  private items: ILoggerViewTreeDataItem[] = [
    
  ];

  private _onDidChangeTreeData = new EventEmitter<void>();
  onDidChangeTreeData: Event<void> = this._onDidChangeTreeData.event;

  getTreeItem(element: ILoggerViewTreeDataItem): TreeItem | Thenable<TreeItem> {
    const label = `${this.getStatusLabel(element.statusCode)} ${SupportedHttpMethod[element.method]}: ${element.url}`;
    const item = new TreeItem(label, TreeItemCollapsibleState.None);

    return item;
  }

  getStatusLabel(statusCode: number | undefined) {
    if (!statusCode) {
      return '◯';
    }
    if (statusCode >= 200 && statusCode < 300) {
      return '🟢';
    } else if (statusCode >= 300 && statusCode < 400) {
      return '🟡';
    } else {
      return '🔴';
    }
  }

  getChildren(
    element?: ILoggerViewTreeDataItem | undefined
  ): ProviderResult<ILoggerViewTreeDataItem[]> {
    if (element) {
      return [];
    }
    return this.items;
  }

  public clearHistory() {
    this.items = [];
    this._onDidChangeTreeData.fire();
  }

  getParent?(
    element: ILoggerViewTreeDataItem
  ): ProviderResult<ILoggerViewTreeDataItem> {
    return undefined;
  }

  resolveTreeItem?(
    item: TreeItem,
    element: ILoggerViewTreeDataItem,
    token: CancellationToken
  ): ProviderResult<TreeItem> {
    return item;
  }

  responseRecieved(response: IHttpResponse) {
    const item = this.items.find(i => i.id === response.correlationId);
    if (!item) {
      return;
    }
    item.responseHeaders = response.headers;
    item.statusCode = response.statusCode;
    item.status = IOOperationStatus.Completed;
    this._onDidChangeTreeData.fire();
  }

  requestRecieved(request: IHttpRequest) {
    this.items.push({
      url: request.uri,
      method: request.method,
      status: IOOperationStatus.Pending,
      requestHeaders: request.headers,
      id: request.correlationId
    });
    this._onDidChangeTreeData.fire();
  }
}
