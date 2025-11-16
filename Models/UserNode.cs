namespace GravityChat.Models
{
    public class UserNode
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        // Coordinates
        public int X { get; set; }
        public int Y { get; set; }

        // Cluster ID
        public string ClusterId { get; set; } = string.Empty;
    }
}
