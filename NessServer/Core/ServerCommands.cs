using NessServer.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NessServer.Core
{
    public class ServerCommands
    {
        private readonly ServerProcessing _server;
        private readonly ConfigImport _config;
        private readonly Action<string> _logAction;
        private readonly Dictionary<string, Action<ClientInfo, string[]>> _commands;

        public ServerCommands(ServerProcessing server, ConfigImport config, Action<string> log)
        {
            _server = server;
            _config = config;
            _logAction = log;

            _commands = new Dictionary<string, Action<ClientInfo, string[]>>(StringComparer.OrdinalIgnoreCase)
        {
            { "kick",  (c, args) => Kick(c, args) },
            { "ban",   (c, args) => Ban(c, args) },
            //{ "unban", (c, args) => Unban(c, args) },
            //{ "mute",  (c, args) => Mute(c, args) },
            //{ "unmute",(c, args) => Unmute(c, args) },
            //{ "clear", (c, args) => Clear(c, args) },
        };
        }

        public bool TryProcess(ClientInfo sender, string message)
        {
            if (!message.StartsWith("/")) return false;

            var parts = message.Substring(1).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return false;

            if (_commands.TryGetValue(parts[0], out var handler))
            {
                handler(sender, parts.Skip(1).ToArray());
                return true;
            }

            _logAction($"[Неизвестная команда: /{parts[0]}]");
            return true;
        }

        //Команды

        private void Kick(ClientInfo sender, string[] args)
        {
            if (args.Length < 1)
            {
                _logAction("[Использование: /kick <ник>]");
                return;
            }

            string target = args[0];

            var client = ClientInfo.GetAll().FirstOrDefault(c =>
            c.UserNickname == target ||
            c.ClientId == target);

            if (client == null)
            {
                _logAction($"[Kick: клиент '{target}' не найден]");
                return;
            }

            try
            {
                client.TcpClient.Close();
                _logAction($"[Kick: {target} отключён]");
            }
            catch (Exception ex)
            {
                _logAction($"[Kick: ошибка для {target}: {ex.Message}]");
            }
        }
        private void Ban(ClientInfo sender, string[] args)
        {
            string[] banlist = File.Exists("banlist.txt")
            ? File.ReadAllLines("banlist.txt")
            : new string[0];
            if (args.Length < 1)
            {
                _logAction("[Использование: /kick <ник>]");
                return;
            }

            string target = args[0];

            var client = ClientInfo.GetAll().FirstOrDefault(c =>
            c.UserNickname == target ||
            c.ClientId == target);

            if (client == null)
            {
                _logAction($"[Ban: клиент '{target}' не найден]");
                return;
            }

            string ip = client.ClientId.Split(":")[0];
            try
            {
                if (banlist.Contains(ip))
                {
                    _logAction($"[Ban: {target} уже забанен]");
                    client.TcpClient.Close();
                    return;
                }
                else
                {
                    File.AppendAllText("banlist.txt", $"\n{ip}");
                    client.TcpClient.Close();
                    _logAction($"[Ban: {target} ({ip}) забанен.]");
                }
            }
            catch (Exception ex)
            {
                _logAction($"[Ban: ошибка для {target}: {ex.Message}]");
            }
        }
    }
}
