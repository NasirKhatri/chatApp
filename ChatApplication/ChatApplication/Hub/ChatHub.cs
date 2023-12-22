using ChatApplication.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatApplication.Hub
{
    public class ChatHub: Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly IDictionary<string, UserRoomConnection> _connection;

        public ChatHub(IDictionary<string, UserRoomConnection> connection)
        {
            _connection = connection;
        }

        public async Task JoinRoom(UserRoomConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName: userConnection.Room!);
            _connection[Context.ConnectionId] = userConnection;
            await Clients.Group(userConnection.Room!)
                .SendAsync(method: "ReceiveMessage", arg1: "Nasir's Chat Room", arg2: $"{userConnection.User} has joined the Group", arg3: DateTime.Now);
            await SendConnectedUser(userConnection.Room!);
        }

        public async Task SendMessage(string message)
        {
            if(_connection.TryGetValue(Context.ConnectionId, out UserRoomConnection userConnection))
            {
                await Clients.Group(userConnection.Room!)
                    .SendAsync(method: "ReceiveMessage", arg1: userConnection.User, arg2: message, arg3: DateTime.Now);
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if(!_connection.TryGetValue(Context.ConnectionId, out UserRoomConnection userConnection))
            {
                return base.OnDisconnectedAsync(exception);
            }
            _connection.Remove(Context.ConnectionId);
            Clients.Group(userConnection.Room)
                .SendAsync(method: "ReceiveMessage", arg1: "Let's Program Bot", arg2: $"{userConnection.User} has left the group", arg3: DateTime.Now);
            SendConnectedUser(userConnection.Room!);
            return base.OnDisconnectedAsync(exception);
        }

        public Task SendConnectedUser(string room)
        {
            IEnumerable<UserRoomConnection> temp = _connection.Values
                            .Where(u => u.Room == room);
            var users = temp
                .Select(s => s.User);
            return Clients.Group(room).SendAsync(method: "ConnectedUser", users);
        }
    }
}
