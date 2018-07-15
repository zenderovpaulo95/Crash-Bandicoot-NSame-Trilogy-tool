using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;


namespace CBNSTT
{
    public partial class Packer_Tool_Form : Form
    {
        public Packer_Tool_Form()
        {
            InitializeComponent();
        }

        public class table
        {
            public int offset;
            public short hz1;
            public short hz1_5;
            public int size;
            public int c_size; //Добавил для, возможно, более удобной сборки сжатых архивов.
            public short hz2;
            public short hz3;
            public string file_name;
            public int index;

            public table() { }

            public table(int _offset, short _hz1, short _hz1_5, int _size, int _c_size, short _hz2, short _hz3, string _file_name, int _index)
            {
                this.offset = _offset;
                this.hz1 = _hz1;
                this.hz1_5 = _hz1_5;
                this.size = _size;
                this.c_size = _c_size;
                this.hz2 = _hz2;
                this.hz3 = _hz3;
                this.file_name = _file_name;
                this.index = _index;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (onlyOneRB.Checked)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "PAK файлы (*.pak) | *.pak";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = ofd.FileName;
                }
            }
            else
            {
                FileFolderDialog ffd = new FileFolderDialog();
                ffd.Dialog.Title = "Выберите папку с pak архивами";

                if(ffd.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = ffd.SelectedPath;
                }
            }
        }

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

        public static void ResortTable(ref List<table> table)
        {
            for (int k = 0; k < table.Count; k++)
            {
                for (int m = k - 1; m >= 0; m--)
                {
                    if (table[m].offset > table[m + 1].offset)
                    {
                        int tmp_offset = table[m].offset;
                        int tmp_size = table[m].size;
                        int tmp_c_size = table[m].c_size;
                        short tmp_hz1 = table[m].hz1;
                        short tmp_hz1_5 = table[m].hz1_5;
                        short tmp_hz2 = table[m].hz2;
                        short tmp_hz3 = table[m].hz3;
                        string tmp_file_name = table[m].file_name;
                        int tmp_index = table[m].index;

                        table[m].offset = table[m + 1].offset;
                        table[m].size = table[m + 1].size;
                        table[m].c_size = table[m + 1].c_size;
                        table[m].hz1 = table[m + 1].hz1;
                        table[m].hz1_5 = table[m + 1].hz1_5;
                        table[m].hz2 = table[m + 1].hz2;
                        table[m].hz3 = table[m + 1].hz3;
                        table[m].file_name = table[m + 1].file_name;
                        table[m].index = table[m + 1].index;

                        table[m + 1].offset = tmp_offset;
                        table[m + 1].hz1 = tmp_hz1;
                        table[m + 1].hz1_5 = tmp_hz1_5;
                        table[m + 1].hz2 = tmp_hz2;
                        table[m + 1].hz3 = tmp_hz3;
                        table[m + 1].size = tmp_size;
                        table[m + 1].c_size = tmp_c_size;
                        table[m + 1].file_name = tmp_file_name;
                        table[m + 1].index = tmp_index;
                    }
                }
            }
        }
                
