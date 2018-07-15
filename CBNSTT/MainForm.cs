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

        private void PackerBtn_Click(object sender, EventArgs e)
        {
            Packer_Tool_Form pak_form = new Packer_Tool_Form();
            pak_form.Show();
        }

        private void TextureBtn_Click(object sender, EventArgs e)
        {
            TextureToolForm tex_tool_form = new TextureToolForm();
            tex_tool_form.Show();
        }

        private void FontEditBtn_Click(object sender, EventArgs e)
        {
            FontEditorForm FontEdit = new FontEditorForm();
            FontEdit.Show();
        }

        private void TextEditBtn_Click(object sender, EventArgs e)
        {
            TextEditForm txt_form = new TextEditForm();
            txt_form.Show();
        }

        private void sndBtn_Click(object sender, EventArgs e)
        {
            SoundToolForm snd_frm = new SoundToolForm();
            snd_frm.Show();
        }
    }
}
