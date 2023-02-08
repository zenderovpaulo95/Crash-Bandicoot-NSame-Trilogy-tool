namespace CBNSTT
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PackerBtn = new System.Windows.Forms.Button();
            this.TextEditBtn = new System.Windows.Forms.Button();
            this.TextureBtn = new System.Windows.Forms.Button();
            this.AboutBtn = new System.Windows.Forms.Button();
            this.SettingsBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // PackerBtn
            // 
            this.PackerBtn.Location = new System.Drawing.Point(32, 25);
            this.PackerBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.PackerBtn.Name = "PackerBtn";
            this.PackerBtn.Size = new System.Drawing.Size(104, 23);
            this.PackerBtn.TabIndex = 0;
            this.PackerBtn.Text = "Packer tool";
            this.PackerBtn.UseVisualStyleBackColor = true;
            this.PackerBtn.Click += new System.EventHandler(this.PackerBtn_Click);
            // 
            // TextEditBtn
            // 
            this.TextEditBtn.Location = new System.Drawing.Point(32, 62);
            this.TextEditBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TextEditBtn.Name = "TextEditBtn";
            this.TextEditBtn.Size = new System.Drawing.Size(106, 23);
            this.TextEditBtn.TabIndex = 1;
            this.TextEditBtn.Text = "Text Editor";
            this.TextEditBtn.UseVisualStyleBackColor = true;
            this.TextEditBtn.Click += new System.EventHandler(this.TextEditBtn_Click);
            // 
            // TextureBtn
            // 
            this.TextureBtn.Location = new System.Drawing.Point(187, 25);
            this.TextureBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TextureBtn.Name = "TextureBtn";
            this.TextureBtn.Size = new System.Drawing.Size(106, 23);
            this.TextureBtn.TabIndex = 3;
            this.TextureBtn.Text = "Texture Tool";
            this.TextureBtn.UseVisualStyleBackColor = true;
            this.TextureBtn.Click += new System.EventHandler(this.TextureBtn_Click);
            // 
            // AboutBtn
            // 
            this.AboutBtn.Location = new System.Drawing.Point(120, 101);
            this.AboutBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.AboutBtn.Name = "AboutBtn";
            this.AboutBtn.Size = new System.Drawing.Size(81, 23);
            this.AboutBtn.TabIndex = 5;
            this.AboutBtn.Text = "About";
            this.AboutBtn.UseVisualStyleBackColor = true;
            this.AboutBtn.Click += new System.EventHandler(this.AboutBtn_Click);
            // 
            // SettingsBtn
            // 
            this.SettingsBtn.Location = new System.Drawing.Point(187, 62);
            this.SettingsBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.SettingsBtn.Name = "SettingsBtn";
            this.SettingsBtn.Size = new System.Drawing.Size(106, 23);
            this.SettingsBtn.TabIndex = 6;
            this.SettingsBtn.Text = "Settings";
            this.SettingsBtn.UseVisualStyleBackColor = true;
            this.SettingsBtn.Click += new System.EventHandler(this.SettingsBtn_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 137);
            this.Controls.Add(this.SettingsBtn);
            this.Controls.Add(this.AboutBtn);
            this.Controls.Add(this.TextureBtn);
            this.Controls.Add(this.TextEditBtn);
            this.Controls.Add(this.PackerBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "Crash Bandicoot Tool";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button PackerBtn;
        private System.Windows.Forms.Button TextEditBtn;
        private System.Windows.Forms.Button TextureBtn;
        private System.Windows.Forms.Button AboutBtn;
        private System.Windows.Forms.Button SettingsBtn;
    }
}

