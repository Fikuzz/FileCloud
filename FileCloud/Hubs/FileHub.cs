using Microsoft.AspNetCore.SignalR;

namespace FileCloud.Hubs
{
    public class FileHub : Hub
    {
        public async Task SendFileLoaded(string fileId)
        {
            await Clients.All.SendAsync("FileLoaded", fileId);
        }
    }
}
