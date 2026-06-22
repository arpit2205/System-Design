using System.Security.Cryptography;
using System.Text;

public class ConsistentHashRingOptimised
{
    private readonly int virtualNodes;

    // Hash -> Server
    private readonly Dictionary<uint, string> ring = new();

    // Sorted hashes for binary search (Logn lookup for next clockwise node)
    private readonly List<uint> sortedHashes = new();

    // Server -> Virtual node hashes (Easy removal)
    private readonly Dictionary<string, List<uint>> serverToHashes = new();

    public ConsistentHashRingOptimised(IEnumerable<string> servers, int virtualNodes)
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
        if (serverToHashes.ContainsKey(server))
        {
            throw new InvalidOperationException($"Server '{server}' already exists.");
        }

        var hashes = new List<uint>();

        for (int i = 0; i < virtualNodes; i++)
        {
            string virtualNodeKey = $"{server}-VN-{i}";
            uint hash = GetHash(virtualNodeKey);

            // Simple collision handling
            while (ring.ContainsKey(hash))
            {
                hash++;
            }

            ring[hash] = server;
            hashes.Add(hash);
            sortedHashes.Add(hash);
        }

        sortedHashes.Sort();
        serverToHashes[server] = hashes;
    }

    public void RemoveServer(string server)
    {
        if (!serverToHashes.TryGetValue(server, out var hashes))
        {
            throw new InvalidOperationException($"Server '{server}' does not exist.");
        }

        foreach (uint hash in hashes)
        {
            ring.Remove(hash);
            sortedHashes.Remove(hash);
        }

        serverToHashes.Remove(server);
    }

    public string GetServer(string key)
    {
        if (ring.Count == 0)
        {
            throw new InvalidOperationException("Hash ring contains no servers.");
        }

        uint hash = GetHash(key);
        int index = sortedHashes.BinarySearch(hash);

        // Exact match not found
        if (index < 0)
        {
            index = ~index;
        }

        // Wrap around
        if (index == sortedHashes.Count)
        {
            index = 0;
        }

        uint serverHash = sortedHashes[index];
        return ring[serverHash];
    }

    private static uint GetHash(string key)
    {
        using var sha256 = SHA256.Create();

        byte[] bytes = sha256.ComputeHash(
            Encoding.UTF8.GetBytes(key));

        return BitConverter.ToUInt32(bytes, 0);
    }
}