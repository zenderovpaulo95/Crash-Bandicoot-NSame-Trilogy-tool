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
    public partial class FontReplacer : Form
    {
        public FontReplacer()
        {
            InitializeComponent();
        }

        private class Textures
        {
            public ushort width;
            public ushort height;
            public int tex_size;
            public string tex_format;

            public Textures() { }
            public Textures(ushort Width, ushort Height, int Size, string Format)
            {
                this.width = Width;
                this.height = Height;
                this.tex_size = Size;
                this.tex_format = Format;
            }
        }

        private class Coords
        {
            public byte[] first_block; //First 16 bytes. I don't change it.
            public int char_id;
            public float y_offset;
            public float x_offset;
            public float x_advance;
            public float unknown1;
            public float unknown2;
            public float width; //width * tex_width
            public float height; //height * tex_height
            public float x; //x * tex_width
            public float y; //y * tex_height
            public float x_end; //x_end * tex_width
            public float y_end; //y_end * tex_height

            public Coords() { }

            public Coords(byte[] FirstBlock, int CharId, float Y_offset, float X_offset, float X_advance,
                float Unknown1, float Unknown2, float Width, float Height, float X, float Y, float X_end, float Y_end)
            {
                this.first_block = FirstBlock;
                this.char_id = CharId;
                this.y_offset = Y_offset;
                this.x_offset = X_offset;
                this.x_advance = X_advance;
                this.unknown1 = Unknown1;
                this.unknown2 = Unknown2;
                this.width = Width;
                this.height = Height;
                this.x = X;
                this.y = Y;
                this.x_end = X_end;
                this.y_end = Y_end;
            }
        }

        private class FontClass
        {
            public Textures[] texture;
            public Coords[,] coordinates;

            public FontClass() { }
        }

        private void ShowTextures(FontClass font)
        {
            dataGridView1.ColumnCount = 4;
            dataGridView1.RowCount = font.texture.Length;

            for (int i = 0; i < font.texture.Length; i++)
            {
                dataGridView1[0, i].Value = font.texture[i].width;
                dataGridView1[1, i].Value = font.texture[i].height;
                dataGridView1[2, i].Value = font.texture[i].tex_size;
                dataGridView1[3, i].Value = font.texture[i].tex_format;
            }
        }

        private void ShowCoords(FontClass fonts, int num)
        {
            dataGridView2.ColumnCount = 13;
            dataGridView2.RowCount = fonts.coordinates.Length;

            for(int i = 0; i < fonts.coordinates.Length; i++)
            {
                dataGridView2[0, i].Value = fonts.coordinates[num, i].char_id;
                dataGridView2[1, i].Value = Encoding.Unicode.GetString(BitConverter.GetBytes(fonts.coordinates[num, i].char_id));
                dataGridView2[2, i].Value = fonts.coordinates[num, i].y_offset;
                dataGridView2[3, i].Value = fonts.coordinates[num, i].x_offset;
                dataGridView2[4, i].Value = fonts.coordinates[num, i].x_advance;
                dataGridView2[5, i].Value = fonts.coordinates[num, i].unknown1;
                dataGridView2[6, i].Value = fonts.coordinates[num, i].unknown2;
                dataGridView2[7, i].Value = fonts.coordinates[num, i].width;
                dataGridView2[8, i].Value = fonts.coordinates[num, i].height;
                dataGridView2[9, i].Value = fonts.coordinates[num, i].x;
                dataGridView2[10, i].Value = fonts.coordinates[num, i].y;
                dataGridView2[11, i].Value = fonts.coordinates[num, i].x_end;
                dataGridView2[12, i].Value = fonts.coordinates[num, i].y_end;
            }
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "igz files (*.igz) | *.igz";

            if(ofd.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(ofd.FileName, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                byte[] CheckBytes = br.ReadBytes(5);

                if (ASCIIEncoding.ASCII.GetString(CheckBytes) == "\x01ZGI\x0A")
                {
                    br.BaseStream.Seek(24, SeekOrigin.Begin);
                    int i = 0;
                    int[] offset = new int[3];
                    int[] size = new int[3];


                    while (i < 3)
                    {
                        offset[i] = br.ReadInt32();
                        size[i] = br.ReadInt32();
                        br.BaseStream.Seek(8, SeekOrigin.Current);
                        i++;
                    }

                    GC.Collect();

                    byte[] block1, block2, block3;

                    br.BaseStream.Seek(offset[0], SeekOrigin.Begin);
                    block1 = br.ReadBytes(size[0]);

                    br.BaseStream.Seek(offset[1], SeekOrigin.Begin);
                    block2 = br.ReadBytes(size[1]);

                    br.BaseStream.Seek(offset[2], SeekOrigin.Begin);
                    block3 = br.ReadBytes(size[2]);

                    int b_offset = 0x48;
                    float size_font;
                    byte[] tmp = new byte[4];
                    Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                    size_font = BitConverter.ToSingle(tmp, 0);

                    b_offset = 0xF0;
                    tmp = new byte[4];
                    Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                    int tex_count = BitConverter.ToInt32(tmp, 0) - 1;

                    FontClass font = new FontClass();
                    font.texture = new Textures[tex_count];
                    font.coordinates = new Coords[tex_count, 0];
                    b_offset += 24;

                    for (int k = 0; k < tex_count; k++)
                    {
                        font.texture[k] = new Textures();
                        tmp = new byte[4];
                        Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                        int tmp_calc = BitConverter.ToInt32(tmp, 0) + 24;
                        tmp = new byte[4];
                        Array.Copy(block2, tmp_calc, tmp, 0, tmp.Length);
                        tmp_calc = BitConverter.ToInt32(tmp, 0) + 56;
                        tmp = new byte[4];
                        Array.Copy(block2, tmp_calc, tmp, 0, tmp.Length);
                        tmp_calc = BitConverter.ToInt32(tmp, 0) + 24;
                        tmp = new byte[2];
                        Array.Copy(block2, tmp_calc, tmp, 0, tmp.Length);
                        font.texture[k].width = BitConverter.ToUInt16(tmp, 0);
                        tmp_calc += 2;
                        tmp = new byte[2];
                        Array.Copy(block2, tmp_calc, tmp, 0, tmp.Length);
                        font.texture[k].height = BitConverter.ToUInt16(tmp, 0);
                        tmp_calc += 0x1E;
                        tmp = new byte[4];
                        Array.Copy(block2, tmp_calc, tmp, 0, tmp.Length);
                        font.texture[k].tex_size = BitConverter.ToInt32(tmp, 0);
                        font.texture[k].tex_format = "DXT5";

                        b_offset += 16;
                    }

                    b_offset = 0x80;

                    tmp = new byte[4];
                    Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                    int coord_off = BitConverter.ToInt32(tmp, 0);
                    int coords_count = 0;

                    /*dataGridView1.RowCount = tex_count;
                    dataGridView1.ColumnCount = 4;
                    dataGridView2.RowCount = coords_count;
                    dataGridView2.ColumnCount = 13;*/

                    for (int k = 0; k < tex_count; k++)
                    {
                        coord_off += 16;
                        tmp = new byte[4];
                        Array.Copy(block2, coord_off, tmp, 0, tmp.Length);
                        coords_count = BitConverter.ToInt32(tmp, 0);

                        coord_off += 24;
                        tmp = new byte[4];
                        Array.Copy(block2, coord_off, tmp, 0, tmp.Length);
                        b_offset = BitConverter.ToInt32(tmp, 0);

                        font.coordinates = new Coords[k+1, coords_count];

                        for (int j = 0; j < coords_count; j++)
                        {
                            font.coordinates[k, j] = new Coords();
                            font.coordinates[k, j].first_block = new byte[16];
                            Array.Copy(block2, b_offset, font.coordinates[k,j].first_block, 0, font.coordinates[k, j].first_block.Length);
                            b_offset += 16;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k, j].char_id = BitConverter.ToInt32(tmp, 0);
                            //dataGridView2[0, j].Value = BitConverter.ToInt32(tmp, 0);
                            //dataGridView2[1, j].Value = Encoding.Unicode.GetString(tmp);
                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k, j].y_offset = BitConverter.ToSingle(tmp, 0);
                            //dataGridView2[2, j].Value = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k, j].x_offset = BitConverter.ToSingle(tmp, 0);
                            //dataGridView2[3, j].Value = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k, j].x_advance = BitConverter.ToSingle(tmp, 0);
                            //dataGridView2[4, j].Value = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k, j].unknown1 = BitConverter.ToSingle(tmp, 0);
                            //dataGridView2[5, j].Value = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k, j].unknown2 = BitConverter.ToSingle(tmp, 0);
                            //dataGridView2[6, j].Value = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k, j].width = BitConverter.ToSingle(tmp, 0);
                            //dataGridView2[7, j].Value = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k, j].height = BitConverter.ToSingle(tmp, 0);
                            //dataGridView2[8, j].Value = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k, j].x = BitConverter.ToSingle(tmp, 0) * font.texture[k].width;
                            //dataGridView2[9, j].Value = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k, j].y = BitConverter.ToSingle(tmp, 0) * font.texture[k].height;
                            //dataGridView2[10, j].Value = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k, j].x_end = BitConverter.ToSingle(tmp, 0) * font.texture[k].width;
                            //dataGridView2[11, j].Value = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k, j].y_end = BitConverter.ToSingle(tmp, 0) * font.texture[k].height;
                            //dataGridView2[12, j].Value = BitConverter.ToSingle(tmp, 0);
                            b_offset += 4;
                        }
                    }

                    ShowTextures(font);
                    ShowCoords(font, 0);
                }
                else MessageBox.Show("This is not a font file!");

                br.Close();
                fs.Close();
            }
        }
    }
}
