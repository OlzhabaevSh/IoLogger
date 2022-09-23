import { WebviewPanel } from "vscode";

export interface IBaseWindow {
    id: string;
    webview: WebviewPanel;
}