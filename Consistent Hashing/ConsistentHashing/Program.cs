public class Program
{
    public static void Main(string[] args)
    {
        List<string> servers = new List<string> { "S1", "S2", "S3" };
        var ring = new ConsistentHashRing(servers, 4);

        Console.WriteLine("user:123 -> " + ring.GetServer("user:123"));
        Console.WriteLine("order:987 -> " + ring.GetServer("order:987"));

        ring.AddServer("S5");
        Console.WriteLine("After adding S5:");
        Console.WriteLine("user:123 -> " + ring.GetServer("user:123"));

        ring.RemoveServer("S2");
        Console.WriteLine("After removing S2:");
        Console.WriteLine("order:987 -> " + ring.GetServer("order:987"));
    }
}