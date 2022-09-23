import path = require("path");
import {
  Disposable,
  ExtensionContext,
  Uri,
  ViewColumn,
  WebviewPanel,
  window,
} from "vscode";
import { ILoggerViewTreeDataItem } from "../models/logger-view-tree-data-item";
import { SupportedHttpMethod } from "../models/supported-http-methods";
import { IBaseWindow } from "./base.window";

export class HttpRequestWindow implements IBaseWindow, Disposable {
  public webview: WebviewPanel;
  cssSrc: Uri;

  constructor(
    public readonly id: string,
    private readonly item: ILoggerViewTreeDataItem,
    private readonly context: ExtensionContext
  ) {
    this.webview = window.createWebviewPanel(
      this.id,
      `${SupportedHttpMethod[item.method]} - ${item.url}`,
      ViewColumn.Active
    );
    const cssPath = path.join(
      context.extensionPath,
      "media",
      "http-request-window.css"
    );
    const cssUri = Uri.file(cssPath);
    this.cssSrc = this.webview.webview.asWebviewUri(cssUri);
    this.webview.webview.html = this.getHttpContent();
  }

  private getHttpContent() {
    return `
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>${SupportedHttpMethod[this.item.method]} - ${
      this.item.url
    }</title>
    <link rel="stylesheet" href="${this.cssSrc}">
    </head>
    <body>
        <div class="dio-section">
            <div class="dio-section-title">General</div>
            <div class="dio-section-item"  tabindex="0">
                <span class="dio-section-item-title">Request URL:</span>
                <span>${this.item.url}</span>
            </div>
            <div class="dio-section-item"  tabindex="0">
                <span class="dio-section-item-title">Request Method:</span>
                <span>${SupportedHttpMethod[this.item.method]}</span>
            </div>
            <div class="dio-section-item"  tabindex="0">
                <span class="dio-section-item-title">Status Code:</span>
                <span>${this.item.statusCode ? this.item.statusCode : 'Pending...'}</span>
            </div>
        </div>
        ${this.getHeadersSectionHttpContent('Request Headers', this.item.requestHeaders)}
        ${this.getHeadersSectionHttpContent('Response Headers', this.item.responseHeaders)}
        
    </body>
    </html>    
    `;
  }

  private getHeadersSectionHttpContent(label: string, headers?: Record<string, string> | null) {
    if (!headers) {
      return "";
    }

    const headerKeys = Object.keys(headers);

    return `
    <div class="dio-section">
      <div class="dio-section-title">${label}</div>
      ${headerKeys
        .map(
          (key) => `
      <div class="dio-section-item" tabindex="0">
          <span class="dio-section-item-title">${key}:</span>
          <span>${headers[key]}</span>
      </div>
      `
        )
        .join("")}
    </div>
  `;
  }

  dispose() {
    this.webview.dispose();
  }
}
