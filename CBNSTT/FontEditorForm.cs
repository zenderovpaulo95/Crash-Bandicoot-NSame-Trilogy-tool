using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CBNSTT
{
    public partial class FontEditorForm : Form
    {
        public FontEditorForm()
        {
            InitializeComponent();
        }

        public struct Coordinates
        {
            public long eight;
            public int one;
            public int zero; //I don't remember. If I don't mistake, in PS4 was char number
            public int char_num; //Char
            public float x;
            public float y;
            public float unknown1;
            public float unknown2;
            public float unknown3;
            public float unknown4;
            public float unknown5;
        }

        public struct Textures
        {
            public short width;
            public short height;
            public int size;
            public int offset; //Needs logically use xor 0x8000000 for correctly show offset
        }

        public class fonts
        {
            public int font_num;

            public Textures[] font_textures;
            public Coordinates[] font_coords;
        }

        //This junk code needs for research fonts

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "igz (*igz) | *.igz";

            if(ofd.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(ofd.FileName, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                try
                {
                    byte[] header = br.ReadBytes(4);

                    if (Encoding.ASCII.GetString(header) == ".ZGI")
                    {
                        //Read block with coordinates and textures info
                        br.BaseStream.Seek(0x28, SeekOrigin.Begin);
                        int offset = br.ReadInt32();
                        int size = br.ReadInt32();

                        //Seek offset to needed block
                        br.BaseStream.Seek(offset, SeekOrigin.Begin);
                        //Temporary skip some needed offsets

                    //TODO: Need to think better about reading fonts
                    }
                    else MessageBox.Show("Unknown file.");

                    br.Close();
                    fs.Close();
                }
                catch
                {
                    if (br != null) br.Close();
                    if (fs != null) fs.Close();

                    MessageBox.Show("Catched unknown error.");
                }
            }
        }

        private void FontEditorForm_Load(object sender, EventArgs e)
        {
            saveToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Enabled = false;
            label1.Text = "Temporary useless. If I want to think about fonts, I'll try to make something.";
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
