namespace NessServer.GUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            lstUsers = new ListView();
            txtLog = new RichTextBox();
            EntryBox = new TextBox();
            userContextMenu = new ContextMenuStrip(components);
            kickToolStripMenuItem = new ToolStripMenuItem();
            banToolStripMenuItem = new ToolStripMenuItem();
            userContextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // lstUsers
            // 
            lstUsers.ContextMenuStrip = userContextMenu;
            lstUsers.Location = new Point(0, 29);
            lstUsers.Name = "lstUsers";
            lstUsers.Size = new Size(137, 421);
            lstUsers.TabIndex = 1;
            lstUsers.UseCompatibleStateImageBehavior = false;
            // 
            // txtLog
            // 
            txtLog.Location = new Point(143, 29);
            txtLog.Name = "txtLog";
            txtLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            txtLog.Size = new Size(656, 358);
            txtLog.TabIndex = 2;
            txtLog.Text = "";
            // 
            // EntryBox
            // 
            EntryBox.Location = new Point(158, 405);
            EntryBox.Name = "EntryBox";
            EntryBox.Size = new Size(621, 23);
            EntryBox.TabIndex = 3;
            EntryBox.KeyDown += EntryBox_KeyDown;
            // 
            // userContextMenu
            // 
            userContextMenu.Items.AddRange(new ToolStripItem[] { kickToolStripMenuItem, banToolStripMenuItem });
            userContextMenu.Name = "userContextMenu";
            userContextMenu.Size = new Size(181, 70);
            userContextMenu.Opening += contextMenuStrip1_Opening;
            // 
            // kickToolStripMenuItem
            // 
            kickToolStripMenuItem.Name = "kickToolStripMenuItem";
            kickToolStripMenuItem.Size = new Size(180, 22);
            kickToolStripMenuItem.Text = "Kick";
            kickToolStripMenuItem.Click += kickToolStripMenuItem_Click;
            // 
            // banToolStripMenuItem
            // 
            banToolStripMenuItem.Name = "banToolStripMenuItem";
            banToolStripMenuItem.Size = new Size(180, 22);
            banToolStripMenuItem.Text = "Ban";
            banToolStripMenuItem.Click += banToolStripMenuItem_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(EntryBox);
            Controls.Add(txtLog);
            Controls.Add(lstUsers);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            userContextMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ListView lstUsers;
        private RichTextBox txtLog;
        private TextBox EntryBox;
        private ContextMenuStrip userContextMenu;
        private ToolStripMenuItem kickToolStripMenuItem;
        private ToolStripMenuItem banToolStripMenuItem;
    }
}
