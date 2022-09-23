export interface IHttpResponse {
    body: object | null;
    correlationId: string;
    date: string;
    headers: Record<string, string>;
    isMessageCompleted: boolean;
    statusCode: number;
}