using NessServer.Core;
using NessServer.Network;
using System.Data.Common;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace NessServer.GUI
{
    public partial class Form1 : Form
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        private ServerProcessing _server;
        private readonly ConfigImport _config;

        private ContextMenuStrip _userContextMenu;
        private ToolStripMenuItem _menuKick;
        private ToolStripMenuItem _menuBan;

        private ServerCommands _serverCommands;

        public Form1(ConfigImport config)
        {
            InitializeComponent();

            this.Text = "Loch Server";

            _config = config;

            //Конфигурация основного окна
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            //Конфигурация чата
            txtLog.ReadOnly = true;
            txtLog.BackColor = Color.FromArgb(30, 30, 30);
            txtLog.ForeColor = Color.LightGreen;
            txtLog.Font = new System.Drawing.Font("Consolas", 10);
            txtLog.BorderStyle = BorderStyle.None;

            txtLog.HandleCreated += (s, e) =>
            {
                // "DarkMode_Explorer" заставляет Windows рисовать тёмный скроллбар
                SetWindowTheme(txtLog.Handle, "DarkMode_Explorer", null);
            };

            //Эвенты списка пользователей
            ClientInfo.OnClientAdded += OnClientAdded;
            ClientInfo.OnClientRemoved += OnClientRemoved;

            //Конфигурация списка пользователей
            lstUsers.BackColor = Color.FromArgb(30, 30, 30);
            lstUsers.ForeColor = Color.LightGreen;
            lstUsers.Font = new System.Drawing.Font("Consolas", 10);
            lstUsers.View = View.Details;
            lstUsers.FullRowSelect = true;
            lstUsers.MultiSelect = false;
            lstUsers.Scrollable = true;
            lstUsers.Columns.Clear();
            lstUsers.Columns.Add("", -2);
            lstUsers.HeaderStyle = ColumnHeaderStyle.None;

            EntryBox.BackColor = Color.FromArgb(30, 30, 30);
            EntryBox.ForeColor = Color.LightGreen;
            EntryBox.Font = new System.Drawing.Font("Consolas", 10);

            //Контекстное меню
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                _server = new ServerProcessing(_config, msg => AddLog(msg));

                _ = Task.Run(() => _server.StartAsync());

                _serverCommands = _server.Commands;

                AddLog("[Сервер работает и ожидает клиентов.]");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[Ошибка при запуске: {ex.Message}\n\nДетали:\n{ex.StackTrace}]",
                                "Ошибка запуска", MessageBoxButtons.OK, MessageBoxIcon.Error);

                AddLog($"[КРИТИЧЕСКАЯ ОШИБКА: {ex.Message}]", true);
            }
        }

        private void EntryBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    string message = EntryBox.Text.Trim();

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        if (message.Length > 1024)
                        {
                            AddLog("[Слишком большое сообщение!]");
                        }
                        if (message.StartsWith("/"))
                        {
                            // Это команда — отдаём в ServerCommands
                            // sender = null → команда "от сервера", прав нет, всё разрешено
                            _serverCommands.TryProcess(null, message);
                            AddLog($"[Команда: {message}]");
                            EntryBox.Clear();
                        }
                        else
                        {
                            AddLog($"[Server] {message}");
                            EntryBox.Clear();
                        }
                    }

                    e.SuppressKeyPress = true;
                }
                catch (Exception ex)
                {
                    AddLog($"{ex}");
                }
            }
        }

        private void AddLog(string message, bool isError = false)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => AddLog(message, isError)));
                return;
            }

            string time = DateTime.Now.ToString("HH:mm:ss");

            txtLog.AppendText($"[{time}] {message}\r\n");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void OnClientAdded(string clientId)
        {
            if (lstUsers.InvokeRequired)
            {
                lstUsers.Invoke(() => OnClientAdded(clientId));
                return;
            }

            lstUsers.Items.Add(clientId);

        }

        private void OnClientRemoved(string clientId)
        {

            if (lstUsers.InvokeRequired)
            {
                lstUsers.Invoke(() => OnClientRemoved(clientId));
                return;
            }

            foreach (ListViewItem item in lstUsers.Items)
            {
                if (item.Text == clientId)
                {
                    lstUsers.Items.Remove(item);
                    return;
                }
            }
        }

        private void darkTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {

        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (lstUsers.SelectedItems.Count == 0)
                e.Cancel = true;
        }

        private void kickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItems.Count == 0) return;
            string nick = lstUsers.SelectedItems[0].Text;

            _serverCommands.TryProcess(null, $"/kick {nick}");
            AddLog($"[Kick: {nick}]");
        }

        private void banToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItems.Count == 0) return;
            string nick = lstUsers.SelectedItems[0].Text;

            var confirm = MessageBox.Show(
                $"Забанить {nick}?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            _serverCommands.TryProcess(null, $"/ban {nick}");
            AddLog($"[Ban: {nick}]");
        }
    }
}