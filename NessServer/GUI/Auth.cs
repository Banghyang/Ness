using NessServer.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NessServer.GUI
{
    public partial class Auth : Form
    {
        private ConfigImport _config;
        public Auth(ConfigImport config)
        {
            InitializeComponent();

            this.Text = "Authorization";

            _config = config;
            txtPassword.KeyDown += txtPassword_KeyDown;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TryConnect();
                e.SuppressKeyPress = true;
            }
        }

        private void TryConnect()
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblError.Location = new Point(115, 144);
                lblError.Text = "Введите пароль!";
                return;
            }

            if (txtPassword.Text.Length < 12)
            {
                lblError.Location = new Point(45, 144);
                lblError.Text = "Пароль должен быть минимум 12 символов!";
                return;
            }

            var crypt = new Crypt();
            _config.ServerPassword = txtPassword.Text;
            _config.Crypt = crypt;

            this.DialogResult = DialogResult.OK;
            this.Close();

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

    }
}
