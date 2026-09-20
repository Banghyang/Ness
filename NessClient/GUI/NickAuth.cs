using NessClient.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NessClient.GUI
{
    public partial class NickAuth : Form
    {
        private ConfigImport _config;
        public NickAuth(ConfigImport config)
        {
            InitializeComponent();

            this.Text = "Authorization";

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            _config = config;
            txtNickname.KeyDown += txtNickname_KeyDown;
        }

        private void txtNickname_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TryConnect();
                e.SuppressKeyPress = true;
            }
        }

        private void TryConnect()
        {
            if (string.IsNullOrWhiteSpace(txtNickname.Text))
            {
                lblError.Text = "Введите никнейм!";
                return;
            }
            if (txtNickname.Text.Length > 12)
            {
                lblError.Text = "Слишком длинный никнейм!";
                return;
            }

            _config.NickName = txtNickname.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void NickAuth_Load(object sender, EventArgs e)
        {

        }
    }
}
