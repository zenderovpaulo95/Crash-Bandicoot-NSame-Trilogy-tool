﻿/*******************************************************
 *     Crash Bandicoot N. Same Trilogy's pack tool     *
 *   Special thanks for research archives' structure:  *
 *                Neo_Kesha & SileNTViP                *
 *******************************************************/

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;


namespace CBNSTT
{
    public partial class Packer_Tool_Form : Form
    {
        public Packer_Tool_Form()
        {
            InitializeComponent();
        }

        public struct table
        {
            public int offset;
            public short order1; //I'm not sure about that var name, but if you'll resort this values it will be ordered
            public short order2;
            public int size;
            public int c_size;              //Compressed size information (for compressed archives)
            public short block_offset;      //Offsets for either table 1 or table 2
            public short compression_flag;  //Compression flag (if 0x2000 then this LZMA compression, else if 0xFFFF then this uncompressed format)
            public string file_name;        //File name
            public int index;               //Indexes (for correctly rebuild archives)
            public byte[] big_chunks_data;  //Count block of compressed data for files more than 2 MB (or 4MB?)
        };

        public class HeaderStruct : IDisposable
        {
            public byte[] header;          //IGA\x1A header
            public int count;              //Element's count in header (it must be 11 but in Nintendo Switch was 12)
            public int table_size;         //Size of file's table and compressed blocks' table
            public int file_count;         //Count of files
            public int chunks_sz;          //Chunk size of one compressed block (maybe for some uncompressed blocks, too)
            public int unknown1;           //Unknown value (I don't know what it does)
            public int unknown2;           //Another one unknown value
            public int zero1;              //I saw there's only 0 value. I'm not sure but maybe both unknown2 and zero1 values are long type and it must be only one value
            public int big_chunks_count;   //Count of chunks for compressed big size files (more than 2MB or 4MB)
            public int small_chunks_count; //Count of chunks for compressed small size files (less than 2MB or 4MB)
            public int name_offset;        //Offset to file name's table
            public int zero2;              //It showes only 0 but I think, it must be long type of name_offset value
            public int name_table_sz;      //Size block with file names
            public int one;                //This variable always shows only value 1

            public int[] IDs;              //Some kind of IDs for files

            public table[] file_table;         //File table's structure (see struct table)
            public byte[] big_chunks_table;   //Massive bytes with short (Int16) type for big compressed archives
            public byte[]  small_chunks_table; //Massive bytes for small compressed archives (it's too hard for research, so I compress only big files)

            //I suppose that after small_chunks_table value uses some data for padding header

            public byte[] unknown_data; //Unknown data with 24 bytes length

            public HeaderStruct() { }

            //Clean memory with unused data
            public void Dispose()
            {
                header = null;
                IDs = null;
                file_table = null;
                big_chunks_table = null;
                small_chunks_table = null;
                unknown_data = null;
            }
        }

