using NessServer.Core;
using NessServer.Network;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NessServer.Network
{
    internal class ClientHandler
    {
        private readonly TcpClient _client;
        private readonly string _clientId;
        private readonly ConfigImport _config;
        private readonly object _lock = new object();
        private readonly Action<string> _logAction;
        private string _UserNickname;

        private DateTime _lastTypingSent = DateTime.MinValue;

        public ClientHandler(TcpClient client, string clientId, ConfigImport config, Action<string> logAction = null)
        {
            _clientId = clientId ?? throw new ArgumentNullException(nameof(client));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logAction = logAction ?? ((msg) => Console.WriteLine(msg));
            _config = config;
        }

        public async Task<bool> VerifyPassword(NetworkStream stream, string message, ClientInfo clientinformation)
        {

            if (message == $"AUTH:{_config.ServerPassword}")
            {
                _logAction?.Invoke($"[Клиент {_UserNickname} ({_clientId}) авторизован.]");
                BroadcastMessageAsync($"[Пользователь {_UserNickname} подключился]", clientinformation);
                return true;
            }
            else
            {
                _logAction($"[Неудачная авторизация с {_clientId}]");
                _logAction?.Invoke($"[Попытка: {message}]");
                return false;
            }
        }

        private bool BanCheck(ClientInfo clientinfo)
        {
            if (!File.Exists("banlist.txt")) return false;
            string ip = clientinfo.ClientId.Split(':')[0];
            return File.ReadAllLines("banlist.txt").Contains(ip);
        }

        public async Task StartHandlingAsync()
        {
            var clientInformation = new ClientInfo(_client, _clientId);
            if (BanCheck(clientInformation))
            {
                _logAction($"[Ban: Не так быстро, {clientInformation.ClientId.Split(":")[0]}]");
                _client.Close();
                return;
            }
            ClientInfo.Add(clientInformation);

            string id = clientInformation.ClientId;
            NetworkStream stream = clientInformation.Stream;
            bool connected = clientInformation.TcpClient.Connected;
            clientInformation.VerifyStatus = false;

            _ = Task.Run(async () =>
            {
                await Task.Delay(200); // 200 мс задержки
                await SendUserListAsync();
            });

            byte[] buffer = new byte[4096];

            try
            {
                while (_client.Connected)
                {

                    byte[] packet = await PacketCount(stream);
                    if (packet == null) break;

                    string decrypted = _config.Crypt.DecryptMessage(packet, _config.ServerPassword);

                    if (clientInformation.UserNickname == null && decrypted.StartsWith("AUTH2:"))
                    {
                        _UserNickname = decrypted.Substring(6).Trim();
                        clientInformation.UserNickname = _UserNickname;
                        continue;
                    }

                    if (clientInformation.VerifyStatus != true)
                    {
                        clientInformation.VerifyStatus = await VerifyPassword(stream, decrypted, clientInformation);
                        if (!clientInformation.VerifyStatus) break;
                        continue;
                    }

                    if (decrypted == "\u0001TYPING:")
                    {
                        if ((DateTime.Now - _lastTypingSent).TotalSeconds < 3) continue;  // игнор
                        _lastTypingSent = DateTime.Now;
                        await BroadcastMessageAsync("\u0001TYPING:" + clientInformation.UserNickname, clientInformation);
                        continue;
                    }

                    if (decrypted == "\u0001NOT_TYPING:")
                    {
                        await BroadcastMessageAsync("\u0001NOT_TYPING:" + clientInformation.UserNickname, clientInformation);
                        continue;
                    }

                    _logAction?.Invoke($"[{id}] {clientInformation.UserNickname}: {decrypted}");

                    await BroadcastMessageAsync($"{clientInformation.UserNickname}: {decrypted}", clientInformation);
                }
            }
            catch (Exception ex)
            {
                _logAction($"[Ошибка при чтении [{id}] {clientInformation.UserNickname}: {ex.Message}]");
            }
            finally
            {
                _client.Close();
                ClientInfo.Remove(clientInformation);
                await BroadcastMessageAsync($"[Пользователь {clientInformation.UserNickname} отключился]", clientInformation);
                await SendUserListAsync();
            }
        }

        private async Task BroadcastMessageAsync(string message, ClientInfo sender)
        {
            var clientsCopy = ClientInfo.GetAll();

            var deadClients = new List<ClientInfo>();

            byte[] encrypted = _config.Crypt.EncryptMessage(message, _config.ServerPassword);

            byte[] lenBuf = BitConverter.GetBytes(encrypted.Length);
            byte[] frame = new byte[4 + encrypted.Length];

            Buffer.BlockCopy(lenBuf, 0, frame, 0, 4);
            Buffer.BlockCopy(encrypted, 0, frame, 4, encrypted.Length);

            foreach (var client in clientsCopy)
            {
                if (client == sender) continue;

                try
                {
                    if (client.TcpClient.Connected && client.Stream.CanWrite)
                    {
                        if (!client.TcpClient.Connected)
                        {
                            deadClients.Add(client);
                            continue;
                        }

                        await client.Stream.WriteAsync(frame, 0, frame.Length);
                        await client.Stream.FlushAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logAction($"[Ошибка отправки клиенту {client.ClientId}: {ex.Message}]");
                    deadClients.Add(client);

                    foreach (var dead in deadClients)
                    {
                        ClientInfo.Remove(dead);
                        try { dead.TcpClient.Close(); } catch { }
                    }
                }
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
        private async Task SendUserListAsync()
        {
            var userIds = ClientInfo.GetAllNicknames();
            string userListMessage = "\u0001users " + string.Join(",", userIds);
            byte[] encrypted = _config.Crypt.EncryptMessage(userListMessage, _config.ServerPassword);

            byte[] lenBuf = BitConverter.GetBytes(encrypted.Length);
            byte[] frame = new byte[4 + encrypted.Length];

            Buffer.BlockCopy(lenBuf, 0, frame, 0, 4);
            Buffer.BlockCopy(encrypted, 0, frame, 4, encrypted.Length);

            var clients = ClientInfo.GetAll();
            foreach (var client in clients)
            {
                try
                {
                    if (client.TcpClient.Connected && client.Stream.CanWrite)
                    {
                        await client.Stream.WriteAsync(frame, 0, frame.Length);
                        await client.Stream.FlushAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logAction($"[Ошибка отправки списка {client.ClientId}: {ex.Message}]");
                }
            }
        }

    }
}
