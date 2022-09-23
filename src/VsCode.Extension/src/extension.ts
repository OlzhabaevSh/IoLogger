import * as vscode from "vscode";
import { DotnetIoLoggerExtensionContoller } from "./extension.controller";

export async function activate(context: vscode.ExtensionContext) {
  console.log(
    'Congratulations, your extension "dotnet-io-logger" is now active!'
  );

  const loggerExtensionContoller = new DotnetIoLoggerExtensionContoller(
    context
  );

  await loggerExtensionContoller.activate();
}

// this method is called when your extension is deactivated
export function deactivate() {}
