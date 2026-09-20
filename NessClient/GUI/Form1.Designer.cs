namespace NessClient
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
            txtLog = new RichTextBox();
            EntryBox = new TextBox();
            lstUsers = new ListView();
            textingBox = new TextBox();
            SuspendLayout();
            // 
            // txtLog
            // 
            txtLog.Location = new Point(143, 41);
            txtLog.Name = "txtLog";
            txtLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            txtLog.Size = new Size(656, 346);
            txtLog.TabIndex = 0;
            txtLog.Text = "";
            txtLog.TextChanged += textBox1_TextChanged;
            // 
            // EntryBox
            // 
            EntryBox.Location = new Point(158, 405);
            EntryBox.Multiline = true;
            EntryBox.Name = "EntryBox";
            EntryBox.ReadOnly = true;
            EntryBox.Size = new Size(621, 23);
            EntryBox.TabIndex = 1;
            EntryBox.TextChanged += textBox1_TextChanged_1;
            EntryBox.KeyDown += EntryBox_KeyDown;
            // 
            // lstUsers
            // 
            lstUsers.Location = new Point(0, 41);
            lstUsers.Name = "lstUsers";
            lstUsers.Size = new Size(137, 411);
            lstUsers.TabIndex = 2;
            lstUsers.UseCompatibleStateImageBehavior = false;
            // 
            // textingBox
            // 
            textingBox.Location = new Point(143, 12);
            textingBox.Name = "textingBox";
            textingBox.Size = new Size(656, 23);
            textingBox.TabIndex = 3;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(textingBox);
            Controls.Add(lstUsers);
            Controls.Add(EntryBox);
            Controls.Add(txtLog);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox txtLog;
        private TextBox EntryBox;
        private ListView lstUsers;
        private TextBox textingBox;
    }
}
