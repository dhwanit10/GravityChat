using GravityChat.Models;

namespace GravityChat.Services
{
    public class UniverseManager
    {
        private readonly Dictionary<string, UserNode> _users = new();
        private readonly Dictionary<string, Cluster> _clusters = new();

        private readonly object _lock = new();

        private const int VisibilityRadius = 150;

        // ---------- PUBLIC METHODS ---------- //

        public UserNode AddUser(string connectionId, string userName)
        {
            lock (_lock)
            {
                int universeSize = GetAdaptiveUniverseSize(_users.Count + 1);

                // Step 1: random spawn
                var user = new UserNode
                {
                    ConnectionId = connectionId,
                    Name = userName,
                    X = Random.Shared.Next(-universeSize, universeSize),
                    Y = Random.Shared.Next(-universeSize, universeSize)
                };

                // Step 2: Try to find nearby users
                var nearby = FindNearbyUsers(user);

                if (nearby.Any())
                {
                    // Join their cluster
                    var clusterId = _users[nearby.First()].ClusterId;

                    // Add user to lookup BEFORE modifying cluster members / recalculating center
                    _users[connectionId] = user;

                    AssignToCluster(user, clusterId);
                }
                else
                {
                    // Step 3: Find nearest cluster for gravity
                    var nearest = FindNearestCluster(user);

                    if (nearest != null)
                    {
                        ApplyGravity(user, nearest);

                        // Add user to lookup BEFORE modifying cluster members / recalculating center
                        _users[connectionId] = user;

                        AssignToCluster(user, nearest.ClusterId);
                    }
                    else
                    {
                        // Step 4: Create new cluster
                        CreateClusterForUser(user);

                        // For new cluster we can add user after creating cluster
                        _users[connectionId] = user;
                    }
                }

                return user;
            }
        }

        public void RemoveUser(string connectionId)
        {
            lock (_lock)
            {
                if (!_users.ContainsKey(connectionId))
                    return;

                var user = _users[connectionId];
                var clusterId = user.ClusterId;

                // SAFETY 1: If cluster still exists, clean up
                if (_clusters.ContainsKey(clusterId))
                {
                    var cluster = _clusters[clusterId];

                    cluster.MemberConnectionIds.Remove(connectionId);

                    // SAFETY 2: Remove dead IDs before recalculating
                    cluster.MemberConnectionIds = cluster.MemberConnectionIds
                        .Where(id => _users.ContainsKey(id))
                        .ToList();

                    // SAFETY 3: If cluster is empty, delete it
                    if (!cluster.MemberConnectionIds.Any())
                    {
                        _clusters.Remove(clusterId);
                    }
                    else
                    {
                        RecalculateClusterCenter(clusterId);
                    }
                }

                // SAFETY 4: Remove user last
                _users.Remove(connectionId);
            }
        }


        public List<UserNode> GetClusterMembers(string clusterId)
        {
            lock (_lock)
            {
                if (!_clusters.ContainsKey(clusterId))
                    return new();

                var cluster = _clusters[clusterId];

                var list = new List<UserNode>();

                foreach (var id in cluster.MemberConnectionIds.ToList())
                {
                    if (_users.ContainsKey(id))
                    {
                        list.Add(_users[id]);
                    }
                    else
                    {
                        // Clean invalid reference
                        cluster.MemberConnectionIds.Remove(id);
                    }
                }

                // If cluster becomes empty after cleanup, remove it
                if (!cluster.MemberConnectionIds.Any())
                {
                    _clusters.Remove(clusterId);
                    return new();
                }

                return list;
            }
        }


        public UserNode? GetUser(string connectionId)
        {
            lock (_lock)
            {
                return _users.TryGetValue(connectionId, out var user)
                    ? user
                    : null;
            }
        }


        public string? GetUserCluster(string connectionId)
        {
            lock (_lock)
            {
                return _users.ContainsKey(connectionId) ? _users[connectionId].ClusterId : null;
            }
        }

        // ---------- PRIVATE LOGIC ---------- //

        private int GetAdaptiveUniverseSize(int count)
        {
            if (count < 5) return 200;
            if (count < 15) return 500;
            if (count < 50) return 1000;
            return 2000;
        }

        private List<string> FindNearbyUsers(UserNode user)
        {
            return _users.Values
                .Where(u => Distance(u.X, u.Y, user.X, user.Y) <= VisibilityRadius)
                .Select(u => u.ConnectionId)
                .ToList();
        }

        private Cluster? FindNearestCluster(UserNode user)
        {
            if (!_clusters.Any()) return null;

            return _clusters.Values
                .OrderBy(c => Distance(c.CenterX, c.CenterY, user.X, user.Y))
                .First();
        }

        private void ApplyGravity(UserNode user, Cluster cluster)
        {
            int offsetX = Random.Shared.Next(-120, -60);
            int offsetY = Random.Shared.Next(60, 120);

            user.X = cluster.CenterX + offsetX;
            user.Y = cluster.CenterY + offsetY;
        }

        private void CreateClusterForUser(UserNode user)
        {
            string id = Guid.NewGuid().ToString("N");

            _clusters[id] = new Cluster
            {
                ClusterId = id,
                MemberConnectionIds = new() { user.ConnectionId },
                CenterX = user.X,
                CenterY = user.Y
            };

            user.ClusterId = id;
        }

        private void AssignToCluster(UserNode user, string clusterId)
        {
            var cluster = _clusters[clusterId];
            cluster.MemberConnectionIds.Add(user.ConnectionId);

            user.ClusterId = clusterId;

            RecalculateClusterCenter(clusterId);
        }

        private void RecalculateClusterCenter(string clusterId)
        {
            var cluster = _clusters[clusterId];
            var members = cluster.MemberConnectionIds.Select(id => _users[id]).ToList();

            cluster.CenterX = (int)members.Average(m => m.X);
            cluster.CenterY = (int)members.Average(m => m.Y);
        }

        private int Distance(int x1, int y1, int x2, int y2)
        {
            return (int)Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }
    }
}
