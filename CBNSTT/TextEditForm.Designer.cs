namespace CBNSTT
{
    partial class TextEditForm
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
            this.ExportBtn = new System.Windows.Forms.Button();
            this.ImportBtn = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.removeTextCB = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // ExportBtn
            // 
            this.ExportBtn.Location = new System.Drawing.Point(29, 12);
            this.ExportBtn.Name = "ExportBtn";
            this.ExportBtn.Size = new System.Drawing.Size(154, 43);
            this.ExportBtn.TabIndex = 0;
            this.ExportBtn.Text = "Export text";
            this.ExportBtn.UseVisualStyleBackColor = true;
            this.ExportBtn.Click += new System.EventHandler(this.ExportBtn_Click);
            // 
            // ImportBtn
            // 
            this.ImportBtn.Location = new System.Drawing.Point(862, 12);
            this.ImportBtn.Name = "ImportBtn";
            this.ImportBtn.Size = new System.Drawing.Size(143, 43);
            this.ImportBtn.TabIndex = 1;
            this.ImportBtn.Text = "Import text";
            this.ImportBtn.UseVisualStyleBackColor = true;
            this.ImportBtn.Click += new System.EventHandler(this.ImportBtn_Click);
            // 
            // listBox1
            // 
            this.listBox1.AllowDrop = true;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 16;
            this.listBox1.Location = new System.Drawing.Point(13, 74);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(1024, 436);
            this.listBox1.TabIndex = 2;
            // 
            // removeTextCB
            // 
            this.removeTextCB.AutoSize = true;
            this.removeTextCB.Location = new System.Drawing.Point(348, 23);
            this.removeTextCB.Name = "removeTextCB";
            this.removeTextCB.Size = new System.Drawing.Size(358, 21);
            this.removeTextCB.TabIndex = 3;
            this.removeTextCB.Text = "Удалить текстовые файлы после импорта текста";
            this.removeTextCB.UseVisualStyleBackColor = true;
            // 
            // TextEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1049, 521);
            this.Controls.Add(this.removeTextCB);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.ImportBtn);
            this.Controls.Add(this.ExportBtn);
            this.MaximizeBox = false;
            this.Name = "TextEditForm";
            this.Text = "Text editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ExportBtn;
        private System.Windows.Forms.Button ImportBtn;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.CheckBox removeTextCB;
    }
}