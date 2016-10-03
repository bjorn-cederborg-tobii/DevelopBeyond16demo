using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{

    const int TcpPortNo = 5940;
    static string TcpServerIp = "127.0.0.1";

    public static void Main()
    {
        
        Console.WriteLine("Please pick either \"server\" (s) or \"client\" (c)");

        string arg = "";
        while (arg != "q")
        {
            arg = Console.ReadKey().KeyChar.ToString();
            Console.WriteLine();
            if (arg == "s")
            {
                Console.WriteLine("Starting echo server...");
                try
                {
                    Console.WriteLine("Starting client..., please input server adress");
                    TcpServerIp = Console.ReadLine();
                    StartServer();
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed starting server, already started?");
                    throw;
                }
            }
            else if (arg == "c")
            {
                Console.WriteLine("Starting client..., please input server adress");
                TcpServerIp = Console.ReadLine();
                StartClient();
            }
            else
            {
                Console.WriteLine("invalid argument, try \"client\" or \"server\" (or \"quit\")");
            }
        }
    }

    class RemoteClient
    {
        public readonly StreamWriter Writer;
        public readonly string Name;

        public RemoteClient(string name, StreamWriter writer)
        {
            Name = name;
            Writer = writer;
        }
    }

    static List<RemoteClient> clientList = new List<RemoteClient>();


    static void StartServer()
    {
        TcpListener listener = new TcpListener(IPAddress.Parse(TcpServerIp), TcpPortNo);
        Console.WriteLine("Local server started on address " + listener.LocalEndpoint.ToString());

        listener.Start();

        while (true)
        {
            Task<TcpClient> client;
            client = listener.AcceptTcpClientAsync();
            client.Wait();
            Task connectionTask = Task.Run(() =>
            {
                NetworkStream stream = client.Result.GetStream();
                StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

                StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                Console.WriteLine("New connection from " + ((IPEndPoint)client.Result.Client.RemoteEndPoint).Address.ToString());

                writer.WriteLine("Welcome! Please enter your name:");
                string clientName = reader.ReadLine();
                clientList.Add(new RemoteClient(clientName, writer));
                writer.WriteLine("Hello " + clientName);
                writer.WriteLine("There are " + (clientList.Count - 1) + " other clients connected");
                foreach (var item in clientList.Where(x => x.Name != clientName))
                {
                    writer.WriteLine(item.Name);
                }
                clientList.Where(x => x.Name != clientName).ToList().ForEach(x => x.Writer.WriteLine(clientName + " connected"));
                while (true)
                {
                    string inputLine = "";
                    while (inputLine != null)
                    {
                        inputLine = reader.ReadLine();
                        string msg = clientName + " : " + inputLine;

                        //TODO: make threadsafe
                        clientList.Where(x => x.Name != clientName).ToList().ForEach(x => x.Writer.WriteLine(msg));
                        Console.WriteLine(msg);
                    }
                    Console.WriteLine("Server saw disconnect from " + clientName);
                    break;
                }
            });
        }
    }



    static void StartClient()
    {

        Console.WriteLine("Starting client and waiting for server...");

        int port = TcpPortNo;
        TcpClient client = new TcpClient();
        Task tsk = client.ConnectAsync(TcpServerIp, port);
        tsk.Wait();
        NetworkStream stream = client.GetStream();
        StreamReader reader = new StreamReader(stream);
        StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

        //Setup name
        Console.WriteLine(reader.ReadLine());
        string name = Console.ReadLine();
        writer.WriteLine(name);

        Task readTask = Task.Run(() =>
        {
            while (true)
            {
                string lineReceived = reader.ReadLine();
                //clear name for incoming message
                Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
                Console.WriteLine(lineReceived);
                Console.Write(name + ": ");
            }
        });

        while (true)
        {
            Console.Write(name + ": ");
            string lineToSend = Console.ReadLine();
            //Console.WriteLine("Sending to server: " + lineToSend);
            writer.WriteLine(lineToSend);

        }
    }
}
