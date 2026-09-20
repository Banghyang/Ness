using NessServer.Core;
using NessServer.Network;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NessServer.Network
{
    public class ServerProcessing
    {
        private readonly string _ip;
        private readonly int _port;
        private TcpListener _server;
        private bool _isRunning;
        private ConfigImport _config;
        private readonly Action<string> _logAction;

        public ServerCommands Commands { get; }
        public  ServerProcessing(ConfigImport config, Action<string> logAction = null)
        {
            _ip = config.Ip;
            _port = config.Port;
            _config = config;
            _logAction = logAction ?? ((msg) => Console.WriteLine(msg));

            Commands = new ServerCommands(this, config, _logAction);
        }
        public async Task StartAsync()
        {
            _isRunning = true;
            _server = new TcpListener(IPAddress.Parse(_ip), _port);

            while (_isRunning)
            {
                var clients = ClientInfo.GetAll();
                if (clients.Count >= _config.MaxClients)
                {
                    if (_server.Server.IsBound)
                    {
                        _logAction($"[Лимит {_config.MaxClients} достигнут. Приём новых подключений приостановлен]");
                        _server.Stop();
                    }
                }
                else
                {
                    _server.Start(5);
                    _logAction($"[Приём возобновлён]");
                }

                while (_isRunning && ClientInfo.GetAll().Count >= _config.MaxClients)
                {
                    await Task.Delay(1000);
                }

                if (!_isRunning) break;

                TcpClient _client;

                try
                {
                    _client = await _server.AcceptTcpClientAsync();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logAction($"{ex}");
                    break;
                }

                string _clientId = _client.Client.RemoteEndPoint?.ToString() ?? "Unknown";

                ClientHandler handler = new ClientHandler(_client, _clientId, _config, _logAction);
                _ = handler.StartHandlingAsync();
            }
        }

         public void Stop()
         {
            _isRunning = false;
            _server?.Stop();
         }
        
    }
}
