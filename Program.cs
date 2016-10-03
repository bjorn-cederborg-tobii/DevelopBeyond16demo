using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    public static void Main()
    {
        Console.WriteLine("Enter s for server");
        if (Console.ReadLine() == "s")
            StartServer();
        StartClient();
    }
    static void StartServer()
    {
        TcpListener listener = new TcpListener(IPAddress.Parse("0.0.0.0"), 1234);
        listener.Start();
        var client = listener.AcceptTcpClientAsync();
        client.Wait();
        var reader = new StreamReader(client.Result.GetStream(), Encoding.ASCII);
        while (true)
        {
            Console.WriteLine(reader.ReadLine());
        }
    }
    static void StartClient()
    {
        TcpClient client = new TcpClient();
        Task tsk = client.ConnectAsync("localhost", 1234);
        tsk.Wait();
        StreamWriter writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
        while (true)
        {
            writer.WriteLine(Console.ReadLine());
        }
    }
}
