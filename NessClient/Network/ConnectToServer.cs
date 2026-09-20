using NessClient.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using MarkdownToRtf;

namespace NessClient.Network
{
    internal class ConnectToServer
    {
        private TcpClient _tcpClient;
        public NetworkStream _stream;
        private readonly string _ip;
        private readonly int _port;

        public event Action<string[]> UserListUpdated;

        private System.Windows.Forms.Timer _reconnectTimer;

        private readonly Action<string> _logAction;

        int reconnectIntervalMs = 3000;

        private ConfigImport _config;

        public event Action<string> MessageReceived;
        public event Action ClearChatRequested;

        public event Action<string> TypingStarted;
        public event Action<string> TypingStopped;

        private DateTime _lastTypingSent = DateTime.MinValue;


        public  ConnectToServer(ConfigImport config, Action<string> logAction = null)
        {
            _config = config;
            _ip = config.Ip;
            _port = config.Port;
            _logAction = logAction ?? ((msg) => Console.WriteLine(msg));
            _tcpClient = new TcpClient();
            _reconnectTimer = new System.Windows.Forms.Timer();
            _reconnectTimer.Interval = reconnectIntervalMs;
            _reconnectTimer.Tick += ReconnectTimer_Tick;
            Connection();
        }
        public NetworkStream GetStream() => _stream;

        public async Task Connection()
        {
            try
            {
                _tcpClient.Connect(_ip, _port);
                _stream = _tcpClient.GetStream();
                ClearChatRequested?.Invoke();
                await SendPaswword();
                await ReadingServer();
            }
            catch(Exception ex)
            {
                _logAction($"[Не удалось подключиться. Повтор через {_reconnectTimer.Interval / 1000} сек...]");
                _reconnectTimer.Start();
            }
        }

        private async void ReconnectTimer_Tick(object sender, EventArgs e)
        {
            _reconnectTimer.Stop();
            _logAction("[Попытка переподключения...]");
            await Connection();
        }

        private async Task SendPaswword()
        {
            SendInput _sendInput = new SendInput(_stream, _config);
            await _sendInput.SendMessage($"AUTH2:{_config.NickName}", _config.ServerPassword);
            await _sendInput.SendMessage($"AUTH:{_config.ServerPassword}", _config.ServerPassword);
        }
        public async Task ReadingServer()
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (_tcpClient.Connected)
                {
                    byte[] packet = await PacketCount(_stream);
                    if (packet == null) break;

                    string decrypted = _config.Crypt.DecryptMessage(packet, _config.ServerPassword);
                    if (decrypted.StartsWith("\u0001users "))
                    {
                        string idsPart = decrypted.Substring(7).Trim();
                        var userIds = idsPart.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        UserListUpdated?.Invoke(userIds);
                    }
                    else
                    {
                        MessageReceived?.Invoke(decrypted);
                    }
                }
            }
            catch (Exception ex)
            {
                _logAction?.Invoke($"[{ex}]");
            }
            finally
            {
                _reconnectTimer?.Stop();
                _reconnectTimer?.Dispose();
                _tcpClient?.Close();
                _stream?.Close();
                _logAction($"[Клиент отключен]");
            }
        }
        private async Task<byte[]> PacketCount(NetworkStream stream)
        {
            const int MIN_PACKET_SIZE = 49;
            const int MAX_PACKET_SIZE = 4096;

            byte[] lenBuf = new byte[4];
            int read = 0;
            while (read < 4)
            {
                int r = await stream.ReadAsync(lenBuf, read, 4 - read);
                if (r == 0)
                {
                    _logAction($"[Клиент закрыл соединение при чтении длины.]");
                    return null;
                }
                read += r;
            }

            int packetLength = BitConverter.ToInt32(lenBuf, 0);

            if (packetLength < MIN_PACKET_SIZE || packetLength > MAX_PACKET_SIZE)
            {
                _logAction($"[Некорректный размер пакета: {packetLength} байт. (мин: {MIN_PACKET_SIZE}, макс: {MAX_PACKET_SIZE})]");
                return null;
            }

            byte[] packetData = new byte[packetLength];
            int totalRead = 0;
            while (totalRead < packetLength)
            {
                int r = stream.Read(packetData, totalRead, packetLength - totalRead);
                if (r == 0)
                {
                    Console.WriteLine($"[Клиент закрыл соединение при чтении тела пакета.]");
                    return null;
                }
                totalRead += r;
            }

            return packetData;
        }
    }
}
