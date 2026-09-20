using NessClient.Core;
using NessClient.Network;
using MarkdownToRtf;
using System;
using System.Media;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using NessClient.Properties;

namespace NessClient
{
    public partial class Form1 : Form
    {
        //Цвет Скроллбара
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        //Настройка уведомлений
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        //Настройка мигания окна
        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        private const uint FLASHW_TRAY = 2;
        private const uint FLASHW_TIMERNOFG = 12;  // мигать, пока окно не станет активным

        private void FlashTaskbar()
        {
            var fi = new FLASHWINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(FLASHWINFO)),
                hwnd = this.Handle,
                dwFlags = FLASHW_TRAY | FLASHW_TIMERNOFG,
                uCount = uint.MaxValue,
                dwTimeout = 0
            };
            FlashWindowEx(ref fi);
        }

        private bool IsMyWindowActive()
        {
            IntPtr foreground = GetForegroundWindow();
            GetWindowThreadProcessId(foreground, out uint pid);
            return pid == (uint)Environment.ProcessId;
        }

        private readonly HashSet<string> _typingUsers = new HashSet<string>();

        private System.Windows.Forms.Timer _typingTimer;

        private ConnectToServer _connection;
        private TcpClient _client;
        private NetworkStream _stream;
        private bool _isConnected;
        private readonly Action<string> _logAction;
        private readonly ConfigImport _config;
        public event Action<string> MessageReceived;

        public Form1(ConfigImport config)
        {
            InitializeComponent();

            this.Text = "Loch";

            _config = config;
            //Конфигурация основного окна
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            //Конфигурация окна чата
            txtLog.ReadOnly = true;
            txtLog.BackColor = Color.FromArgb(30, 30, 30);
            txtLog.ForeColor = Color.LightGreen;
            txtLog.Font = new Font("Consolas", 10);
            txtLog.BorderStyle = BorderStyle.None;

            txtLog.HandleCreated += (s, e) =>
            {
                // "DarkMode_Explorer" заставляет Windows рисовать тёмный скроллбар
                SetWindowTheme(txtLog.Handle, "DarkMode_Explorer", null);
            };

            //Конфигурация списка пользователей
            lstUsers.BackColor = Color.FromArgb(30, 30, 30);
            lstUsers.ForeColor = Color.LightGreen;
            lstUsers.Font = new Font("Consolas", 10);
            lstUsers.View = View.Details;
            lstUsers.FullRowSelect = true;
            lstUsers.MultiSelect = false;
            lstUsers.Scrollable = true;
            lstUsers.Columns.Clear();
            lstUsers.Columns.Add("", -2);
            lstUsers.HeaderStyle = ColumnHeaderStyle.None;

            //Конфигурация поля ввода
            EntryBox.BackColor = Color.FromArgb(30, 30, 30);
            EntryBox.ForeColor = Color.LightGreen;
            EntryBox.Font = new Font("Consolas", 10);

            //Конфигурация поля "Печатает..."
            textingBox.ReadOnly = true;
            textingBox.BackColor = Color.FromArgb(30, 30, 30);
            textingBox.ForeColor = Color.LightGreen;
            textingBox.Font = new Font("Consolas", 10);
            textingBox.BorderStyle = BorderStyle.None;

            //Конфигурация таймера
            _typingTimer = new System.Windows.Forms.Timer();
            _typingTimer.Interval = 2000;   // 2 секунды
            _typingTimer.Tick += TypingTimer_Tick;
            _typingTimer.Start();

        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                _connection = new ConnectToServer(_config, msg => AddLog(msg));

                _connection.MessageReceived += OnMessageReceived;
                _connection.UserListUpdated += OnUserListUpdated;
                _connection.ClearChatRequested += OnClearChatRequested;

                _connection.TypingStarted += OnTypingStarted;
                _connection.TypingStopped += OnTypingStopped;

                txtLog.Clear();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении: {ex.Message}\n\nДетали:\n{ex.StackTrace}",
                                "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);

                AddLog($"КРИТИЧЕСКАЯ ОШИБКА: {ex.Message}", true);
            }
        }

        private void EntryBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (e.Shift)
                {
                    return;
                }
                string message = EntryBox.Text.Trim();

                if (!string.IsNullOrWhiteSpace(message))
                {
                    if (message.Length > 1024)
                    {
                        AddLog("[Слишком большое сообщение!]");
                    }
                    else
                    {
                        SendInput messageSender = new SendInput(_connection._stream, _config);
                        messageSender.SendMessage(message, _config.ServerPassword);
                        DisplayMessage($"You: {message}");
                        EntryBox.Clear();
                    }
                }

                e.SuppressKeyPress = true;
            }
        }

        private void UpdateTypingLabel()
        {
            if (textingBox.InvokeRequired)
            {
                textingBox.Invoke(new Action(UpdateTypingLabel));
                return;
            }

            List<string> users;
            lock (_typingUsers) { users = _typingUsers.ToList(); }

            if (users.Count == 0)
            {
                textingBox.Text = "";
                return;
            }

            if (users.Count == 1)
                textingBox.Text = $"{users[0]} печатает...";
            else if (users.Count == 2)
                textingBox.Text = $"{users[0]} и {users[1]} печатают...";
            else
                textingBox.Text = "Несколько человек печатают...";
        }

        private void OnTypingStarted(string nick)
        {
            lock (_typingUsers) { _typingUsers.Add(nick); }
            UpdateTypingLabel();
        }

        private void OnTypingStopped(string nick)
        {
            lock (_typingUsers) { _typingUsers.Remove(nick); }
            UpdateTypingLabel();
        }

        private void DisplayMessage(string message)
        {
            AppendFormattedMessage(message);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Пусто
        }

        private void OnUserListUpdated(string[] userIds)
        {
            UpdateUserList(userIds);
        }

        private void UpdateUserList(string[] userIds)
        {
            if (EntryBox.ReadOnly == true) EntryBox.ReadOnly = false;
            if (lstUsers.InvokeRequired)
            {
                lstUsers.Invoke(() => UpdateUserList(userIds));
                return;
            }

            lstUsers.Items.Clear();
            foreach (var id in userIds)
            {
                lstUsers.Items.Add(id);
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

        private void ClearChat()
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(() => ClearChat());
                return;
            }
            txtLog.Clear();
        }

        private void OnClearChatRequested()
        {
            ClearChat();
        }

        private void PlayNotifySound()
        {
            var player = new SoundPlayer(NessClient.Properties.Resources.Notification);
            player.Play();
        }

        private void OnMessageReceived(string message)
        {
            if (message.StartsWith("\u0001TYPING:"))
            {
                string nick = message.Substring(8).Trim();
                OnTypingStarted(nick);
                return;
            }

            if (message.StartsWith("\u0001NOT_TYPING:"))
            {
                string nick = message.Substring(12).Trim();
                OnTypingStopped(nick);
                return;
            }

            if (WindowState == FormWindowState.Minimized || !IsMyWindowActive())
            {
                PlayNotifySound();
                FlashTaskbar();
            }

            AppendFormattedMessage(message);
        }

        private void AppendFormattedMessage(string message)
            {
                try
                {
                    string rtf = MarkdownToRtfConverter.Convert(message);

                    if (string.IsNullOrWhiteSpace(rtf))
                    {
                        txtLog.AppendText(message + Environment.NewLine);
                        return;
                    }

                    if (txtLog.InvokeRequired)
                    {
                        txtLog.Invoke(() => InsertRtf(rtf));
                    }
                    else
                    {
                        InsertRtf(rtf);
                    }
                }
                catch (Exception ex)
                {
                    txtLog.AppendText(message + Environment.NewLine);
                }
            }

            private void InsertRtf(string rtf)
            {
                txtLog.SelectionStart = txtLog.TextLength;
                txtLog.SelectionLength = 0;

                txtLog.SelectedRtf = rtf;

                txtLog.ScrollToCaret();
            }

        private async void TypingTimer_Tick(object sender, EventArgs e)
        {
            if (_connection == null || _connection._stream == null) return;
            if (!_connection._stream.CanWrite) return;

            bool hasText = !string.IsNullOrWhiteSpace(EntryBox.Text);
            string status = hasText ? "\u0001TYPING:" : "\u0001NOT_TYPING:";

            try
            {
                SendInput sender2 = new SendInput(_connection._stream, _config);
                await sender2.SendMessage(status, _config.ServerPassword);
            }
            catch (Exception ex)
            {
                AddLog($"[Ошибка отправки статуса: {ex.Message}]");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _typingTimer?.Stop();
            _typingTimer?.Dispose();
            base.OnFormClosing(e);
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }
    }
}