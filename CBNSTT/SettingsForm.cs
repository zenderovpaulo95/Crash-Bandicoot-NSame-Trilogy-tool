using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CBNSTT
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void BrowseBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select AMD compressonator CLI tool";

            if(ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            MainForm.filePath = AppDomain.CurrentDomain.BaseDirectory + MainForm.slash + "config.txt";
            string path = textBox1.Text;

            if ((path != null) && (path != "")
                && (System.IO.File.Exists(path)))
            {
                System.IO.File.WriteAllText(MainForm.filePath, path, Encoding.UTF8);
                MainForm.filePath = path;
                //System.IO.File.WriteAllText(path, MainForm.filePath, Encoding.UTF8);
            }

            Close();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            if (MainForm.filePath != null) textBox1.Text = MainForm.filePath;
        }
    }
}
