using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;

public class ClientObject
{
    protected internal string Id { get; private set; }
    protected internal NetworkStream Stream { get; private set; }
    string userName;
    TcpClient client;
    Server server; 

    public ClientObject(TcpClient tcpClient, Server serverObject)
    {
        Id = Guid.NewGuid().ToString();
        client = tcpClient;
        server = serverObject;
        serverObject.AddConnection(this);
    }

    public void Process()
    {
        try
        {
            Stream = client.GetStream();
           
            string message = GetMessage();
            userName = message;

            message = userName + " join the chat";

            server.BroadcastMessage(message, this.Id);
            Console.WriteLine(message);

            while (true)
            {
                try
                {
                    message = GetMessage();
                    message = String.Format("{0}: {1}", userName, message);
                    Console.WriteLine(message);
                    server.BroadcastMessage(message, this.Id);
                }
                catch
                {
                    message = String.Format("{0}: leave the chat", userName);
                    Console.WriteLine(message);
                    server.BroadcastMessage(message, this.Id);
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
            // � ������ ������ �� ����� ��������� �������
            server.RemoveConnection(this.Id);
            Close();
        }
    }

    // ������ ��������� ��������� � �������������� � ������
    private string GetMessage()
    {
        byte[] data = new byte[256]; // ����� ��� ���������� ������
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

    // �������� �����������
    protected internal void Close()
    {
        if (Stream != null)
            Stream.Close();
        if (client != null)
            client.Close();
    }
}