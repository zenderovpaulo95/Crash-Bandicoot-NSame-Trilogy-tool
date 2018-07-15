namespace CBNSTT
{
    partial class FontEditorForm
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
            this.components = new System.ComponentModel.Container();
            this.textureDataGridView = new System.Windows.Forms.DataGridView();
            this.textureMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.экспортироватьТекстурыPVRToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.импортироватьТекстурыPVRToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CoordDataGridView = new System.Windows.Forms.DataGridView();
            this.coordsMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.импортироватьКоординатыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.экспортироватьКоординатыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.formMenuStrip = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.textureDataGridView)).BeginInit();
            this.textureMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CoordDataGridView)).BeginInit();
            this.coordsMenuStrip.SuspendLayout();
            this.formMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // textureDataGridView
            // 
            this.textureDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.textureDataGridView.ContextMenuStrip = this.textureMenuStrip;
            this.textureDataGridView.Location = new System.Drawing.Point(12, 50);
            this.textureDataGridView.Name = "textureDataGridView";
            this.textureDataGridView.RowTemplate.Height = 24;
            this.textureDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textureDataGridView.Size = new System.Drawing.Size(523, 150);
            this.textureDataGridView.TabIndex = 0;
            // 
            // textureMenuStrip
            // 
            this.textureMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.textureMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.экспортироватьТекстурыPVRToolStripMenuItem,
            this.импортироватьТекстурыPVRToolStripMenuItem});
            this.textureMenuStrip.Name = "textureMenuStrip";
            this.textureMenuStrip.Size = new System.Drawing.Size(297, 52);
            // 
            // экспортироватьТекстурыPVRToolStripMenuItem
            // 
            this.экспортироватьТекстурыPVRToolStripMenuItem.Name = "экспортироватьТекстурыPVRToolStripMenuItem";
            this.экспортироватьТекстурыPVRToolStripMenuItem.Size = new System.Drawing.Size(296, 24);
            this.экспортироватьТекстурыPVRToolStripMenuItem.Text = "Экспортировать текстуры (PVR)";
            this.экспортироватьТекстурыPVRToolStripMenuItem.Click += new System.EventHandler(this.экспортироватьТекстурыPVRToolStripMenuItem_Click);
            // 
            // импортироватьТекстурыPVRToolStripMenuItem
            // 
            this.импортироватьТекстурыPVRToolStripMenuItem.Name = "импортироватьТекстурыPVRToolStripMenuItem";
            this.импортироватьТекстурыPVRToolStripMenuItem.Size = new System.Drawing.Size(296, 24);
            this.импортироватьТекстурыPVRToolStripMenuItem.Text = "Импортировать текстуры (PVR)";
            this.импортироватьТекстурыPVRToolStripMenuItem.Click += new System.EventHandler(this.импортироватьТекстурыPVRToolStripMenuItem_Click);
            // 
            // CoordDataGridView
            // 
            this.CoordDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CoordDataGridView.ContextMenuStrip = this.coordsMenuStrip;
            this.CoordDataGridView.Location = new System.Drawing.Point(12, 224);
            this.CoordDataGridView.Name = "CoordDataGridView";
            this.CoordDataGridView.RowTemplate.Height = 24;
            this.CoordDataGridView.Size = new System.Drawing.Size(1238, 420);
            this.CoordDataGridView.TabIndex = 2;
            // 
            // coordsMenuStrip
            // 
            this.coordsMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.coordsMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.импортироватьКоординатыToolStripMenuItem,
            this.экспортироватьКоординатыToolStripMenuItem});
            this.coordsMenuStrip.Name = "coordsMenuStrip";
            this.coordsMenuStrip.Size = new System.Drawing.Size(281, 52);
            // 
            // импортироватьКоординатыToolStripMenuItem
            // 
            this.импортироватьКоординатыToolStripMenuItem.Name = "импортироватьКоординатыToolStripMenuItem";
            this.импортироватьКоординатыToolStripMenuItem.Size = new System.Drawing.Size(280, 24);
            this.импортироватьКоординатыToolStripMenuItem.Text = "Импортировать координаты";
            this.импортироватьКоординатыToolStripMenuItem.Click += new System.EventHandler(this.импортироватьКоординатыToolStripMenuItem_Click);
            // 
            // экспортироватьКоординатыToolStripMenuItem
            // 
            this.экспортироватьКоординатыToolStripMenuItem.Name = "экспортироватьКоординатыToolStripMenuItem";
            this.экспортироватьКоординатыToolStripMenuItem.Size = new System.Drawing.Size(280, 24);
            this.экспортироватьКоординатыToolStripMenuItem.Text = "Экспортировать координаты";
            // 
            // formMenuStrip
            // 
            this.formMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.formMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem});
            this.formMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.formMenuStrip.Name = "formMenuStrip";
            this.formMenuStrip.Size = new System.Drawing.Size(1262, 28);
            this.formMenuStrip.TabIndex = 3;
            this.formMenuStrip.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.quitToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(57, 24);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.openToolStripMenuItem.Text = "Открыть";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.saveToolStripMenuItem.Text = "Сохранить";
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.saveAsToolStripMenuItem.Text = "Сохранить как...";
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.quitToolStripMenuItem.Text = "Выйти";
            // 
            // FontEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1262, 673);
            this.Controls.Add(this.CoordDataGridView);
            this.Controls.Add(this.textureDataGridView);
            this.Controls.Add(this.formMenuStrip);
            this.MainMenuStrip = this.formMenuStrip;
            this.Name = "FontEditorForm";
            this.Text = "Font editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FontEditorForm_FormClosing);
            this.Load += new System.EventHandler(this.FontEditorForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.textureDataGridView)).EndInit();
            this.textureMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CoordDataGridView)).EndInit();
            this.coordsMenuStrip.ResumeLayout(false);
            this.formMenuStrip.ResumeLayout(false);
            this.formMenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView textureDataGridView;
        private System.Windows.Forms.DataGridView CoordDataGridView;
        private System.Windows.Forms.MenuStrip formMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip textureMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem экспортироватьТекстурыPVRToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem импортироватьТекстурыPVRToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip coordsMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem импортироватьКоординатыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem экспортироватьКоординатыToolStripMenuItem;
    }
}