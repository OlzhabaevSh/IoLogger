using Microsoft.IoLogger.Server.Hubs;
using Microsoft.IoLogger.Server.Services;

namespace Microsoft.IoLogger.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddSignalR();

            builder.Services.AddSingleton<LoggerService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();
            app.MapHub<LogsHub>("/logsHub");
            
            app.Run();
        }
    }
}