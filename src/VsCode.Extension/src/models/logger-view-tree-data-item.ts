import { IHttpRequest } from "./http-request";
import { IHttpResponse } from "./http-response";
import { IOOperationStatus } from "./io-operation-status";
import { SupportedHttpMethod } from "./supported-http-methods";

export interface ILoggerViewTreeDataItem {
    url: string;
    method: SupportedHttpMethod;
    status: IOOperationStatus;
    statusCode?: number;
    requestHeaders?: Record<string, string> | null;
    responseHeaders?: Record<string, string> | null;
    id: string;
  }