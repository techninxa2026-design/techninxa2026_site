using Microsoft.AspNetCore.SignalR;
using techninxa.Models;
using Techninxa.Models;
using System.Collections.Concurrent;

namespace Techninxa.Hubs
{
    public class CompanyChatHub : Hub
    {
        private readonly ApplicationDbContext _db;

        private static readonly ConcurrentDictionary<string, string> OnlineUsers = new();

        public CompanyChatHub(ApplicationDbContext db)
        {
            _db = db;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            if (httpContext?.Session.GetString("CompanyChatLoggedIn") != "true")
            {
                Context.Abort();
                return;
            }

            var username =
                httpContext.Session.GetString("CompanyChatUsername");

            if (string.IsNullOrWhiteSpace(username))
            {
                Context.Abort();
                return;
            }

            username = username.Trim();

            OnlineUsers[Context.ConnectionId] = username;

            await Clients.All.SendAsync(
                "UserOnline",
                username
            );

            await SendOnlineUsers();

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(
            Exception? exception)
        {
            if (OnlineUsers.TryRemove(
                Context.ConnectionId,
                out var username))
            {
                // Check whether the same user has another connection
                var stillOnline = OnlineUsers.Values.Any(
                    x => x.Equals(
                        username,
                        StringComparison.OrdinalIgnoreCase)
                );

                if (!stillOnline)
                {
                    await Clients.All.SendAsync(
                        "UserOffline",
                        username
                    );
                }

                await SendOnlineUsers();
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            var httpContext = Context.GetHttpContext();

            if (httpContext?.Session.GetString("CompanyChatLoggedIn") != "true")
            {
                return;
            }

            var username =
                httpContext.Session.GetString("CompanyChatUsername");

            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            username = username.Trim();
            message = message.Trim();

            if (message.Length > 1000)
            {
                return;
            }

            var chatMessage = new ChatMessage
            {
                Username = username,
                Message = message,
                SentAt = DateTime.Now
            };

            _db.ChatMessages.Add(chatMessage);

            await _db.SaveChangesAsync();

            await Clients.All.SendAsync(
                "ReceiveMessage",
                chatMessage.Id,
                chatMessage.Username,
                chatMessage.Message,
                chatMessage.SentAt.ToString("hh:mm tt")
            );
        }

        public async Task SendFileMessage(
            string fileName,
            string fileUrl,
            bool isImage)
        {
            var httpContext = Context.GetHttpContext();

            if (httpContext?.Session.GetString("CompanyChatLoggedIn") != "true")
            {
                return;
            }

            var username =
                httpContext.Session.GetString("CompanyChatUsername");

            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(fileName) ||
                string.IsNullOrWhiteSpace(fileUrl))
            {
                return;
            }

            username = username.Trim();

            // Store a special attachment message.
            var attachmentMessage =
                $"[FILE]|{fileName}|{fileUrl}|{isImage}";

            var chatMessage = new ChatMessage
            {
                Username = username,
                Message = attachmentMessage,
                SentAt = DateTime.Now
            };

            _db.ChatMessages.Add(chatMessage);

            await _db.SaveChangesAsync();

            await Clients.All.SendAsync(
                "ReceiveMessage",
                chatMessage.Id,
                chatMessage.Username,
                chatMessage.Message,
                chatMessage.SentAt.ToString("hh:mm tt")
            );
        }

        private async Task SendOnlineUsers()
        {
            var users = OnlineUsers.Values
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            await Clients.All.SendAsync(
                "OnlineUsers",
                users
            );
        }
    }
}