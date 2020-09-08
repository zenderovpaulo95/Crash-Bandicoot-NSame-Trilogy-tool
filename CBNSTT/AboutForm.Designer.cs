namespace CBNSTT
{
    partial class AboutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.aboutRichTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // aboutRichTextBox
            // 
            this.aboutRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.aboutRichTextBox.DetectUrls = false;
            this.aboutRichTextBox.Location = new System.Drawing.Point(12, 12);
            this.aboutRichTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.aboutRichTextBox.Name = "aboutRichTextBox";
            this.aboutRichTextBox.ReadOnly = true;
            this.aboutRichTextBox.Size = new System.Drawing.Size(425, 390);
            this.aboutRichTextBox.TabIndex = 0;
            this.aboutRichTextBox.Text = resources.GetString("aboutRichTextBox.Text");
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(449, 412);
            this.Controls.Add(this.aboutRichTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "AboutForm";
            this.Text = "About tool";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox aboutRichTextBox;
    }
}