using System;
using System.Windows.Forms;
using NessServer.Core;
using NessServer.GUI;

namespace NessServer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            var config = new ConfigImport();

            ApplicationConfiguration.Initialize();

            using (Auth loginForm = new Auth(config))
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