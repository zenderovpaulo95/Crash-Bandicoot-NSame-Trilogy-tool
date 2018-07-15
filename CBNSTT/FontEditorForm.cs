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

        Font file_font = new Font();
        bool opened_font = false;

        public class coords
        {
            public long one;
            public long eight;
            public int char_id;
            public float x_offset; //float1
            public float y_offset; //float2
            public float x_advance; //float3
            public float hz_1; //float4
            public float hz_2; //float5
            public float x; //float6
            public float y; //float7
            public float coord_width; //float8
            public float coord_height; //float9
            public float end_x; //float10
            public float end_y; //float11

            public coords() { }

            public coords(long _one, long _eight, int _char_id, float _x_offset,
                float _y_offset, float _x_advance, float _hz_1, float _hz_2, float _x,
                float _y, float _coord_width, float _coords_height, float _end_x, float _end_y)
            {
                this.one = _one;
                this.eight = _eight;
                this.char_id = _char_id;
                this.x_offset = _x_offset;
                this.y_offset = _y_offset;
                this.x_advance = _x_advance;
                this.hz_1 = _hz_1;
                this.hz_2 = _hz_2;
                this.x = _x;
                this.y = _y;
                this.coord_width = _coord_width;
                this.coord_height = _coords_height;
                this.end_x = _end_x;
                this.end_y = _end_y;
            }
        }

        public class Textures
        {
            public short width;
            public short height;
            public int size;
            public byte[] code;
            public byte[] content;

            public Textures() { }

            public Textures(short _width, short _height, int _size,
                byte[] _code, byte[] _content)
            {
                this.width = _width;
                this.height = _height;
                this.size = _size;
                this.code = _code;
                this.content = _content;
            }
        }

        public class Font : IDisposable
        {
            public List<coords> FontCoords;
            public List<Textures> FontTextures;
            public int count;
            public int[] offsets;
            public int[] sizes;
            public byte[] first_block; //Имеется в виду заголовок
            public byte[] second_block;
            public byte[] third_block;
            public List<byte[]> sub_blocks;

            public int[] in_tex_offsets;

            public Font()
            {
            }

            public void Dispose()
            {
                /*FontCoords.Clear();
                FontTextures.Clear();
                sub_blocks.Clear();*/
                offsets = null;
                sizes = null;
                first_block = null;
                second_block = null;
                third_block = null;
            }
        }

        private void экспортироватьТекстурыPVRToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Если захочу, запилю позже");
        }

        private void импортироватьТекстурыPVRToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Потом");
        }

        private void импортироватьКоординатыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "FNT-coords (*.fnt) | *.fnt";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string[] strings = File.ReadAllLines(ofd.FileName);

                short width = -1;
                short height = -1;
                
                long one = 1;
                long eight = 8;
                int char_id = -1;
                float x = 1.0f; //float6
                float y = 1.0f; //float7
                float coord_width = 1.0f; //float8
                float coord_height = 1.0f; //float9
                float end_x = 1.0f; //float10
                float end_y = 1.0f; //float11
                float x_offset = 1.0f; //float1
                float y_offset = 1.0f; //float2
                float x_advance = 1.0f; //float3
                float hz_1 = 0.0f; //float4
                float hz_2 = 0.0f; //float5

                bool coord = false;
                int count = 0;

                for (int m = 0; m < strings.Length; m++)
                {
                    string[] par = strings[m].Split(new char[] { ' ', '=', '\"', ',' });
                    coord = false;
                    count = 0;

                    for (int t = 0; t < par.Length; t++)
                    {
                        if (t <= par.Length - 1)
                        {   
                            if (par[t] == "scaleW")
                            {
                                width = Convert.ToInt16(par[t + 1]);
                            }
                            if (par[t] == "scaleH")
                            {
                                height = Convert.ToInt16(par[t + 1]);
                            }
                            if (par[t] == "id")
                            {
                                char_id = Convert.ToInt32(par[t + 1]);
                                coord = true;
                                count++;
                            }
                            if (par[t] == "x")
                            {
                                x = Convert.ToSingle(par[t + 1]);
                                count++;
                            }
                            if (par[t] == "y")
                            {
                                y = Convert.ToSingle(par[t + 1]);
                                count++;
                            }
                            if (par[t] == "width")
                            {
                                coord_width = Convert.ToSingle(par[t + 1]);
                                end_x = x + coord_width;
                                x /= width;
                                end_x /= width;
                                count += 2;
                            }
                            if (par[t] == "height")
                            {
                                coord_height = Convert.ToSingle(par[t + 1]);
                                end_y = y + coord_height;
                                y /= height;
                                end_y /= height;

                                count += 2;
                            }
                            if (par[t] == "xoffset")
                            {
                                x_offset = Convert.ToSingle(par[t + 1]);
                                count++;
                            }
                            if (par[t] == "yoffset")
                            {
                                y_offset = Convert.ToSingle(par[t + 1]);
                                count++;
                            }
                            if (par[t] == "xadvance")
                            {
                                x_advance = Convert.ToSingle(par[t + 1]);
                                count++;
                            }
                        }
                    }

                    if(count == 10 && coord)
                    {

                    }
                }
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            

            /*ofd.Filter = "IGZ (*.igz) | *.igz";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(ofd.FileName, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                int offset = 16;
                br.BaseStream.Seek(offset, SeekOrigin.Begin);
                offset += 8;
                file_font.count = br.ReadInt32();

                br.BaseStream.Seek(8, SeekOrigin.Current);

                file_font.offsets = new int[3];
                file_font.sizes = new int[3];

                for(int i = 0; i < 3; i++)
                {
                    file_font.offsets[i] = br.ReadInt32();
                    file_font.sizes[i] = br.ReadInt32();
                    br.BaseStream.Seek(8, SeekOrigin.Current);

                    offset += 16;
                }

                br.BaseStream.Seek(0, SeekOrigin.Begin);
                file_font.first_block = br.ReadBytes(file_font.offsets[0]);

                br.BaseStream.Seek(file_font.offsets[0], SeekOrigin.Begin);
                file_font.second_block = br.ReadBytes(file_font.sizes[0]);
                file_font.third_block = br.ReadBytes(file_font.sizes[1]);

                offset = 128;

                byte[] tmp = new byte[4];
                Array.Copy(file_font.third_block, offset, tmp, 0, tmp.Length);
                

                br.Close();
                fs.Close();
                opened_font = true;
            }*/
        }

        private void FontEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!opened_font) file_font.Dispose();
        }

        private void FontEditorForm_Load(object sender, EventArgs e)
        {
            textureMenuStrip.Enabled = false;
            coordsMenuStrip.Enabled = false;
            opened_font = false;
            saveToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Enabled = false;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(ofd.FileName, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                //int count = br.ReadInt32();
                //int count2 = br.ReadInt32();

                List<int> test = new List<int>();
                List<int> test2 = new List<int>();
                List<int> test3 = new List<int>();
                List<int> test4 = new List<int>();

                int off = 0;

                int tmp;
                short tmp2;

                for(int i = 0; i < 13; i++)
                {
                    tmp = br.ReadInt32();
                    test.Add(tmp);
                    tmp = br.ReadInt32();
                    test2.Add(tmp);
                    tmp = br.ReadInt32();
                    test3.Add(tmp);
                    tmp = br.ReadInt32();
                    tmp = tmp & 0x800;
                    test4.Add(tmp);
                }

                CoordDataGridView.RowCount = 13;
                CoordDataGridView.ColumnCount = 4;
                

                for(int i = 0; i < CoordDataGridView.RowCount; i++)
                {
                    CoordDataGridView[0, i].Value = test[i];
                    CoordDataGridView[1, i].Value = test2[i];
                    CoordDataGridView[2, i].Value = test3[i];
                    CoordDataGridView[3, i].Value = test4[i];
                }

               
                br.Close();
                fs.Close();
            }

            int res = 0xf194ac;
            MessageBox.Show((0x80 & 0x22).ToString());
            /*SevenZip.Compression.LZMA.Encoder encode = new SevenZip.Compression.LZMA.Encoder();
            byte[] content = File.ReadAllBytes("D:\\Crash Bandicoot\\legal\\temporary\\mack\\data\\win64\\output\\packages\\generated\\ui\\legal_pkg.igz");
            //byte[] content2 = new byte[200];
            MemoryStream ms = new MemoryStream(content);
            MemoryStream ms2 = new MemoryStream();
            encode.Code(ms, ms2, -1, -1, null);
            content = ms2.ToArray();
            ms.Close();
            ms2.Close();

            File.WriteAllBytes("D:\\Crash Bandicoot\\legal\\temporary\\mack\\data\\win64\\output\\packages\\generated\\ui\\legal_pkg.igz.cmp", content);
            MessageBox.Show("Done");*/
        }
    }
}
