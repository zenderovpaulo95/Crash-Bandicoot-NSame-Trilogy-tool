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

        List<TextureToolForm.formats> tex_formats = new List<TextureToolForm.formats>();

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

        private string GetTexFormat(byte[] block, List<TextureToolForm.formats> texture_formats)
        {
            if (block == null) return "Empty texture block";
            for(int i = 0; i < texture_formats.Count; i++)
            {
                if (BitConverter.ToString(texture_formats[i].code) == BitConverter.ToString(block)) return texture_formats[i].format;
            }

            return "Unknown";
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
            public Textures[] texture; //Get info about textures
            public int[] coords_count; //Get count coordinates in each fonts
            public int[] coords_offs; //offsets to coordinates in each fonts
            public List<Coords[]> coordinates;

            public FontClass() { }
        }

        FontClass font; //Font class

        private void ShowTextures(FontClass font)
        {
            dataGridView1.ColumnCount = 4;
            dataGridView1.Columns[0].HeaderText = "Width";
            dataGridView1.Columns[1].HeaderText = "Height";
            dataGridView1.Columns[2].HeaderText = "Size";
            dataGridView1.Columns[3].HeaderText = "Format";
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
            dataGridView2.Columns[0].HeaderText = "Char ID";
            dataGridView2.Columns[1].HeaderText = "Char";
            dataGridView2.Columns[2].HeaderText = "Y offset";
            dataGridView2.Columns[3].HeaderText = "X offset";
            dataGridView2.Columns[4].HeaderText = "X advance";
            dataGridView2.Columns[5].HeaderText = "Unknown 1"; //I don't know what it is.
            dataGridView2.Columns[6].HeaderText = "Unknown 2"; //And what it is, too.
            dataGridView2.Columns[7].HeaderText = "Width";
            dataGridView2.Columns[8].HeaderText = "Height";
            dataGridView2.Columns[9].HeaderText = "X start";
            dataGridView2.Columns[10].HeaderText = "Y start";
            dataGridView2.Columns[11].HeaderText = "X end";
            dataGridView2.Columns[12].HeaderText = "Y end";

            dataGridView2.RowCount = fonts.coordinates[num].Length;

            label1.Text = "Count of fonts: " + fonts.coordinates.Count;
            label2.Text = "Current font: " + (num + 1) + ". Count of symbols: " + fonts.coordinates[num].Length;

            for(int i = 0; i < fonts.coordinates[num].Length; i++)
            {
                dataGridView2[0, i].Value = fonts.coordinates[num][i].char_id;
                dataGridView2[1, i].Value = Encoding.Unicode.GetString(BitConverter.GetBytes(fonts.coordinates[num][i].char_id));
                dataGridView2[2, i].Value = fonts.coordinates[num][i].y_offset;
                dataGridView2[3, i].Value = fonts.coordinates[num][i].x_offset;
                dataGridView2[4, i].Value = fonts.coordinates[num][i].x_advance;
                dataGridView2[5, i].Value = fonts.coordinates[num][i].unknown1;
                dataGridView2[6, i].Value = fonts.coordinates[num][i].unknown2;
                dataGridView2[7, i].Value = fonts.coordinates[num][i].width;
                dataGridView2[8, i].Value = fonts.coordinates[num][i].height;
                dataGridView2[9, i].Value = fonts.coordinates[num][i].x;
                dataGridView2[10, i].Value = fonts.coordinates[num][i].y;
                dataGridView2[11, i].Value = fonts.coordinates[num][i].x_end;
                dataGridView2[12, i].Value = fonts.coordinates[num][i].y_end;
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
                    GC.Collect();

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

                    byte[] head_check = new byte[4];
                    int b2_offset = 0;
                    byte[] tex_format = null;

                    while((Encoding.ASCII.GetString(head_check) != "EXID") || (b2_offset < block2.Length))
                    {
                        head_check = new byte[4];
                        Array.Copy(block1, b2_offset, head_check, 0, head_check.Length);

                        if(Encoding.ASCII.GetString(head_check) == "EXID")
                        {
                            head_check = new byte[4];
                            Array.Copy(block1, b2_offset + 12, head_check, 0, head_check.Length);
                            b2_offset += BitConverter.ToInt32(head_check, 0);
                            tex_format = new byte[4];
                            Array.Copy(block1, b2_offset, tex_format, 0, tex_format.Length);
                            break;
                        }

                        head_check = new byte[4];
                        Array.Copy(block1, b2_offset + 8, head_check, 0, head_check.Length);
                        b2_offset += BitConverter.ToInt32(head_check, 0);
                    }

                    int b_offset = 0x48;
                    float size_font;
                    byte[] tmp = new byte[4];
                    Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                    size_font = BitConverter.ToSingle(tmp, 0);

                    b_offset = 0xF0;
                    tmp = new byte[4];
                    Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                    int tex_count = BitConverter.ToInt32(tmp, 0) - 1;

                    font = new FontClass();
                    font.texture = new Textures[tex_count];
                    //font.coordinates = new Coords[tex_count, 0];
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
                        font.texture[k].tex_format = GetTexFormat(tex_format, tex_formats);

                        b_offset += 16;
                    }

                    b_offset = 0x80;

                    tmp = new byte[4];
                    Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                    int coord_off = BitConverter.ToInt32(tmp, 0);
                    int coords_count = 0;

                    font.coords_offs  = new int[tex_count];
                    font.coords_count = new int[tex_count];
                    font.coordinates = new List<Coords[]>();
                    //font.coordinates = new Coords[tex_count, 0];

                    for (int k = 0; k < tex_count; k++)
                    {
                        coord_off += 16;
                        tmp = new byte[4];
                        Array.Copy(block2, coord_off, tmp, 0, tmp.Length);
                        coords_count = BitConverter.ToInt32(tmp, 0);
                        font.coords_count[k] = coords_count;

                        coord_off += 8;
                        tmp = new byte[4];
                        Array.Copy(block2, coord_off, tmp, 0, tmp.Length);
                        b_offset = BitConverter.ToInt32(tmp, 0) + 16 + coord_off;
                        font.coords_offs[k] = b_offset;
                        //b_offset += 0x40;

                        font.coordinates.Add(new Coords[coords_count]);

                        for (int j = 0; j < font.coords_count[k]; j++)
                        {
                            font.coordinates[k][j] = new Coords();
                            font.coordinates[k][j].first_block = new byte[16];
                            Array.Copy(block2, b_offset, font.coordinates[k][j].first_block, 0, font.coordinates[k][j].first_block.Length);
                            b_offset += 16;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k][j].char_id = BitConverter.ToInt32(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k][j].y_offset = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k][j].x_offset = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k][j].x_advance = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k][j].unknown1 = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k][j].unknown2 = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k][j].width = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k][j].height = BitConverter.ToSingle(tmp, 0);

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k][j].x = BitConverter.ToSingle(tmp, 0) * font.texture[k].width;

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k][j].y = BitConverter.ToSingle(tmp, 0) * font.texture[k].height;

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k][j].x_end = BitConverter.ToSingle(tmp, 0) * font.texture[k].width;

                            b_offset += 4;
                            tmp = new byte[4];
                            Array.Copy(block2, b_offset, tmp, 0, tmp.Length);
                            font.coordinates[k][j].y_end = BitConverter.ToSingle(tmp, 0) * font.texture[k].height;
                            b_offset += 4;
                        }

                        coord_off = b_offset;
                    }

                    ShowTextures(font);
                    ShowCoords(font, 0);
                }
                else MessageBox.Show("This is not a font file!");

                br.Close();
                fs.Close();
            }
        }

        private void DataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int num = dataGridView1.CurrentCell.RowIndex;
            ShowCoords(font, num);
        }

        private void FontReplacer_Load(object sender, EventArgs e)
        {
            byte[] code_rgba8888 = { 0xDE, 0x08, 0x46, 0x99 };
            byte[] tmp = { 0x50, 0x56, 0x52, 0x03, 0x00, 0x00, 0x00, 0x00, 0x72, 0x67, 0x62, 0x61, 0x08, 0x08, 0x08, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            tex_formats.Add(new TextureToolForm.formats(tmp, code_rgba8888, "r8g8b8a8")); //8888RGBA
            tmp = null;

            byte[] code_bc1 = { 0xCD, 0x06, 0x3B, 0x9D };
            byte[] code_bc1_switch = { 0x51, 0x28, 0x28, 0x1B };
            byte[] tmp2 = { 0x50, 0x56, 0x52, 0x03, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            tex_formats.Add(new TextureToolForm.formats(tmp2, code_bc1, "BC1")); //DXT1
            tex_formats.Add(new TextureToolForm.formats(tmp2, code_bc1_switch, "BC1")); //DXT1_switch
            tmp2 = null;

            byte[] code_bc3 = { 0x39, 0x88, 0x88, 0xDA };
            byte[] code_bc3_switch = { 0xCD, 0x6E, 0x45, 0x37 };
            byte[] tmp4 = { 0x50, 0x56, 0x52, 0x03, 0x00, 0x00, 0x00, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            tex_formats.Add(new TextureToolForm.formats(tmp4, code_bc3, "BC3")); //DXT5
            tex_formats.Add(new TextureToolForm.formats(tmp4, code_bc3_switch, "BC3")); //DXT5_switch
            tmp4 = null;

            byte[] code_ati2n = { 0x18, 0x47, 0xB9, 0x78 };

            GC.Collect();
        }
    }
}
