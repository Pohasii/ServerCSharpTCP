using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

public struct NetworkEvent
{
    public ClientObject sender;
    public string message;
}

public class Server
{
    TcpListener tcpListener;
    Dictionary<int, ClientObject> clients = new Dictionary<int, ClientObject>();

    int clientsId;

    Dictionary<int, Action<NetworkEvent>> networkEvents = new Dictionary<int, Action<NetworkEvent>>();

    void OnOtherPlayerConected(NetworkEvent e)
    {
        var client = e.sender;

        client.UserName = e.message;

        var msg = client.UserName + " connected";
        Console.WriteLine(client.Id + " " + msg);
        BroadcastMessage(0, msg, client.Id);
    }

    public async void Start()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Any, 55443);
            tcpListener.Start();
            Console.WriteLine("Server started...");

            new Authentication(this);
            new Chat(this);
            On(0, OnOtherPlayerConected);

            while (true)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();

                var clientObject = new ClientObject(clientsId, tcpClient, this);


                Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                clientThread.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Disconnect();
        }
    }

    public void AddConnection (ClientObject client) {
        clients.Add(clientsId, client);
        clientsId++;
    }

    public void RemoveConnection(int id)
    {
        clients.Remove(id);
    }

    public void On(int type, Action<NetworkEvent> action)
    {
        if (networkEvents.TryGetValue(type, out var act))
        {
            act += action;
        }
        else
        {
            networkEvents.Add(type, action);
        }
    }

    public void Off(int type, Action<NetworkEvent> action)
    {
        if (networkEvents.TryGetValue(type, out var act))
        {
            act -= action;
        }
    }

    public void Send(int type, string message, int id)
    {
        var msg = ConstructMessage(type, message);

        byte[] data = Encoding.Unicode.GetBytes(msg);

        clients[id].Stream.Write(data, 0, data.Length);
    }

    public void BroadcastMessage(int type, string message, int id)
    {
        var msg = ConstructMessage(type, message);

        byte[] data = Encoding.Unicode.GetBytes(msg);

        foreach (var item in clients)
        {
            if (item.Key != id)
            {
                item.Value.Stream.Write(data, 0, data.Length);
            }
        }
    }

    public void ReciveMessage(ClientObject client, string message)
    {
        var (type, msg) = DeConstructMessage(message);

        if (networkEvents.TryGetValue(type, out var action))
        {
            var netEvent = new NetworkEvent { sender = client, message = msg };
            action.Invoke(netEvent);
        }
    }

    (int type, string msg) DeConstructMessage(string msg)
    {
        var splitMsg = msg.Split(new[] { ',' });

        var type = int.Parse(splitMsg[0]);
        var message = splitMsg[1];

        return (type, message);
    }

    string ConstructMessage(int type, string message)
    {
        return type + "," + message;
    }

    public void Disconnect()
    {
        tcpListener.Stop();

        for (int i = 0; i < clients.Count; i++)
        {
            clients[i].Close();
        }
        Environment.Exit(0);
    }
}