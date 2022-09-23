import { SupportedHttpMethod } from "./supported-http-methods";

export interface IHttpRequest {
    body: object | null;
    correlationId: string;
    date: string;
    headers: Record<string, string> | null;
    isMessageCompleted: boolean;
    method: SupportedHttpMethod;
    uri: string;
}