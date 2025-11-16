namespace GravityChat.Models
{
    public class Cluster
    {
        public string ClusterId { get; set; } = string.Empty;
        public List<string> MemberConnectionIds { get; set; } = new();

        // Calculated center of the cluster
        public int CenterX { get; set; }
        public int CenterY { get; set; }
    }
}
