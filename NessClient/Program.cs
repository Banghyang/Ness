using NessClient.Core;
using NessClient.GUI;
using System;
using System.Windows.Forms;

namespace NessClient
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Action<string> _logAction = null;
            _logAction = _logAction ?? ((msg) => Console.WriteLine(msg));
            var config = new ConfigImport();

            ApplicationConfiguration.Initialize();

            using (NickAuth loginForm = new NickAuth(config))
            {
                if (loginForm.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }

            using (Auth loginForm = new Auth(config, _logAction))
            {
                if (loginForm.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }

            Application.Run(new Form1(config));
        }
    }
}