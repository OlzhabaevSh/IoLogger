using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.IoLogger.Core.Aspnet;
using System.Data.Common;
using System.Reflection;
using System.Text.Json;

namespace Microsoft.IoLogger.ConsoleClient
{
    internal class Program
    {
        private static readonly string _signalrUri = "https://localhost:7211/logsHub";
        private static HubConnection _hubConnection;
        private static int _processId;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Provide process id:");
            _processId = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Start processing");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_signalrUri)
            .Build();

            _hubConnection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await _hubConnection.StartAsync();
            };

            _hubConnection.On<int[]>("ProcessesReceived", processIds =>
            {
                Console.WriteLine("**************");
                Console.WriteLine("ProcessesReceived: ");
                foreach (var item in processIds)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine("**************");
            });

            _hubConnection.On<Microsoft.IoLogger.Core.Http.HttpRequestMessage>("httpRequest", httpRequest =>
            {
                Console.WriteLine("**************");
                Console.WriteLine("httpRequest: ");
                Console.WriteLine($"{JsonSerializer.Serialize(httpRequest)}");
                Console.WriteLine("**************");
            });

            _hubConnection.On<Microsoft.IoLogger.Core.Http.HttpResponseMessage>("httpResponse", httpResponse =>
            {
                Console.WriteLine("**************");
                Console.WriteLine("httpResponse: ");
                Console.WriteLine($"{JsonSerializer.Serialize(httpResponse)}");
                Console.WriteLine("**************");
            });

            _hubConnection.On<Core.Aspnet.AspnetRequestMessage>("aspnetRequest", aspnetRequest =>
            {
                Console.WriteLine("**************");
                Console.WriteLine("aspnetRequest: ");
                Console.WriteLine($"{JsonSerializer.Serialize(aspnetRequest)}");
                Console.WriteLine("**************");
            });

            _hubConnection.On<AspnetResponseMessage>("aspnetRequest", aspnetRequest =>
            {
                Console.WriteLine("**************");
                Console.WriteLine("aspnetRequest: ");
                Console.WriteLine($"{JsonSerializer.Serialize(aspnetRequest)}");
                Console.WriteLine("**************");
            });

            Console.CancelKeyPress += async (object? sender, ConsoleCancelEventArgs e) =>
            {
                await _hubConnection.InvokeAsync("Unsubscribe", _processId);
                await _hubConnection.DisposeAsync();
            };

            // connect
            await _hubConnection.StartAsync();

            // select process
            await _hubConnection.InvokeAsync("Subscribe", _processId);

            Console.ReadKey();
        }
    }
}