        public string experimentalRepack(string input_path, string output_path, string dir_path)
        {
            if (File.Exists(output_path + ".tmp")) File.Delete(output_path + ".tmp");

            DirectoryInfo di = new DirectoryInfo(dir_path);
            FileInfo[] fi = di.GetFiles("*.*", SearchOption.AllDirectories);

            if(fi.Length > 0)
            {
                FileStream fr = new FileStream(input_path, FileMode.Open);
                BinaryReader br = new BinaryReader(fr);

                byte[] header = br.ReadBytes(4);
                long header_offset = 0;
                header_offset += 4;

                if (Encoding.ASCII.GetString(header) != "IGA\x1A")
                {
                    br.Close();
                    fr.Close();
                    return "Неверный pak файл для пересборки архивов";
                }

                FileStream fw = new FileStream(output_path + ".tmp", FileMode.CreateNew);
                BinaryWriter bw = new BinaryWriter(fw);

                bw.Write(header);

                int count = br.ReadInt32();
                bw.Write(count);
                header_offset += 4;
                int size = br.ReadInt32();
                bw.Write(size);
                header_offset += 4;
                int count_file = br.ReadInt32();
                bw.Write(count_file);
                header_offset += 4;
                int chunk = br.ReadInt32();
                bw.Write(chunk);
                header_offset += 4;
                float hz = br.ReadSingle();
                bw.Write(hz);
                header_offset += 4;
                int hz2 = br.ReadInt32();
                bw.Write(hz2);
                header_offset += 4;
                int hz3 = br.ReadInt32();
                bw.Write(hz3);
                header_offset += 4;
                int hz4 = br.ReadInt32();
                bw.Write(hz4);
                header_offset += 4;
                int hz5 = br.ReadInt32();
                bw.Write(hz5);
                header_offset += 4;
                long name_off = br.ReadInt64();
                bw.Write(name_off);
                header_offset += 8;
                int hz6 = br.ReadInt32();
                bw.Write(hz6);
                header_offset += 4;
                int hz7 = br.ReadInt32();
                bw.Write(hz7);
                header_offset += 4;

                byte[] hashes = br.ReadBytes(4 * count_file);
                bw.Write(hashes);
                hashes = new byte[16 * count_file];

                bw.Write(hashes); //Заготовка для таблицы с файлами

                hashes = null;

                header_offset += (4 * count_file);
                
                List<table> tabl = new List<table>();

                for (int i = 0; i < count_file; i++)
                {
                    tabl.Add(new table());
                    tabl[i].offset = br.ReadInt32();
                    tabl[i].hz1 = br.ReadInt16();
                    tabl[i].hz1_5 = br.ReadInt16();
                    tabl[i].size = br.ReadInt32();
                    tabl[i].c_size = -1; //Пригодится, если окажется, что флаг не равен -1
                    tabl[i].hz2 = br.ReadInt16();
                    tabl[i].hz3 = br.ReadInt16();
                    //tabl[i].hz2 = -1;
                    //tabl[i].hz3 = -1;
                    tabl[i].index = i;
                }

                header_offset += (16 * count_file);

                int last_sz = size - ((4 * count_file) + (16 * count_file));
                header_offset += last_sz;

                byte[] last_chunk = br.ReadBytes(last_sz);

                bw.Write(last_chunk);
                last_chunk = null;

                br.BaseStream.Seek(name_off, SeekOrigin.Begin);
                byte[] name_block = br.ReadBytes((int)fr.Length - (int)name_off);
                int off = 0;
                int offf = 0;
                int counter = 0;

                int index = 0;

                byte[] tmp;

                //string[] files = new string[count_file];

                for (int j = 0; j < count_file; j++)
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

                    string tmp_str = Encoding.ASCII.GetString(tmp);
                    tabl[j].file_name = tmp_str;
                    if (tabl[j].file_name.Contains("/")) tabl[j].file_name = tabl[j].file_name.Replace('/', '\\');

                    //tabl[j].file_name = files[j];
                }

                List<table> new_table = new List<table>();

                for (int i = 0; i < tabl.Count; i++)
                {
                    new_table.Add(new table());
                    new_table[i].offset = tabl[i].offset;
                    new_table[i].size = tabl[i].size;
                    new_table[i].c_size = tabl[i].c_size;
                    new_table[i].hz1 = tabl[i].hz1;
                    new_table[i].hz2 = tabl[i].hz2;
                    new_table[i].hz3 = tabl[i].hz3;
                    new_table[i].file_name = tabl[i].file_name;
                    new_table[i].index = tabl[i].index;
                }

                ResortTable(ref new_table);
                
                int found_ind = -1;

                int offset = new_table[0].offset;

                br.BaseStream.Seek(header_offset, SeekOrigin.Begin);
                tmp = br.ReadBytes(offset - (int)header_offset);
                bw.Write(tmp);
                tmp = null;

                for(int c = 0; c < count_file; c++)
                {
                    found_ind = -1;

                    new_table[c].offset = offset;

                    for(int ind = 0; ind < fi.Length; ind++)
                    {
                        if(new_table[c].file_name.ToUpper().IndexOf(fi[ind].Name.ToUpper()) > 0)
                        {
                            found_ind = ind;
                            break;
                        }
                    }

                    if(found_ind != -1)
                    {
                        byte[] content = new byte[pad_size((int)fi[found_ind].Length, 0x800)];
                        FileStream fs = new FileStream(fi[found_ind].FullName, FileMode.Open);
                        fs.Read(content, 0, (int)fs.Length);
                        fs.Close();
                        bw.Write(content);
                        new_table[c].size = content.Length;
                        new_table[c].hz2 = -1;
                        new_table[c].hz3 = -1;
                        offset += content.Length;
                        content = null;
                    }
                    else
                    {
                        int idx = 0;

                        while (tabl[idx].index != new_table[c].index)
                        {
                            idx++;

                            if(idx > count_file)
                            {
                                bw.Close();
                                fw.Close();
                                br.Close();
                                fr.Close();

                                File.Delete(output_path + ".tmp");

                                return "Файл не удалось пересобрать";
                            }
                        }

                        int size_file = tabl[idx].size;

                        if(tabl[idx].hz3 != -1)
                        {
                            if (c + 1 < count_file) size_file = new_table[c + 1].offset - tabl[idx].offset;
                            else size_file = (int)name_off - tabl[idx].offset;
                        }

                        br.BaseStream.Seek(tabl[idx].offset, SeekOrigin.Begin);
                        byte[] content = br.ReadBytes(size_file);
                        bw.Write(content);
                        content = null;
                        offset += size_file;
                    }
                }

                name_off = offset;

                bw.Write(name_block);
                br.Close();
                fr.Close();

                name_block = null;

                bw.BaseStream.Seek(40, SeekOrigin.Begin);
                bw.Write(name_off);

                offset = 56 + (4 * count_file);
                bw.BaseStream.Seek(offset, SeekOrigin.Begin);

                for(int c = 0; c < count_file; c++)
                    for(int d = 0; d < count_file; d++)
                    {
                        if(tabl[c].index == new_table[d].index)
                        {
                            bw.Write(new_table[d].offset);
                            bw.Write(new_table[d].hz1);
                            bw.Write(new_table[d].hz1_5);
                            bw.Write(new_table[d].size);
                            bw.Write(new_table[d].hz2);
                            bw.Write(new_table[d].hz3);
                        }
                    }

                new_table.Clear();
                tabl.Clear();
                new_table = null;
                tabl = null;

                bw.Close();
                fw.Close();

                GC.Collect();

                if (File.Exists(output_path)) File.Delete(output_path);

                File.Move(output_path + ".tmp", output_path);
            }