        //Browse PAK file's dialog form
        private void button1_Click(object sender, EventArgs e)
        {
            //For only one selected archives
            if (onlyOneRB.Checked)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "PAK files (*.pak) | *.pak";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = ofd.FileName;
                }
            }
            else //For some archives
            {
                FileFolderDialog ffd = new FileFolderDialog();
                ffd.Dialog.Title = "Choose a folder with pak's archives";

                if(ffd.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = ffd.SelectedPath;
                }
            }
        }

        //Padding function (size - file size, chunks - chunk size)
        int pad_size(int size, int chunks)
        {
            if(size % chunks != 0)
            {
                while(size % chunks != 0)
                {
                    if (size % chunks == 0) break;
                    size++;
                }
            }

            return size;
        }

        //Resort file table function
        public static void ResortTable(ref table[] table_resort)
        {
            for (int k = 0; k < table_resort.Length; k++)
            {
                for (int m = k - 1; m >= 0; m--)
                {
                    if (table_resort[m].offset > table_resort[m + 1].offset)
                    {
                        int tmp_offset = table_resort[m].offset;
                        int tmp_size = table_resort[m].size;
                        int tmp_c_size = table_resort[m].c_size;
                        short tmp_order1 = table_resort[m].order1;
                        short tmp_order2 = table_resort[m].order2;
                        short tmp_blk_off = table_resort[m].block_offset;
                        short tmp_com_fl = table_resort[m].compression_flag;
                        string tmp_file_name = table_resort[m].file_name;
                        int tmp_index = table_resort[m].index;
                        byte[] tmp_bcd = table_resort[m].big_chunks_data;

                        table_resort[m].offset = table_resort[m + 1].offset;
                        table_resort[m].size = table_resort[m + 1].size;
                        table_resort[m].c_size = table_resort[m + 1].c_size;
                        table_resort[m].order1 = table_resort[m + 1].order1;
                        table_resort[m].order2 = table_resort[m + 1].order2;
                        table_resort[m].block_offset = table_resort[m + 1].block_offset;
                        table_resort[m].compression_flag = table_resort[m + 1].compression_flag;
                        table_resort[m].file_name = table_resort[m + 1].file_name;
                        table_resort[m].index = table_resort[m + 1].index;
                        table_resort[m].big_chunks_data = table_resort[m + 1].big_chunks_data;

                        table_resort[m + 1].offset = tmp_offset;
                        table_resort[m + 1].order1 = tmp_order1;
                        table_resort[m + 1].order2 = tmp_order2;
                        table_resort[m + 1].block_offset = tmp_blk_off;
                        table_resort[m + 1].compression_flag = tmp_com_fl;
                        table_resort[m + 1].size = tmp_size;
                        table_resort[m + 1].c_size = tmp_c_size;
                        table_resort[m + 1].file_name = tmp_file_name;
                        table_resort[m + 1].index = tmp_index;
                        table_resort[m + 1].big_chunks_data = tmp_bcd;
                    }
                }
            }
        }

        //Unpack archive function (tested for Nintendo Switch version)
        public string UnpackArchive(string input_path, string dir_path)
        {
            int num = -1;
            if (!Directory.Exists(dir_path)) return "Directory not found. Please select another extract directory path.";
            if (!File.Exists(input_path)) return "File not found. Please select another pak file.";

            FileStream fr = new FileStream(input_path, FileMode.Open);
            BinaryReader br = new BinaryReader(fr);

            try
            {
                //Collect header information
                HeaderStruct head = new HeaderStruct();
                long header_offset = 0;
                int new_head_offset = 0;

                head.header = br.ReadBytes(4);
                header_offset += 4;
                new_head_offset += 4;
                head.count = br.ReadInt32();
                header_offset += 4;
                new_head_offset += 4;
                head.table_size = br.ReadInt32();
                header_offset += 4;
                new_head_offset += 4;
                head.file_count = br.ReadInt32();
                header_offset += 4;
                new_head_offset += 4;

                head.chunks_sz = br.ReadInt32();
                header_offset += 4;
                new_head_offset += 4;
                head.unknown1 = br.ReadInt32();
                header_offset += 4;
                new_head_offset += 4;
                head.unknown2 = br.ReadInt32();
                header_offset += 4;
                new_head_offset += 4;
                head.zero1 = br.ReadInt32();
                header_offset += 4;
                new_head_offset += 4;
                head.big_chunks_count = br.ReadInt32();
                header_offset += 4;
                new_head_offset += 4;
                head.small_chunks_count = br.ReadInt32();
                header_offset += 4;
                new_head_offset += 4;
                head.name_offset = br.ReadInt32();
                head.zero2 = br.ReadInt32();
                header_offset += 8;
                new_head_offset += 8;
                head.name_table_sz = br.ReadInt32();
                header_offset += 4;
                new_head_offset += 4;
                head.one = br.ReadInt32();
                header_offset += 4;
                new_head_offset += 4;

                head.IDs = new int[head.file_count];

                int file_size = 0;

                for (int i = 0; i < head.file_count; i++)
                {
                    head.IDs[i] = br.ReadInt32();
                    file_size += 4;
                }

                header_offset += (4 * head.file_count);
                new_head_offset += (4 * head.file_count);

                head.file_table = new table[head.file_count];

                for (int i = 0; i < head.file_count; i++)
                {
                    head.file_table[i].offset = br.ReadInt32();
                    head.file_table[i].order1 = br.ReadInt16();
                    head.file_table[i].order2 = br.ReadInt16();
                    head.file_table[i].size = br.ReadInt32();
                    head.file_table[i].c_size = -1; //Default set value -1 for uncompressed files. If file is compressed, it changes for compressed size
                    head.file_table[i].block_offset = br.ReadInt16();
                    head.file_table[i].compression_flag = br.ReadInt16();
                    head.file_table[i].index = i;
                }

                header_offset += (16 * head.file_count);
                new_head_offset += (16 * head.file_count);
                file_size += (16 * head.file_count);

                head.big_chunks_table = new byte[1];
                if (head.big_chunks_count > 0)
                {
                    head.big_chunks_table = br.ReadBytes(head.big_chunks_count * 2);
                    header_offset += head.big_chunks_count * 2;
                }

                head.small_chunks_table = new byte[1];
                if (head.small_chunks_count > 0)
                {
                    head.small_chunks_table = br.ReadBytes(head.small_chunks_count);
                    header_offset += head.small_chunks_count;
                }

                head.small_chunks_count = 0;
                head.big_chunks_count = 0;
                head.table_size = file_size;

                int padded_off = pad_size((int)header_offset, 4);
                br.BaseStream.Seek(padded_off, SeekOrigin.Begin);

                int new_size = (int)header_offset - head.small_chunks_count - (head.big_chunks_count * 2);

                //Very strange method. I need rethink this method.
                int padded_sz = pad_size(new_size, 4) - new_size;
                byte[] tmp;

                if (padded_sz > 0)
                {
                    tmp = new byte[padded_sz];
                    header_offset += padded_sz;
                }

                head.unknown_data = br.ReadBytes(24);
                new_size += 24;
                new_head_offset += 24;
                padded_sz = pad_size(new_head_offset, 0x800) - new_head_offset;

                header_offset = pad_size(new_head_offset, 0x800);

                br.BaseStream.Seek(head.name_offset, SeekOrigin.Begin);
                byte[] name_block = br.ReadBytes(head.name_table_sz);
                int off = 0;
                int offf = 0;
                int counter = 0;

                int index = 0;

                for (int j = 0; j < head.file_count; j++)
                {
                    tmp = new byte[4];
                    counter = 0;
                    Array.Copy(name_block, off, tmp, 0, 4);
                    off += 4;
                    offf = BitConverter.ToInt32(tmp, 0);

                    tmp = new byte[1];

                    char ch = '1';

                    tmp = new byte[name_block.Length - offf];
                    Array.Copy(name_block, offf, tmp, 0, tmp.Length);

                    index = 0;

                    while (index < tmp.Length)
                    {
                        ch = (char)tmp[index];

                        if (ch == '\0')
                        {
                            break;
                        }

                        index++;
                        counter++;
                    }

                    tmp = new byte[counter];
                    Array.Copy(name_block, offf, tmp, 0, tmp.Length);

                    head.file_table[j].file_name = Encoding.ASCII.GetString(tmp);
                    if (head.file_table[j].file_name.Contains("/")) head.file_table[j].file_name = head.file_table[j].file_name.Replace('/', '\\');
                }

                table[] new_table = new table[head.file_count];

                for (int i = 0; i < head.file_table.Length; i++)
                {
                    new_table[i].offset = head.file_table[i].offset;
                    new_table[i].size = head.file_table[i].size;
                    new_table[i].c_size = head.file_table[i].c_size;
                    new_table[i].order1 = head.file_table[i].order1;
                    new_table[i].order2 = head.file_table[i].order2;
                    new_table[i].block_offset = head.file_table[i].block_offset;
                    new_table[i].compression_flag = head.file_table[i].compression_flag;
                    new_table[i].file_name = head.file_table[i].file_name;
                    new_table[i].index = head.file_table[i].index;

                    if (head.file_table[i].size >= 0x40000 && head.big_chunks_count > 0)
                    {
                        int tmp_off = head.file_table[i].block_offset * 2;
                        int tmp_sz = (pad_size(head.file_table[i].size, 0x8000) / 0x8000) + 2;
                        head.file_table[i].big_chunks_data = new byte[tmp_sz];
                        Array.Copy(head.big_chunks_table, tmp_off, head.file_table[i].big_chunks_data, 0, head.file_table[i].big_chunks_data.Length);
                    }
                    else head.file_table[i].big_chunks_data = null;

                    new_table[i].big_chunks_data = head.file_table[i].big_chunks_data;
                }


                byte[] content;
                int ch_size;
                byte[] properties;
                byte[] c_content;


                for (int j = 0; j < new_table.Length; j++)
                {
                    string pak_name = get_file_name(input_path);
                    pak_name = pak_name.Remove(pak_name.IndexOf('.'), pak_name.Length - pak_name.IndexOf('.'));
                    string dir = get_dir_path(new_table[j].file_name);

                    if (!Directory.Exists(dir_path + "\\" + pak_name + "\\" + dir)) Directory.CreateDirectory(dir_path + "\\" + pak_name + "\\" + dir);
                    if (File.Exists(dir_path + "\\" + pak_name + "\\" + new_table[j].file_name)) File.Delete(dir_path + "\\" + pak_name + "\\" + new_table[j].file_name);

                    if (new_table[j].compression_flag != -1)
                    {
                        int size = 0;

                        int offset = new_table[j].offset;
                        int def_block = 0x8000;

                        FileStream fw = new FileStream(dir_path + "\\" + pak_name + "\\" + new_table[j].file_name, FileMode.CreateNew);

                        while (size != new_table[j].size)
                        {
                            try
                            {
                                br.BaseStream.Seek(offset, SeekOrigin.Begin);
                                if (head.count == 11) ch_size = br.ReadInt16();
                                else ch_size = br.ReadInt32();

                                if (def_block > new_table[j].size - size) def_block = new_table[j].size - size;

                                properties = br.ReadBytes(5);
                                c_content = br.ReadBytes(ch_size);

                                SevenZip.Compression.LZMA.Decoder decode = new SevenZip.Compression.LZMA.Decoder();
                                decode.SetDecoderProperties(properties);
                                MemoryStream ms = new MemoryStream(c_content);
                                MemoryStream ms2 = new MemoryStream();
                                decode.Code(ms, ms2, ch_size, def_block, null);
                                content = ms2.ToArray();
                                ms.Close();
                                ms2.Close();

                                fw.Write(content, 0, content.Length);

                                if (head.count == 11) offset += pad_size(ch_size + 7, 0x800);
                                else offset += pad_size(ch_size + 9, 0x800);

                                size += def_block;

                                content = null;
                                c_content = null;
                            }
                            catch
                            {
                                if(size >= new_table[j].size)
                                {
                                    if (fw != null) fw.Close();
                                    if (br != null) br.Close();
                                    if (fr != null) fr.Close();
                                    if (File.Exists(dir_path + "\\" + pak_name + "\\" + new_table[j].file_name)) File.Delete(dir_path + "\\" + pak_name + "\\" + new_table[j].file_name);

                                    return "Wrong archive format: " + get_file_name(pak_name);
                                }
                                else
                                {
                                    br.BaseStream.Seek(offset, SeekOrigin.Begin);
                                    def_block = 0x8000;

                                    if (def_block > new_table[j].size - size) def_block = new_table[j].size - size;

                                    content = br.ReadBytes(def_block);
                                    fw.Write(content, 0, content.Length);

                                    offset += pad_size(def_block, 0x800);
                                    size += def_block;
                                    content = null;
                                }
                            }
                        }

                        fw.Close();
                    }
                    else
                    {
                        br.BaseStream.Seek(new_table[j].offset, SeekOrigin.Begin);
                        content = br.ReadBytes(new_table[j].size);

                        FileStream fw = new FileStream(dir_path + "\\" + pak_name + "\\" + new_table[j].file_name, FileMode.CreateNew);
                        fw.Write(content, 0, content.Length);
                        fw.Close();
                        content = null;
                    }
                }

                new_table = null;
                head.Dispose();

                br.Close();
                fr.Close();

                return "File " + input_path + " successfully extracted!";
            }
            catch
            {
                if (br != null) br.Close();
                if (fr != null) fr.Close();
                return "Something wrong. The last file was number " + (num + 1).ToString();
            }
        }

        //Experimental repack function
        public string RepackNew(string input_path, string output_path, string dir_path)
        {
            if (File.Exists(output_path + ".tmp")) File.Delete(output_path + ".tmp");

            DirectoryInfo di = new DirectoryInfo(dir_path);
            FileInfo[] fi = di.GetFiles("*.*", SearchOption.AllDirectories);

            FileStream fr = new FileStream(input_path, FileMode.Open);
            FileStream fs = new FileStream(output_path + ".tmp", FileMode.CreateNew);
            BinaryReader br = new BinaryReader(fr);
            BinaryWriter bw = new BinaryWriter(fs);

            byte[] c_header = { 0x5D, 0x00, 0x80, 0x00, 0x00 }; //Compressed header (for PC version. For Nintendo Switch I have to think)

            HeaderStruct head = new HeaderStruct();

            long header_offset = 0;
            int new_head_offset = 0;

            head.header = br.ReadBytes(4);
            header_offset += 4;
            new_head_offset += 4;
            head.count = br.ReadInt32();
            header_offset += 4;
            new_head_offset += 4;
            head.table_size = br.ReadInt32();
            header_offset += 4;
            new_head_offset += 4;
            head.file_count = br.ReadInt32();
            header_offset += 4;
            new_head_offset += 4;

            if (fi.Length != head.file_count)
            {
                br.Close();
                bw.Close();
                fs.Close();
                fr.Close();
                File.Delete(output_path + ".tmp");

                return "Count of files in directory and archives don't fit. It must be " + head.file_count + "but found " + fi.Length + " files.";
            }

            head.chunks_sz = br.ReadInt32();
            header_offset += 4;
            new_head_offset += 4;
            head.unknown1 = br.ReadInt32();
            header_offset += 4;
            new_head_offset += 4;
            head.unknown2 = br.ReadInt32();
            header_offset += 4;
            new_head_offset += 4;
            head.zero1 = br.ReadInt32();
            header_offset += 4;
            new_head_offset += 4;
            head.big_chunks_count = br.ReadInt32();
            header_offset += 4;
            new_head_offset += 4;
            head.small_chunks_count = br.ReadInt32();
            header_offset += 4;
            new_head_offset += 4;
            head.name_offset = br.ReadInt32();
            head.zero2 = br.ReadInt32();
            header_offset += 8;
            new_head_offset += 8;
            head.name_table_sz = br.ReadInt32();
            header_offset += 4;
            new_head_offset += 4;
            head.one = br.ReadInt32();
            header_offset += 4;
            new_head_offset += 4;

            head.IDs = new int[head.file_count];

            int file_size = 0;

            for (int i = 0; i < head.file_count; i++)
            {
                head.IDs[i] = br.ReadInt32();
                file_size += 4;
            }

            header_offset += (4 * head.file_count);
            new_head_offset += (4 * head.file_count);

            head.file_table = new table[head.file_count];

            for (int i = 0; i < head.file_count; i++)
            {
                head.file_table[i].offset = br.ReadInt32();
                head.file_table[i].order1 = br.ReadInt16();
                head.file_table[i].order2 = br.ReadInt16();
                head.file_table[i].size = br.ReadInt32();
                head.file_table[i].c_size = -1;
                head.file_table[i].block_offset = br.ReadInt16();
                head.file_table[i].compression_flag = br.ReadInt16();
                
                head.file_table[i].index = i;
            }

            header_offset += (16 * head.file_count);
            new_head_offset += (16 * head.file_count);
            file_size += (16 * head.file_count);

            head.big_chunks_table = new byte[1];
            if (head.big_chunks_count > 0)
            {
                head.big_chunks_table = br.ReadBytes(head.big_chunks_count * 2);
                header_offset += head.big_chunks_count * 2;
            }

            head.small_chunks_table = new byte[1];
            if (head.small_chunks_count > 0)
            {
                head.small_chunks_table = br.ReadBytes(head.small_chunks_count);
                header_offset += head.small_chunks_count;
            }

            head.small_chunks_count = 0;
            head.big_chunks_count = 0;
            head.table_size = file_size;

            int padded_off = pad_size((int)header_offset, 4);
            br.BaseStream.Seek(padded_off, SeekOrigin.Begin);

            int new_size = (int)header_offset - head.small_chunks_count - (head.big_chunks_count * 2);

            //Какой-то изврат. Надо будет подумать над этим...
            int padded_sz = pad_size(new_size, 4) - new_size;
            byte[] tmp;

            if (padded_sz > 0)
            {
                tmp = new byte[padded_sz];
                header_offset += padded_sz;
            }

            head.unknown_data = br.ReadBytes(24);
            new_size += 24;
            new_head_offset += 24;
            padded_sz = pad_size(new_head_offset, 0x800) - new_head_offset;

            header_offset = pad_size(new_head_offset, 0x800);

            br.BaseStream.Seek(head.name_offset, SeekOrigin.Begin);
            byte[] name_block = br.ReadBytes(head.name_table_sz);
            int off = 0;
            int offf = 0;
            int counter = 0;

            int index = 0;

            for (int j = 0; j < head.file_count; j++)
            {
                tmp = new byte[4];
                counter = 0;
                Array.Copy(name_block, off, tmp, 0, 4);
                off += 4;
                offf = BitConverter.ToInt32(tmp, 0);

                tmp = new byte[1];

                char ch = '1';

                tmp = new byte[name_block.Length - offf];
                Array.Copy(name_block, offf, tmp, 0, tmp.Length);

                index = 0;

                while (index < tmp.Length)
                {
                    ch = (char)tmp[index];

                    if (ch == '\0')
                    {
                        break;
                    }

                    index++;
                    counter++;
                }

                tmp = new byte[counter];
                Array.Copy(name_block, offf, tmp, 0, tmp.Length);

                head.file_table[j].file_name = Encoding.ASCII.GetString(tmp);
                if (head.file_table[j].file_name.Contains("/")) head.file_table[j].file_name = head.file_table[j].file_name.Replace('/', '\\');
            }

            table[] new_table = new table[head.file_count];

            for (int i = 0; i < head.file_table.Length; i++)
            {
                new_table[i].offset = head.file_table[i].offset;
                new_table[i].size = head.file_table[i].size;
                new_table[i].c_size = head.file_table[i].c_size;
                new_table[i].order1 = head.file_table[i].order1;
                new_table[i].order2 = head.file_table[i].order2;
                new_table[i].block_offset = head.file_table[i].block_offset;
                new_table[i].compression_flag = head.file_table[i].compression_flag;
                new_table[i].file_name = head.file_table[i].file_name;
                new_table[i].index = head.file_table[i].index;

                if (head.file_table[i].size >= 0x40000 && head.big_chunks_count > 0)
                {
                    int tmp_off = head.file_table[i].block_offset * 2;
                    int tmp_sz = (pad_size(head.file_table[i].size, 0x8000) / 0x8000) + 2;
                    head.file_table[i].big_chunks_data = new byte[tmp_sz];
                    Array.Copy(head.big_chunks_table, tmp_off, head.file_table[i].big_chunks_data, 0, head.file_table[i].big_chunks_data.Length);
                }
                else head.file_table[i].big_chunks_data = null;

                new_table[i].big_chunks_data = head.file_table[i].big_chunks_data;
            }

            ResortTable(ref new_table);

            int offset = 0;
            int size = 0;
            int count = 0;
            byte[] tmp2;
            List<byte[]> tmp3 = new List<byte[]>();
            List<byte[]> tmp4 = new List<byte[]>();
            int c_offset = 0; //Table's offset

                for (int i = 0; i < new_table.Length; i++)
                {
                    index = 0;

                    bool res = false;

                    while (!res)
                    {
                        if (fi[index].FullName.Length < 255)
                        {
                            if (fi[index].FullName.ToUpper().IndexOf(new_table[i].file_name.ToUpper()) > 0)
                            {
                                new_table[i].offset = offset;
                                new_table[i].size = (int)fi[index].Length;

                            if (new_table[i].size >= 0x40000 && new_table[i].compression_flag != -1)
                            {
                                count = (pad_size(new_table[i].size, 0x8000) / 0x8000) + 1;
                                head.big_chunks_count += count;

                                int c_off = 0;

                                int f_off = 0; //For recounting file's offset
                                int f_size = (int)fi[index].Length;
                                new_table[i].size = (int)fi[index].Length;

                                int bl_size = 0x8000; //Default uncompressed block size
                                short len = 0;
                                int len_int32 = 0; //Nintendo Switch
                                short tmps = 0;
                                int pos = 4;
                                int plus = 9;

                                if (tmp3.Count > 0) tmp3.Clear();

                                new_table[i].c_size = 0;

                                FileStream fcr = new FileStream(fi[index].FullName, FileMode.Open);
                                while(f_off != f_size)
                                {
                                    bl_size = 0x8000;
                                    if (bl_size > f_size - f_off) bl_size = f_size - f_off;
                                    tmp = new byte[bl_size];
                                    fcr.Read(tmp, 0, tmp.Length);

                                    f_off += tmp.Length;

                                    SevenZip.Compression.LZMA.Encoder encode = new SevenZip.Compression.LZMA.Encoder();
                                    MemoryStream ms = new MemoryStream(tmp);
                                    MemoryStream ms2 = new MemoryStream();
                                    encode.Code(ms, ms2, -1, -1, null);
                                    ms2.Close();
                                    ms.Close();

                                    tmp = ms2.ToArray();
                                    len = (short)tmp.Length;
                                    len_int32 = tmp.Length;

                                    if (head.count == 11)
                                    {
                                        pos = 2;
                                        plus = 7;
                                    }

                                    tmp2 = new byte[pad_size(tmp.Length + plus, 0x800)];
                                    Array.Copy(c_header, 0, tmp2, pos, c_header.Length);
                                    Array.Copy(tmp, 0, tmp2, plus, tmp.Length);

                                    if (head.count == 11)
                                    {
                                        tmp = new byte[2];
                                        tmp = BitConverter.GetBytes(len);
                                        Array.Copy(tmp, 0, tmp2, 0, tmp.Length);
                                    }
                                    else
                                    {
                                        tmp = new byte[4];
                                        tmp = BitConverter.GetBytes(len_int32);
                                        Array.Copy(tmp, 0, tmp2, 0, tmp.Length);
                                    }

                                    new_table[i].c_size += tmp2.Length;

                                    tmp = new byte[2];
                                    tmps = (short)((c_off / 0x800) | 0x8000); //First compressed offset divides 0x800, then logically sums 0x8000 for correct value
                                    tmp = BitConverter.GetBytes(tmps);
                                    tmp3.Add(tmp);
                                    c_off += tmp2.Length;

                                    bw.Write(tmp2, 0, tmp2.Length);
                                }

                                tmps = (short)(new_table[i].c_size / 0x800);
                                tmp = new byte[2];
                                tmp = BitConverter.GetBytes(tmps);
                                tmp3.Add(tmp);

                                tmp = new byte[count * 2];

                                new_table[i].big_chunks_data = null;
                                new_table[i].block_offset = (short)c_offset;

                                c_offset += (tmp.Length / 2);

                                for (int c = 0; c < tmp3.Count; c++)
                                {
                                    Array.Copy(tmp3[c], 0, tmp, c * 2, tmp3[c].Length);
                                }

                                fcr.Close();

                                tmp4.Add(tmp);
                                offset += new_table[i].c_size;
                            }
                            else
                            {
                                offset += pad_size((int)fi[index].Length, 0x800);
                                new_table[i].block_offset = -1;
                                new_table[i].compression_flag = -1;

                                FileStream frf = new FileStream(fi[index].FullName, FileMode.Open);
                                tmp = new byte[frf.Length];
                                frf.Read(tmp, 0, tmp.Length);
                                frf.Close();

                                tmp2 = new byte[pad_size(tmp.Length, 0x800)];
                                Array.Copy(tmp, 0, tmp2, 0, tmp.Length);

                                bw.Write(tmp2, 0, tmp2.Length);
                            }
                                res = true;
                                break;
                            }

                            index++;

                            if (index >= fi.Length)
                            {
                                br.Close();
                                bw.Close();
                                fs.Close();
                                fr.Close();

                                if (File.Exists(output_path + ".tmp")) File.Delete(output_path + ".tmp");
                                GC.Collect();
                                return "Файл архива " + output_path + " не найден в папке!";
                            }
                        }
                        else
                        {
                            //Stupid Windows 7's limit. I use this silly method
                            br.Close();
                            bw.Close();
                            fs.Close();
                            fr.Close();

                            if (File.Exists(output_path + ".tmp")) File.Delete(output_path + ".tmp");
                            GC.Collect();
                            return "File name path's length more than 255 symbols. Please trim it for correctly work.";
                        }
                    }
                }

            head.name_offset = offset;

            bw.Close();
            br.Close();
            fs.Close();
            fr.Close();

            offset = pad_size(pad_size(56 + (4 * head.file_count) + (16 * head.file_count) + (head.big_chunks_count * 2) + head.small_chunks_count, 4) + 24, 0x800);

            for(int i = 0; i < new_table.Length; i++)
            {
                new_table[i].offset += offset;
            }
            head.name_offset += offset;

            if (File.Exists(output_path)) File.Delete(output_path);

            fr = new FileStream(output_path + ".tmp", FileMode.Open);
            fs = new FileStream(output_path, FileMode.CreateNew);
            bw = new BinaryWriter(fs);
            bw.Write(head.header);
            bw.Write(head.count);
            head.table_size = (4 * head.file_count) + (16 * head.file_count) + (head.big_chunks_count * 2) + head.small_chunks_count;
            bw.Write(head.table_size);
            bw.Write(head.file_count);
            bw.Write(head.chunks_sz);
            bw.Write(head.unknown1);
            bw.Write(head.unknown2);
            bw.Write(head.zero1);
            bw.Write(head.big_chunks_count);
            bw.Write(head.small_chunks_count);
            bw.Write(head.name_offset);
            bw.Write(head.zero2);
            bw.Write(head.name_table_sz);
            bw.Write(head.one);

            for(int i = 0; i < head.file_count; i++)
            {
                bw.Write(head.IDs[i]);
            }

            for (int i = 0; i < head.file_count; i++)
                for(int j = 0; j < new_table.Length; j++)
                {
                    if(head.file_table[i].index == new_table[j].index)
                    {
                        bw.Write(new_table[j].offset);
                        
                        bw.Write(new_table[j].order1);
                        
                        bw.Write(new_table[j].order2);
                        
                        bw.Write(new_table[j].size);
                        
                        bw.Write(new_table[j].block_offset);
                        
                        bw.Write(new_table[j].compression_flag);
                        
                    }
                }

            for(int i = 0; i < tmp4.Count; i++)
            {
                bw.Write(tmp4[i]);
                
            }

            if(head.small_chunks_table.Length > 0 && head.small_chunks_count > 0)
            {
                bw.Write(head.small_chunks_table);
                
            }

            int tmp_sz2 = 56 + (4 * head.file_count) + (16 * head.file_count) + (head.big_chunks_count * 2) + head.small_chunks_count;

            tmp = new byte[pad_size(tmp_sz2, 4) - tmp_sz2];
            bw.Write(tmp);

            tmp = new byte[offset - (pad_size(tmp_sz2, 4) + 24)];

            bw.Write(head.unknown_data);

            bw.Write(tmp);

            offset = 0;

            while (offset != fr.Length)
            {
                tmp = new byte[0x10000]; //just in case I uses buffer with 64KB
                if (tmp.Length > fr.Length - offset) tmp = new byte[fr.Length - offset];

                fr.Read(tmp, 0, tmp.Length);
                bw.Write(tmp);

                offset += tmp.Length;
            }

            bw.Write(name_block);
            
            fr.Close();
            bw.Close();
            fs.Close();

            if (File.Exists(output_path + ".tmp")) File.Delete(output_path + ".tmp");


            new_table = null;
            head.Dispose();

            return "File " + output_path + " successfully rebuilt!";
        }

        public string RepackArchive(string input_path, string output_path, string dir_path, bool compress)
        {
            #region
            if (File.Exists(output_path + ".tmp")) File.Delete(output_path + ".tmp");

                DirectoryInfo di = new DirectoryInfo(dir_path);
                FileInfo[] fi = di.GetFiles("*.*", SearchOption.AllDirectories);

                FileStream fr = new FileStream(input_path, FileMode.Open);
                FileStream fs = new FileStream(output_path + ".tmp", FileMode.CreateNew);
                BinaryReader br = new BinaryReader(fr);
                BinaryWriter bw = new BinaryWriter(fs);

            HeaderStruct head = new HeaderStruct();

                long header_offset = 0;
                int new_head_offset = 0;

                head.header = br.ReadBytes(4);
                    header_offset += 4;
                    new_head_offset += 4;
                head.count = br.ReadInt32();
                    header_offset += 4;
                    new_head_offset += 4;
                head.table_size = br.ReadInt32();
                    header_offset += 4;
                    new_head_offset += 4;
                head.file_count = br.ReadInt32();
                    header_offset += 4;
                    new_head_offset += 4;

            if (fi.Length != head.file_count)
            {
                br.Close();
                bw.Close();
                fs.Close();
                fr.Close();
                File.Delete(output_path + ".tmp");

                return "Count of files in directory and archives don't fit. It must be " + head.file_count + "but found " + fi.Length + " files.";
            }

                head.chunks_sz = br.ReadInt32();
                    header_offset += 4;
                    new_head_offset += 4;
                head.unknown1 = br.ReadInt32();
                    header_offset += 4;
                    new_head_offset += 4;
                head.unknown2 = br.ReadInt32();
                    header_offset += 4;
                    new_head_offset += 4;
                head.zero1 = br.ReadInt32();
                    header_offset += 4;
                    new_head_offset += 4;
                head.big_chunks_count = br.ReadInt32();
                    header_offset += 4;
                    new_head_offset += 4;
                head.small_chunks_count = br.ReadInt32();
                    header_offset += 4;
                    new_head_offset += 4;
                head.name_offset = br.ReadInt32();
                head.zero2 = br.ReadInt32();
                    header_offset += 8;
                    new_head_offset += 8;
                head.name_table_sz = br.ReadInt32();
                    header_offset += 4;
                    new_head_offset += 4;
                head.one = br.ReadInt32();
                    header_offset += 4;
                    new_head_offset += 4;

            head.IDs = new int[head.file_count];

            int file_size = 0;

            for(int i = 0; i < head.file_count; i++)
            {
                head.IDs[i] = br.ReadInt32();
                file_size += 4;
            }

            header_offset += (4 * head.file_count);
            new_head_offset += (4 * head.file_count);

            head.file_table = new table[head.file_count];

            for (int i = 0; i < head.file_count; i++)
            {
                head.file_table[i].offset = br.ReadInt32();
                head.file_table[i].order1 = br.ReadInt16();
                head.file_table[i].order2 = br.ReadInt16();
                head.file_table[i].size = br.ReadInt32();
                head.file_table[i].c_size = -1;
                head.file_table[i].block_offset = br.ReadInt16();
                head.file_table[i].compression_flag = br.ReadInt16();
                head.file_table[i].block_offset = -1;
                head.file_table[i].compression_flag = -1;
                head.file_table[i].index = i;
            }

             header_offset += (16 * head.file_count);
             new_head_offset += (16 * head.file_count);
             file_size += (16 * head.file_count);

            head.big_chunks_table = new byte[1];
            if (head.big_chunks_count > 0)
            {
                head.big_chunks_table = br.ReadBytes(head.big_chunks_count * 2);
                header_offset += head.big_chunks_count * 2;
            }

            head.small_chunks_table = new byte[1];
            if(head.small_chunks_count > 0)
            {
                head.small_chunks_table = br.ReadBytes(head.small_chunks_count);
                header_offset += head.small_chunks_count;
            }

            head.small_chunks_count = 0;
            head.big_chunks_count = 0;
            head.table_size = file_size;

            int padded_off = pad_size((int)header_offset, 4);
            br.BaseStream.Seek(padded_off, SeekOrigin.Begin);

            int new_size = (int)header_offset - head.small_chunks_count - (head.big_chunks_count * 2);

            int padded_sz = pad_size(new_size, 4) - new_size;
            byte[] tmp;

            if (padded_sz > 0)
            {
                tmp = new byte[padded_sz];
                header_offset += padded_sz;
            }

            head.unknown_data = br.ReadBytes(24);
            new_size += 24;
            new_head_offset += 24;
            padded_sz = pad_size(new_head_offset, 0x800) - new_head_offset;
            
            header_offset = pad_size(new_head_offset, 0x800);

             br.BaseStream.Seek(head.name_offset, SeekOrigin.Begin);
             byte[] name_block = br.ReadBytes(head.name_table_sz);
             int off = 0;
             int offf = 0;
             int counter = 0;

             int index = 0;

                    for (int j = 0; j < head.file_count; j++)
                    {
                        tmp = new byte[4];
                        counter = 0;
                        Array.Copy(name_block, off, tmp, 0, 4);
                        off += 4;
                        offf = BitConverter.ToInt32(tmp, 0);

                        tmp = new byte[1];

                        char ch = '1';

                        tmp = new byte[name_block.Length - offf];
                        Array.Copy(name_block, offf, tmp, 0, tmp.Length);

                        index = 0;

                        while (index<tmp.Length)
                        {
                            ch = (char) tmp[index];

                            if (ch == '\0')
                            {
                                break;
                            }

                            index++;
                            counter++;
                        }

                        tmp = new byte[counter];
                        Array.Copy(name_block, offf, tmp, 0, tmp.Length);

                        head.file_table[j].file_name = Encoding.ASCII.GetString(tmp);
                        if (head.file_table[j].file_name.Contains("/")) head.file_table[j].file_name = head.file_table[j].file_name.Replace('/', '\\');
                    }

                    table[] new_table = new table[head.file_count];

                    for(int i = 0; i < head.file_table.Length; i++)
                    {
                        new_table[i].offset = head.file_table[i].offset;
                        new_table[i].size = head.file_table[i].size;
                        new_table[i].c_size = head.file_table[i].c_size;
                        new_table[i].order1 = head.file_table[i].order1;
                        new_table[i].order2 = head.file_table[i].order2;
                        new_table[i].block_offset = head.file_table[i].block_offset;
                        new_table[i].compression_flag = head.file_table[i].compression_flag;
                        new_table[i].file_name = head.file_table[i].file_name;
                        new_table[i].index = head.file_table[i].index;
                    }

                    ResortTable(ref new_table);

                int files_off = (int)header_offset;

            for (int i = 0; i < new_table.Length; i++)
            {
                index = 0;

                bool res = false;

                while (!res)
                {
                    if (fi[index].FullName.Length < 255)
                    {
                        if (fi[index].FullName.ToUpper().IndexOf(new_table[i].file_name.ToUpper()) > 0)
                        {
                            new_table[i].offset = files_off;
                            new_table[i].size = (int)fi[index].Length;

                            files_off += pad_size((int)fi[index].Length, 0x800);
                            res = true;
                            break;
                        }

                        index++;

                        if (index >= fi.Length)
                        {
                            br.Close();
                            bw.Close();
                            fs.Close();
                            fr.Close();

                            if (File.Exists(output_path + ".tmp")) File.Delete(output_path + ".tmp");
                            GC.Collect();
                            return "File of archive " + output_path + " wasn't found in this directory.";
                        }
                    }
                    else
                    {
                        br.Close();
                        bw.Close();
                        fs.Close();
                        fr.Close();

                        if (File.Exists(output_path + ".tmp")) File.Delete(output_path + ".tmp");
                        GC.Collect();
                        return "File name path's length more than 255 symbols. Please trim it for correctly work.";
                    }
                }
            }

                head.name_offset = files_off;

                for (int f = 0; f < head.file_table.Length; f++)
                {
                    for (int j = 0; j < new_table.Length; j++)
                    {
                        if ((new_table[j].index == head.file_table[f].index))
                        {
                             head.file_table[f].offset = new_table[j].offset;
                             head.file_table[f].size = new_table[j].size;

                            break;
                        }
                    }
                }

                //Writing recieved data into file
                bw.Write(head.header);
                bw.Write(head.count);
                bw.Write(head.table_size);
                bw.Write(head.file_count);
                bw.Write(head.chunks_sz);
                bw.Write(head.unknown1);
                bw.Write(head.unknown2);
                bw.Write(head.zero1);
                bw.Write(head.big_chunks_count);
                bw.Write(head.small_chunks_count);
                bw.Write(head.name_offset);
                bw.Write(head.zero2);
                bw.Write(head.name_table_sz);
                bw.Write(head.one);
            
            for(int i = 0; i < head.IDs.Length; i++)
            {
                bw.Write(head.IDs[i]);
            }

                for (int i = 0; i < head.file_table.Length; i++)
                {
                    bw.Write(head.file_table[i].offset);
                    bw.Write(head.file_table[i].order1);
                    bw.Write(head.file_table[i].order2);
                    bw.Write(head.file_table[i].size);
                    bw.Write(head.file_table[i].block_offset);
                    bw.Write(head.file_table[i].compression_flag);
                }

                bw.Write(head.unknown_data);
                if (padded_sz > 0)
                {
                    tmp = new byte[padded_sz];
                    bw.Write(tmp);
                }

                byte[] tmp_f;

                int idx = 0;
                bool res2;

                for (int i = 0; i < new_table.Length; i++)
                {
                    idx = 0;
                    res2 = false;

                    while (!res2)
                    {
                        if (fi[idx].FullName.ToUpper().IndexOf(new_table[i].file_name.ToUpper()) > 0)
                        {
                                tmp = File.ReadAllBytes(fi[idx].FullName);
                                tmp_f = new byte[pad_size(tmp.Length, 0x800)];
                                Array.Copy(tmp, 0, tmp_f, 0, tmp.Length);
                                bw.Write(tmp_f);
                                tmp = null;
                                tmp_f = null;
                                res2 = true;
                        }
                        idx++;
                    }
                }

                bw.Write(name_block);

                name_block = null;

            string info = "\r\n";


                new_table = null;

                br.Close();
                bw.Close();
                fs.Close();
                fr.Close();

                if (File.Exists(output_path)) File.Delete(output_path);

                File.Move(output_path + ".tmp", output_path);

                head.Dispose();

                GC.Collect();

                return "File " + output_path + " successfully rebuilt." + info;
            #endregion
        }

        public static string get_file_name(string path)
        {
            int len = path.Length - 1;

            while(path[len] != '\\')
            {
                len--;

                if (len < 0) return null;
            }

            path = path.Remove(0, len + 1);

            return path;
        }

        public static string get_dir_path(string path)
        {
            int len = path.Length - 1;

            while (path[len] != '\\')
            {
                len--;

                if (len < 0) return null;
            }

            path = path.Remove(len, path.Length - len);

            return path;
        }

        private void button2_Click(object sender, EventArgs e) //Browse folder with resources
        {
            FileFolderDialog fbd = new FileFolderDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
               textBox2.Text = fbd.SelectedPath;
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            bool save_modal = checkBox1.Checked;

            string output_path = textBox1.Text;

            string pak_path = textBox1.Text; 

            string dir_path = textBox2.Text; //Resource's folder

            if (onlyOneRB.Checked)
            {
                if (File.Exists(pak_path) && Directory.Exists(dir_path))
                {
                    if (save_modal)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Filter = "PAK file | *.pak";

                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            output_path = sfd.FileName;
                        }
                    }

                    string result = RepackArchive(pak_path, output_path, dir_path, false);

                    MessageBox.Show(result);
                }
            }
            else
            {
                if(Directory.Exists(pak_path) && Directory.Exists(dir_path))
                {
                    if(save_modal)
                    {
                        FileFolderDialog ffd = new FileFolderDialog();
                        ffd.Dialog.Title = "Please select rebuild folder";

                        if(ffd.ShowDialog() == DialogResult.OK)
                        {
                            output_path = ffd.SelectedPath;
                        }
                    }

                    DirectoryInfo di = new DirectoryInfo(pak_path);
                    FileInfo[] fi = di.GetFiles("*.pak");

                    string[] dirs = Directory.GetDirectories(dir_path);

                    if (fi.Length > 0 && dirs.Length > 0)
                    {
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = fi.Length - 1;

                        string result = "";

                        for (int i = 0; i < fi.Length; i++)
                        {
                            for (int j = 0; j < dirs.Length; j++)
                            {
                                string check = get_file_name(dirs[j]);
                                if (fi[i].Name.Contains(check) && check.Length == fi[i].Name.Length - 4)
                                {
                                    var Thread = new System.Threading.Thread(
                                        () =>
                                        {
                                            result = RepackArchive(fi[i].FullName, output_path + "\\" + fi[i].Name, dirs[j], false);
                                        }
                                    );
                                    Thread.Start();
                                    Thread.Join();
                                    listBox1.Items.Add(result);
                                }
                            }

                            progressBar1.Value = i;
                        }
                    }
                    else listBox1.Items.Add("Please check pak files or rebuild's folders.");
                    
                }
            }           
        }

        private void Packer_Tool_Form_Load(object sender, EventArgs e)
        {
            onlyOneRB.Checked = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bool save_modal = checkBox1.Checked;

            string output_path = textBox1.Text;

            string pak_path = textBox1.Text;

            string dir_path = textBox2.Text;

            if (onlyOneRB.Checked)
            {
                if (File.Exists(pak_path) && Directory.Exists(dir_path))
                {
                    if (save_modal)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Filter = "PAK file | *.pak";

                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            output_path = sfd.FileName;
                        }
                    }

                    string result = RepackNew(pak_path, output_path, dir_path);

                    MessageBox.Show(result);
                }
            }
            else
            {
                if (Directory.Exists(pak_path) && Directory.Exists(dir_path))
                {
                    if (save_modal)
                    {
                        FileFolderDialog ffd = new FileFolderDialog();
                        ffd.Dialog.Title = "Please select folder with rebuilt archives";

                        if (ffd.ShowDialog() == DialogResult.OK)
                        {
                            output_path = ffd.SelectedPath;
                        }
                    }

                    DirectoryInfo di = new DirectoryInfo(pak_path);
                    FileInfo[] fi = di.GetFiles("*.pak");

                    string[] dirs = Directory.GetDirectories(dir_path);

                    if (fi.Length > 0 && dirs.Length > 0)
                    {
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = fi.Length - 1;

                        string result = "";

                        for (int i = 0; i < fi.Length; i++)
                        {
                            for (int j = 0; j < dirs.Length; j++)
                            {
                                string check = get_file_name(dirs[j]);
                                if (fi[i].Name.Contains(check) && check.Length == fi[i].Name.Length - 4)
                                {
                                    var Thread = new System.Threading.Thread(
                                        () =>
                                        {
                                            result = RepackNew(fi[i].FullName, output_path + "\\" + fi[i].Name, dirs[j]);
                                        }
                                    );
                                    Thread.Start();
                                    Thread.Join();
                                    listBox1.Items.Add(result);
                                }
                            }

                            progressBar1.Value = i;
                        }
                    }
                    else listBox1.Items.Add("Please check pak files or rebuild's folders.");

                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            bool save_modal = checkBox1.Checked;

            string output_path = textBox1.Text;

            string pak_path = textBox1.Text;

            string dir_path = textBox2.Text;

            if (onlyOneRB.Checked)
            {
                if (File.Exists(pak_path) && Directory.Exists(dir_path))
                {
                    if (save_modal)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Filter = "PAK file | *.pak";

                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            output_path = sfd.FileName;
                        }
                    }

                    string result = UnpackArchive(pak_path, dir_path);

                    MessageBox.Show(result);
                }
            }
            else
            {
                if (Directory.Exists(pak_path) && Directory.Exists(dir_path))
                {
                    DirectoryInfo di = new DirectoryInfo(pak_path);
                    FileInfo[] fi = di.GetFiles("*.pak");

                    if (fi.Length > 0)
                    {
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = fi.Length - 1;

                        string result = "";

                        for (int i = 0; i < fi.Length; i++)
                        {
                                    var Thread = new System.Threading.Thread(
                                        () =>
                                        {
                                            result = UnpackArchive(fi[i].FullName, dir_path);
                                        }
                                    );
                                    Thread.Start();
                                    Thread.Join();
                                    listBox1.Items.Add(result);

                            progressBar1.Value = i;
                        }
                    }
                    else listBox1.Items.Add("Please check pak files or rebuild's folders.");
                }
            }
        }
    }
}
