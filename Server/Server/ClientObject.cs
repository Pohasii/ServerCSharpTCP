using System;
using System.Net.Sockets;
using System.Text;

public class ClientObject
{
  
    public NetworkStream Stream { get; private set; }
    TcpClient client;
    Server server;

    public int Id { get; private set; }
    public string UserName { get; set; }

    public ClientObject(int id, TcpClient tcpClient, Server serverObject)
    {
        Id = id;
        client = tcpClient;
        server = serverObject;
        Stream = client.GetStream();
    }

    public void Process()
    {
        try
        {
            while (true)
            {
                try
                {
                    var message = GetMessage();

                    server.ReciveMessage(this, message);
                }
                catch
                {
                    var message = string.Format("{0}: leave the chat", UserName);
                    Console.WriteLine(message);
                    server.BroadcastMessage(1, message, Id);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            server.RemoveConnection(Id);
            Close();
        }
    }

    string GetMessage()
    {
        byte[] data = new byte[56];
        StringBuilder builder = new StringBuilder();
        int bytes = 0;
        do
        {
            bytes = Stream.Read(data, 0, data.Length);
            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
        }
        while (Stream.DataAvailable);

        return builder.ToString();
    }

    public void Close()
    {
        if (Stream != null)
            Stream.Close();
        if (client != null)
            client.Close();
    }
}