            return "Архив " + output_path + " пересобран успешно.";
        }

        public string RepackArchive(string input_path, string output_path, string dir_path, bool compress, bool readtable)
        {
            #region
            if (File.Exists(output_path + ".tmp")) File.Delete(output_path + ".tmp");

                DirectoryInfo di = new DirectoryInfo(dir_path);
                FileInfo[] fi = di.GetFiles("*.*", SearchOption.AllDirectories);

                FileStream fr = new FileStream(input_path, FileMode.Open);
                FileStream fs = new FileStream(output_path + ".tmp", FileMode.CreateNew);
                BinaryReader br = new BinaryReader(fr);
                BinaryWriter bw = new BinaryWriter(fs);

                long header_offset = 0;

                byte[] header = br.ReadBytes(4);
                    header_offset += 4;
                int count = br.ReadInt32();
                    header_offset += 4;
                int size = br.ReadInt32();
                    header_offset += 4;
                int count_file = br.ReadInt32();
                    header_offset += 4;
                int chunk = br.ReadInt32();
                    header_offset += 4;
                float hz = br.ReadSingle();
                    header_offset += 4;
                int hz2 = br.ReadInt32();
                    header_offset += 4;
                int hz3 = br.ReadInt32();
                    header_offset += 4;
                int hz4 = br.ReadInt32();
                    header_offset += 4;
                int hz5 = br.ReadInt32();
                    header_offset += 4;
                long name_off = br.ReadInt64();
                    header_offset += 8;
                int hz6 = br.ReadInt32();
                    header_offset += 4;
                int hz7 = br.ReadInt32();
                    header_offset += 4;

                byte[] hashes = br.ReadBytes(4 * count_file);

                    header_offset += (4 * count_file);

            if (fi.Length != count_file)
            {
                br.Close();
                bw.Close();
                fs.Close();
                fr.Close();
                File.Delete(output_path + ".tmp");

                return "Количество файлов в папке не соответствует количеству файлов в архиве! Найдено " + fi.Length + ", а должно быть " + count_file;
            }
                    List<table> tabl = new List<table>();

                    for (int i = 0; i<count_file; i++)
                    {
                        tabl.Add(new table());
                        tabl[i].offset = br.ReadInt32();
                        tabl[i].hz1 = br.ReadInt16();
                        tabl[i].hz1_5 = br.ReadInt16();
                        tabl[i].size = br.ReadInt32();
                        tabl[i].c_size = -1; //Пригодится, если окажется, что флаг не равен -1
                        tabl[i].hz2 = br.ReadInt16();
                        tabl[i].hz3 = br.ReadInt16();
                        if (!compress)
                        {
                            tabl[i].hz2 = -1;
                            tabl[i].hz3 = -1;
                        }
                        tabl[i].index = i;
                    }

                        header_offset += (16 * count_file);

                    int last_sz = size - ((4 * count_file) + (16 * count_file));
                        header_offset += last_sz;

                    byte[] last_chunk = br.ReadBytes(last_sz);

                    br.BaseStream.Seek(name_off, SeekOrigin.Begin);
                    byte[] name_block = br.ReadBytes((int)fr.Length - (int)name_off);
                    int off = 0;
                    int offf = 0;
                    int counter = 0;

                    int index = 0;

                    byte[] tmp;

                    string[] files = new string[count_file];

                    for (int j = 0; j<count_file; j++)
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

                        string tmp_str = Encoding.ASCII.GetString(tmp);
                        files[j] = tmp_str;
                        if (files[j].Contains("/")) files[j] = files[j].Replace('/', '\\');

                        tabl[j].file_name = files[j];
                    }

                    List<table> new_table = new List<table>();

                    for(int i = 0; i<tabl.Count; i++)
                    {
                        new_table.Add(new table());
                        new_table[i].offset = tabl[i].offset;
                        new_table[i].size = tabl[i].size;
                        new_table[i].c_size = tabl[i].c_size;
                        new_table[i].hz1 = tabl[i].hz1;
                        new_table[i].hz2 = tabl[i].hz2;
                        new_table[i].hz3 = tabl[i].hz3;
                        new_table[i].file_name = tabl[i].file_name;
                        new_table[i].index = tabl[i].index;
                    }

                    ResortTable(ref new_table);

                int[] sizes = new int[count_file];

                int c_count = -1; //Для расчёта сжатых блоков

                for (int i = 0; i < new_table.Count; i++)
                {
                    index = 0;

                    bool res = false;

                    while (!res)
                    {
                        if (fi[index].FullName.ToUpper().IndexOf(new_table[i].file_name.ToUpper()) > 0)
                        {
                            sizes[i] = (int)fi[index].Length; //Надо будет убрать это недоразумение с sizes
                            new_table[i].size = (int)fi[index].Length;

                            if (new_table[i].hz3 != -1)
                            {
                                //Пока запилю как идеальный вариант, когда сжатые блоки весят меньше 2КБ 
                                if (new_table[i].size > 0x8000)
                                {
                                    c_count = pad_size(new_table[i].size, 0x8000) / 0x8000;

                                    new_table[i].c_size = 0;

                                    byte[] content = File.ReadAllBytes(fi[index].FullName);
                                    //byte[] c_buffer = new byte[1];
                                    byte[] buffer;
                                    int buf_size = 0;
                                    int c_off = 0;

                                    //byte[] pad_buffer;

                                    for (int c = 0; c < c_count; c++)
                                    {
                                        buf_size = 0x8000;
                                        buffer = new byte[buf_size];

                                        if (buffer.Length > content.Length - c_off)
                                        {
                                            buf_size = content.Length - c_off;
                                            buffer = new byte[buf_size];
                                        }

                                        Array.Copy(content, c_off, buffer, 0, buffer.Length);
                                        int c_size = -1;
                                        using (MemoryStream mem_content = new MemoryStream(buffer))
                                        {
                                            using (MemoryStream mem_c_content = new MemoryStream())
                                            {
                                                SevenZip.Compression.LZMA.Encoder encode = new SevenZip.Compression.LZMA.Encoder();
                                                encode.Code(mem_content, mem_c_content, -1, -1, null);
                                                c_size = (int)mem_c_content.Length + 7;
                                                //position = (int)mem_c_content.Position;
                                            }
                                        }

                                        new_table[i].c_size += pad_size(c_size, 0x800);
                                        c_off += buf_size;

                                    }
                                }
                                else
                                {
                                    int c_size = -1;
                                    byte[] content = File.ReadAllBytes(fi[index].FullName);
                                    MemoryStream mem_content = new MemoryStream(content);
                                    MemoryStream mem_c_content = new MemoryStream();
                                    SevenZip.Compression.LZMA.Encoder compressor = new SevenZip.Compression.LZMA.Encoder();
                                    compressor.Code(mem_content, mem_c_content, -1, -1, null);
                                    c_size = (int)mem_c_content.Length + 7;
                                    mem_c_content.Close();
                                    mem_content.Close();

                                    //c_b_size = (short)c_conent.Length;
                                    //new_table[i].c_size = pad_size(c_conent.Length + 7, 0x800);
                                    new_table[i].c_size = pad_size(c_size, 0x800);
                                }
                            }

                            res = true;
                            break;
                        }

                        index++;

                        if (index >= files.Length)
                        {
                            //index = 0;
                            br.Close();
                            bw.Close();
                            fs.Close();
                            fr.Close();

                            if (File.Exists(output_path + ".tmp")) File.Delete(output_path + ".tmp");
                            GC.Collect();
                            return "Файл архива " + output_path + " не найден в папке!";
                        }
                    }
                }

                int files_off = new_table[0].offset;
                br.BaseStream.Seek(header_offset, SeekOrigin.Begin);
                tmp = br.ReadBytes(files_off - (int)header_offset);
                //br.BaseStream.Seek()

                //int i_deb = 0;

                for (int i = 0; i < new_table.Count; i++)
                {
                    new_table[i].offset = files_off;
                    //new_table[i].size = sizes[i];
                    //i_deb = pad_size(sizes[i], chunk);
                    if (new_table[i].hz3 != -1 && new_table[i].c_size != -1) files_off += new_table[i].c_size;
                    else files_off += pad_size(new_table[i].size, chunk);
                }

                name_off = files_off;

                for (int f = 0; f < tabl.Count; f++)
                {
                    for (int j = 0; j < new_table.Count; j++)
                    {
                        if ((new_table[j].index == tabl[f].index))
                        {
                            tabl[f].offset = new_table[j].offset;
                            tabl[f].size = new_table[j].size;
                            //tabl[f].hz2 = 0;

                            break;
                        }
                    }
                }

                //Запись данных в файл
                bw.Write(header);
                bw.Write(count);
                bw.Write(size);
                bw.Write(count_file);
                bw.Write(chunk);
                bw.Write(hz);
                bw.Write(hz2);
                bw.Write(hz3);
                bw.Write(hz4);
                bw.Write(hz5);
                bw.Write(name_off);
                bw.Write(hz6);
                bw.Write(hz7);
                bw.Write(hashes);

                for (int i = 0; i < tabl.Count; i++)
                {
                    bw.Write(tabl[i].offset);
                    bw.Write(tabl[i].hz1);
                    bw.Write(tabl[i].hz1_5);
                    bw.Write(tabl[i].size);
                    bw.Write(tabl[i].hz2);
                    bw.Write(tabl[i].hz3);
                }

                bw.Write(last_chunk);
                bw.Write(tmp); //Остаток куска заголовка

                last_chunk = null;

                byte[] tmp_f;

                int idx = 0;
                bool res2;

                for (int i = 0; i < new_table.Count; i++)
                {
                    idx = 0;
                    res2 = false;

                    while (!res2)
                    {
                        if (fi[idx].FullName.ToUpper().IndexOf(new_table[i].file_name.ToUpper()) > 0)
                        {
                            if (new_table[i].hz3 != -1 && new_table[i].c_size != -1)
                            {
                                byte[] pad_buffer = null;
                                byte[] header_c = { 0x5D, 0x00, 0x80, 0x00, 0x00 };
                                if (new_table[i].size > 0x8000)
                                {
                                    c_count = pad_size(new_table[i].size, 0x8000) / 0x8000;

                                    new_table[i].c_size = 0;

                                    byte[] content = File.ReadAllBytes(fi[idx].FullName);
                                    //byte[] c_buffer = new byte[1];
                                    byte[] buffer;
                                    int buf_sz = 0;
                                    int c_off = 0;

                                    for (int c = 0; c < c_count; c++)
                                    {
                                        buf_sz = 0x8000;
                                        buffer = new byte[buf_sz];
                                        if (buffer.Length > content.Length - c_off)
                                        {
                                            buf_sz = content.Length - c_off;
                                            buffer = new byte[buf_sz];
                                        }

                                        Array.Copy(content, c_off, buffer, 0, buffer.Length);

                                        int c_size = -1;
                                        using (MemoryStream mem_content = new MemoryStream(buffer))
                                        {
                                            using (MemoryStream mem_c_content = new MemoryStream())
                                            {
                                                SevenZip.Compression.LZMA.Encoder encode = new SevenZip.Compression.LZMA.Encoder();
                                                encode.Code(mem_content, mem_c_content, -1, -1, null);
                                                c_size = (int)mem_c_content.Length + 7;
                                                buffer = mem_c_content.ToArray();
                                            }
                                        }


                                        pad_buffer = new byte[pad_size(c_size, 0x800)];
                                        new_table[i].c_size += pad_buffer.Length;

                                        byte[] b_c_size = new byte[2];
                                        b_c_size = BitConverter.GetBytes((short)buffer.Length);
                                        Array.Copy(b_c_size, 0, pad_buffer, 0, b_c_size.Length);
                                        Array.Copy(header_c, 0, pad_buffer, 2, header_c.Length);
                                        Array.Copy(buffer, 0, pad_buffer, 7, buffer.Length);
                                        bw.Write(pad_buffer, 0, pad_buffer.Length);
                                        c_off += buf_sz;

                                    }
                                    buffer = null;
                                    //c_buffer = null;
                                }
                                else
                                {
                                    byte[] content = File.ReadAllBytes(fi[idx].FullName);
                                    int c_size = -1;
                                    MemoryStream mem_content = new MemoryStream(content);
                                    MemoryStream mem_c_content = new MemoryStream();
                                    SevenZip.Compression.LZMA.Encoder compressor = new SevenZip.Compression.LZMA.Encoder();
                                    compressor.Code(mem_content, mem_c_content, -1, -1, null);
                                    c_size = (int)mem_c_content.Length + 7;
                                    content = mem_c_content.ToArray();
                                    mem_c_content.Close();
                                    mem_content.Close();

                                    pad_buffer = new byte[pad_size(c_size, 0x800)];

                                    byte[] b_size = new byte[2];
                                    b_size = BitConverter.GetBytes((short)content.Length);

                                    Array.Copy(b_size, 0, pad_buffer, 0, b_size.Length);
                                    Array.Copy(header_c, 0, pad_buffer, 2, header_c.Length);
                                    Array.Copy(content, 0, pad_buffer, 7, content.Length);

                                    bw.Write(pad_buffer, 0, pad_buffer.Length);
                                    pad_buffer = null;
                                    b_size = null;
                                    content = null;
                                    //c_b_size = (short)c_conent.Length;
                                    //new_table[i].c_size = pad_size(c_conent.Length, 0x800);
                                }
                            }
                            else
                            {
                                tmp = File.ReadAllBytes(fi[idx].FullName);
                                tmp_f = new byte[pad_size(tmp.Length, chunk)];
                                Array.Copy(tmp, 0, tmp_f, 0, tmp.Length);
                                bw.Write(tmp_f);
                                tmp = null;
                                tmp_f = null;
                            }
                            res2 = true;
                        }
                        idx++;
                    }
                }

                bw.Write(name_block);

                name_block = null;

            string info = "\r\n";

            if (readtable)
            {
                string result = "";
                for (int i = 0; i < new_table.Count; i++)
                {
                    result += (i + 1).ToString() + ". Offset = " + new_table[i].offset + "\thz1 = " + new_table[i].hz1;
                    result += "\thz1_5 = " + new_table[i].hz1_5 + "\tsize = " + new_table[i].size + "\thz2 = ";
                    result += new_table[i].hz2 + "\thz3 = " + new_table[i].hz3 + "\r\n";
                }

                File.WriteAllText(output_path.Replace(".pak", ".txt"), result);

                info += "Отсортированная таблица хранится в файле " + output_path.Replace(".pak", ".txt");
            }

            new_table.Clear();
                tabl.Clear();
                sizes = null;
                files = null;

                br.Close();
                bw.Close();
                fs.Close();
                fr.Close();

                if (File.Exists(output_path)) File.Delete(output_path);

                File.Move(output_path + ".tmp", output_path);

                GC.Collect();

                return "Файл " + output_path + " пересобран успешно!" + info;
            #endregion
        }

        static public string get_file_name(string path)
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

        private void button2_Click(object sender, EventArgs e)
        {
            //FolderBrowserDialog fbd = new FolderBrowserDialog();

            FileFolderDialog fbd = new FileFolderDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
               textBox2.Text = fbd.SelectedPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bool save_modal = checkBox1.Checked;
            bool compress = compressCB.Checked;
            bool get_list = checkBox2.Checked;

            string output_path = textBox1.Text;

            string pak_path = textBox1.Text;

            string dir_path = textBox2.Text; //Папка с ресурсами

            if (onlyOneRB.Checked)
            {
                if (File.Exists(pak_path) && Directory.Exists(dir_path))
                {
                    if (save_modal)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Filter = "PAK File | *.pak";

                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            output_path = sfd.FileName;
                        }
                    }

                    //string result = RepackArchive(pak_path, output_path, textBox2.Text, compress, get_list);

                    string result = experimentalRepack(pak_path, output_path, dir_path);

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
                        ffd.Dialog.Title = "Укажите папку для пересобранных архивов";

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
                                //if (dirs[j].Contains("\\" + fi[i].Name.Remove(fi[i].Name.Length - 4, 4)))
                                string check = get_file_name(dirs[j]);
                                if (fi[i].Name.Contains(check) && check.Length == fi[i].Name.Length - 4)
                                {
                                    //result = RepackArchive(fi[i].FullName, output_path + "\\" + fi[i].Name, dirs[j], compress, get_list);
                                    //System.Threading.Thread.Sleep(100);
                                    var Thread = new System.Threading.Thread(
                                        () =>
                                        {
                                            result = experimentalRepack(fi[i].FullName, output_path + "\\" + fi[i].Name, dirs[j]);
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
                    else listBox1.Items.Add("Проверьте на наличие файлов pak или папок для пересборки архивов");
                    
                }
            }           
        }

        private void Packer_Tool_Form_Load(object sender, EventArgs e)
        {
            onlyOneRB.Checked = true;
        }

        private void getTableBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Pak files (*.pak) | *.pak";

            if(ofd.ShowDialog() == DialogResult.OK)
            {
                RepackArchive(ofd.FileName, null, Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)), false, true);
            }
        }
    }
}
