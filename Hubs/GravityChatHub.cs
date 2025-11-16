using GravityChat.Models;
using GravityChat.Services;
using Microsoft.AspNetCore.SignalR;

namespace GravityChat.Hubs
{
    public class GravityChatHub : Hub
    {
        private readonly UniverseManager _universe;

        public GravityChatHub(UniverseManager universe)
        {
            _universe = universe;
        }

        public override async Task OnConnectedAsync()
        {
            string userName = GenerateCoolName();

            var user = _universe.AddUser(Context.ConnectionId, userName);

            await Groups.AddToGroupAsync(Context.ConnectionId, user.ClusterId);

            // Send initial info to the caller
            await Clients.Caller.SendAsync("UniverseInit", new
            {
                connectionId = user.ConnectionId,
                userName = user.Name,
                x = user.X,
                y = user.Y,
                clusterId = user.ClusterId
            });

            // Send updated cluster info to all members
            await BroadcastClusterUpdate(user.ClusterId);

            // Notify cluster members that someone joined
            await Clients.Group(user.ClusterId).SendAsync("ClusterNotification", new
            {
                type = "join",
                id = user.ConnectionId,
                name = user.Name
            });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = _universe.GetUser(Context.ConnectionId);
            var clusterId = user?.ClusterId;

            _universe.RemoveUser(Context.ConnectionId);

            if (clusterId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, clusterId);

                // Notify cluster members that someone left
                await Clients.Group(clusterId).SendAsync("ClusterNotification", new
                {
                    type = "leave",
                    id = Context.ConnectionId,
                    name = user?.Name
                });

                await BroadcastClusterUpdate(clusterId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ---------- CHAT METHODS ---------- //

        public async Task SendClusterMessage(string clusterId, string message)
        {
            var user = _universe.GetUser(Context.ConnectionId);
            if (user == null) return;

            var payload = new
            {
                senderId = user.ConnectionId,
                senderName = user.Name,
                message
            };

            await Clients.Group(clusterId).SendAsync("ReceiveClusterMessage", payload);
        }

        public async Task SendPrivateMessage(string targetConnectionId, string message)
        {
            var user = _universe.GetUser(Context.ConnectionId);
            if (user == null) return;

            var payload = new
            {
                senderId = user.ConnectionId,
                senderName = user.Name,
                message
            };

            // send to recipient
            await Clients.Client(targetConnectionId).SendAsync("ReceivePrivateMessage", payload);

            // send an "echo" to the caller so the client can append outgoing message into the correct conversation
            var echo = new
            {
                // receiver id so client knows which conversation to append to
                recipientId = targetConnectionId,
                senderId = user.ConnectionId,
                senderName = user.Name,
                message,
                echo = true
            };

            await Clients.Caller.SendAsync("ReceivePrivateMessage", echo);

            // also send a compact private notification to the recipient so clients can surface badges
            await Clients.Client(targetConnectionId).SendAsync("PrivateNotification", new
            {
                senderId = user.ConnectionId,
                senderName = user.Name,
                preview = message
            });
        }

        // ---------- UTILITY ---------- //

        private async Task BroadcastClusterUpdate(string clusterId)
        {
            var members = _universe.GetClusterMembers(clusterId);

            if (members == null || members.Count == 0)
                return;

            var dto = members.Select(m => new
            {
                id = m.ConnectionId,
                name = m.Name,
                x = m.X,
                y = m.Y,
                clusterId = m.ClusterId
            }).ToList();

            await Clients.Group(clusterId).SendAsync("ClusterUpdate", dto);
        }

        private string GenerateCoolName()
        {
            var prefixes = new[] { "neo", "hyper", "sky", "lil", "big", "ghost", "x", "drip", "vibe", "glitch", "omega", "nano", "zero" };
            var adjectives = new[] { "chill", "savage", "dope", "wavy", "lit", "snazzy", "quirky", "spooky", "rad", "slick", "frosty" };
            var nouns = new[] { "pixel", "ninja", "panda", "rider", "vortex", "ghost", "comet", "mango", "wizard", "droid", "burrito" };

            string p = prefixes[Random.Shared.Next(prefixes.Length)];
            string a = adjectives[Random.Shared.Next(adjectives.Length)];
            string n = nouns[Random.Shared.Next(nouns.Length)];
            int num = Random.Shared.Next(1, 999);

            // Examples: neo-chill_panda, x_wavywizard42, drip-litcomet
            return $"{p}-{a}{n}{num}";
        }
    }
}
