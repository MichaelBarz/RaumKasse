using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alchemy;
using Alchemy.Classes;
using System.Net;
using System.Collections.Concurrent;

namespace RaumKasse
{
    public class AppServer
    {
        private WebSocketServer server;
        private ConcurrentDictionary<EndPoint, AppUser> appUser;

        public AppServer(int port)
        {
            this.appUser = new ConcurrentDictionary<EndPoint, AppUser>();

            this.server = new WebSocketServer(port, IPAddress.Any)
            {
                OnReceive = OnReceive,
                OnSend = OnSend,
                OnConnected = OnConnected,
                OnDisconnect = OnDisconnect,
                TimeOut = new TimeSpan(0, 5, 0)
            };
            this.server.Start();
        }

        private void OnConnected(UserContext context)
        {
            Console.WriteLine("Client Connected From: " + context.ClientAddress);
            var user = new AppUser { Context = context };
            appUser.TryAdd(context.ClientAddress, user);
        }

        private void OnReceive(UserContext context)
        {
            Console.WriteLine("Received Data From: " + context.ClientAddress);
            Console.WriteLine(context.DataFrame.ToString());
        }

        private void OnSend(UserContext context)
        {
            Console.WriteLine("Data Sent To: " + context.ClientAddress);
        }

        private void OnDisconnect(UserContext context)
        {
            Console.WriteLine("Client Disconnected: " + context.ClientAddress);
            AppUser removedUser;
            appUser.TryRemove(context.ClientAddress, out removedUser);
        }

        public void Stop()
        {
            this.server.Stop();
        }

        public class AppUser
        {
            public string Name = String.Empty;
            public UserContext Context { get; set; }
        }
    }
}
