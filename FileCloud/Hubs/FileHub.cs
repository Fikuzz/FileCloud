using Microsoft.AspNetCore.SignalR;

namespace FileCloud.Hubs
{
    public class FileHub : Hub
    {
        public Task Ping() => Task.CompletedTask;

        // Метод для подключения клиента к группе, соответствующей текущей папке
        public async Task JoinFolderGroup(Guid folderId)
        {
            // Добавляем текущее соединение в группу
            await Groups.AddToGroupAsync(Context.ConnectionId, folderId.ToString());

            // Подтверждение клиенту
            await Clients.Caller.SendAsync("Notify", $"Watching folder: {folderId.ToString()}");
        }

        // Метод для отключения от группы папки
        public async Task LeaveFolderGroup(Guid folderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, folderId.ToString());
        }
    }
}
