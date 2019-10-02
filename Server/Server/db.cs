using System;
using System.Collections.Generic;
using Newtonsoft.Json;

    public class DataBases {

    private Server server;
    const int trigger = 3;

        public HashSet<table> Tables = new HashSet<table> {
            new table {
                login = "test1",
                password = "test1"
            },
            new table {
                login = "test2",
                password = "test2"
            },
            new table {
                login = "test3",
                password = "test3"
            }
        };

        public DataBases(Server server) {
            this.server = server;
            server.On(trigger, asd);
        }

        public void asd(NetworkEvent e) {
            table auth = JsonConvert.DeserializeObject<table>(e.message);
            if (Check(auth)) {
                server.AddConnection(e.sender);
            } else {
                var sms = JsonConvert.SerializeObject(new { loging = false });
                server.Send(3, sms, e.sender.Id);
                e.sender.Close();
            }
        }

        public bool Check(table auth) {
            return Tables.TryGetValue(auth, out var table);
        }

    }

    public struct table {
        public string login;
        public string password;
    }
