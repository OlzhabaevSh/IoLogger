import * as SignalR from "@microsoft/signalr";
import { Disposable } from "vscode";
import { Subject } from "rxjs";
import { IHttpRequest } from "./models/http-request";
import { IHttpResponse } from "./models/http-response";

export class SignalRConnection implements Disposable {
  private readonly connection: SignalR.HubConnection;
  private connectionAwaiter?: Promise<any>;
  private started = false;
  private processId?: number;
  private connectedToProcess = false;
  private connectingToProcess = false;

  private readonly processesRecievedSubject = new Subject<number[]>();
  public readonly processesRecieved$ =
    this.processesRecievedSubject.asObservable();

  private readonly httpRequestRecievedSubject = new Subject<IHttpRequest>();
  public readonly httpRequestRecieved$ = this.httpRequestRecievedSubject.asObservable();

  private readonly httpResponseRecievedSubject = new Subject<IHttpResponse>();
  public readonly httpResponseRecieved$ = this.httpResponseRecievedSubject.asObservable();

  private readonly aspnetRequestRecievedSubject = new Subject<IHttpRequest>();
  public readonly aspnetRequestRecieved$ = this.aspnetRequestRecievedSubject.asObservable();

  private readonly aspnetResponseRecievedSubject = new Subject<IHttpResponse>();
  public readonly aspnetResponseRecieved$ = this.aspnetResponseRecievedSubject.asObservable();

  constructor(private readonly url: string) {
    this.connection = new SignalR.HubConnectionBuilder()
      .withUrl(this.url)
      .build();
    this.connection.on("ProcessesReceived", this.processesRecieved.bind(this));
    this.connection.on("httpRequest", this.httpRequestRecieved.bind(this));
    this.connection.on("httpResponse", this.httpResponseRecieved.bind(this));
    this.connection.on("aspnetRequest", this.aspnetRequestRecieved.bind(this));
    this.connection.on("aspnetResponse", this.aspnetResponseRecieved.bind(this));
  }

  async dispose() {
    this.processesRecievedSubject.complete();
    this.httpRequestRecievedSubject.complete();
    this.httpResponseRecievedSubject.complete();
    
    if (this.connectedToProcess && this.processId !== undefined) {
      await this.disconnectFromProcess(this.processId);
    }
    await this.connection.stop();
  }

  private async awaitConnection() {
    if (!this.started) {
      this.connectionAwaiter = this.start();
    }
    if (this.connectionAwaiter) {
      return await this.connectionAwaiter;
    } else {
      return await Promise.resolve();
    }
  }

  private async start() {
    this.started = true;
    this.connectionAwaiter = this.connection
      .start()
      .then(() => delete this.connectionAwaiter);
    await this.connectionAwaiter;
  }

  private processesRecieved(processes: number[]) {
    console.log("processesRecieved:", processes);
    this.processesRecievedSubject.next(processes);
  }

  private httpRequestRecieved(request: IHttpRequest) {
    this.httpRequestRecievedSubject.next(request);
  }

  private httpResponseRecieved(response: IHttpResponse) {
    this.httpResponseRecievedSubject.next(response);
  }

  private aspnetRequestRecieved(request: IHttpRequest) {
    this.aspnetRequestRecievedSubject.next(request);
  }

  private aspnetResponseRecieved(response: IHttpResponse) {
    this.aspnetResponseRecievedSubject.next(response);
  }

  public async getProcesses() {
    await this.awaitConnection();
    await this.connection.send("GetProcesses");
  }

  public async connectToProcess(processId: number) {
    if (this.connectingToProcess) {
      throw new Error(
        "You are already connecting to a process please wait for operation end"
      );
    }
    if (this.connectedToProcess) {
      throw new Error("Already connected to process");
    }
    this.connectingToProcess = true;
    try {
      await this.awaitConnection();
      await this.connection.send("Subscribe", processId);
      this.processId = processId;
      this.connectedToProcess = true;
      console.log('subscrived to log events');
    } catch (ex) {
      throw ex;
    } finally {
      this.connectingToProcess = false;
    }
  }

  public async disconnectFromProcess(processId: number) {
    await this.awaitConnection();
    await this.connection.send("Unsubscribe", processId);
    console.log('unsubscribed from log events');
  }
}
