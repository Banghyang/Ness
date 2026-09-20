using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NessServer.Network
{
    public class ClientInfo
    {
        public TcpClient TcpClient { get; }
        public NetworkStream Stream { get; }
        public string ClientId { get; }

        public bool VerifyStatus { get; set; }

        public string UserNickname { get; set; }

        public static event Action<string> OnClientAdded;
        public static event Action<string> OnClientRemoved;

        private static readonly List<ClientInfo> _clients = new List<ClientInfo>();
        private static readonly object _lock = new object();

        public ClientInfo(TcpClient client, string clientId)
        {
            TcpClient = client ?? throw new ArgumentNullException(nameof(client));
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            Stream = client.GetStream();
        }

        public static void Add(ClientInfo client)
        {
            lock (_lock)
            {
                _clients.Add(client);
                OnClientAdded?.Invoke(client.ClientId);
            }
        }
        public static void Remove(ClientInfo client)
        {
            lock (_lock)
            {
                _clients.Remove(client);
                OnClientRemoved?.Invoke(client.ClientId);
            }
        }

        public static List<ClientInfo> GetAll()
        {
            lock (_lock)
            {
                return _clients.ToList();
            }
        }
        public static List<string> GetAllIds()
        {
            lock (_lock)
            {
                return _clients.Select(c => c.ClientId).ToList();
            }
        }

        public static List<string> GetAllNicknames()
        {
            lock (_lock)
            {
                return _clients.Select(c => c.UserNickname).ToList();
            }
        }
    }
}
