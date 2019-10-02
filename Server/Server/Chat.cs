using System;

public class Chat
{
    Server server;

    public Chat(Server server)
    {
        this.server = server;
        server.On(1, ReciveChatMessage);
    }

    void ReciveChatMessage(NetworkEvent e)
    {
        var client = e.sender;

        var msg = client.UserName + ": " + e.message;
        Console.WriteLine(client.Id + " " + msg);
        server.BroadcastMessage(1, msg, client.Id);
    }
}