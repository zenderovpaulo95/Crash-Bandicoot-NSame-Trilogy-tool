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
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public static char slash = System.IO.Path.DirectorySeparatorChar;

        //call form for work with pak files
        private void PackerBtn_Click(object sender, EventArgs e)
        {
            Packer_Tool_Form pak_form = new Packer_Tool_Form();
            pak_form.Show();
        }

        //Work with textures form
        private void TextureBtn_Click(object sender, EventArgs e)
        {
            TextureToolForm tex_tool_form = new TextureToolForm();
            tex_tool_form.Show();
        }

        //Work with modify text files
        private void TextEditBtn_Click(object sender, EventArgs e)
        {
            TextEditForm txt_form = new TextEditForm();
            txt_form.Show();
        }

        //Call "about tool" form
        private void AboutBtn_Click(object sender, EventArgs e)
        {
            AboutForm about = new AboutForm();
            about.Show();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
