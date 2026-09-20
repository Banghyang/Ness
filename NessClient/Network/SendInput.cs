using NessClient.Core;
using System.IO;
using System.Net.Sockets;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NessClient.Network
{
    internal class SendInput
    {
        private readonly NetworkStream _stream;
        private string _message;
        private ConfigImport _config;
        public SendInput(NetworkStream stream, ConfigImport config)
        {
            _config = config;
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public async Task SendMessage(string message, string password)
        {
            if (_stream == null) throw new ArgumentNullException(nameof(_stream));
            if (string.IsNullOrWhiteSpace(message)) return;

            byte[] encrypted = _config.Crypt.EncryptMessage(message, password);

            byte[] lenBuf = BitConverter.GetBytes(encrypted.Length);
            byte[] frame = new byte[4 + encrypted.Length];

            Buffer.BlockCopy(lenBuf, 0, frame, 0, 4);
            Buffer.BlockCopy(encrypted, 0, frame, 4, encrypted.Length);

            await _stream.WriteAsync(frame, 0, frame.Length);
        }
    }

}

