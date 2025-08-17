using Microsoft.AspNetCore.SignalR;

namespace FileCloud.Hubs
{
    public class FileHub : Hub
    {
        public Task Ping() => Task.CompletedTask;

    }
}
