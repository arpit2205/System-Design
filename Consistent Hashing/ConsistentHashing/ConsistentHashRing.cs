public class ConsistentHashRing
{
    private readonly int virtualNodes;
    private readonly SortedDictionary<int, string> ring = new SortedDictionary<int, string>();
    private readonly HashSet<string> servers = new HashSet<string>();

    public ConsistentHashRing(IEnumerable<string> servers, int virtualNodes)
    {
        if (virtualNodes <= 0)
        {
            throw new ArgumentException("virtualNodes must be positive");
        }

        this.virtualNodes = virtualNodes;

        foreach (var server in servers)
        {
            AddServer(server);
        }
    }

    public void AddServer(string server)
    {
        if (!servers.Add(server))
        {
            throw new InvalidOperationException("Server already exists");
        }

        for (int i = 0; i < virtualNodes; i++)
        {
            string virtualNodeKey = $"{server}-VN-{i}";
            int hash = GetHash(virtualNodeKey);
            ring[hash] = server;
        }
    }

    public void RemoveServer(string server)
    {
        if (!servers.Remove(server))
        {
            throw new InvalidOperationException("Server does not exist");
        }

        for (int i = 0; i < virtualNodes; i++)
        {
            string virtualNodeKey = $"{server}-VN-{i}";
            int hash = GetHash(virtualNodeKey);
            ring.Remove(hash);
        }
    }

    public string GetServer(string key)
    {
        if (ring.Count == 0)
        {
            throw new InvalidOperationException("Hash ring has no servers");
        }

        int hash = GetHash(key);

        foreach (var node in ring)
        {
            if (node.Key >= hash)
            {
                return node.Value;
            }
        }

        // Wrap around to the first server in the ring
        return ring.First().Value;
    }

    private int GetHash(string key)
    {
        return key.GetHashCode();
    }
}