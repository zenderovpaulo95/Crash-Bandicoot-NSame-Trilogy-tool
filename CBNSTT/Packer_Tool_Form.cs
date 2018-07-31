/**********************************************************************
 *   Утилита по сборке архивов игры Crash Bandicoot N. Same Trilogy   *
 *   Особая благодарность за помощь в разборе структуры архивов:      *
 *                      Neo_Kesha и SileNTViP                         *
 **********************************************************************/

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;


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
            public short order1;
            public short order2;
            public int size;
            public int c_size;              //Добавил для, возможно, более удобной сборки сжатых архивов.
            public short block_offset;      //Смещение в таблице 1 или 2
            public short compression_flag;  //Сжат или не сжат архив (Если значение равно 0x2000, то это LZMA сжатие)
            public string file_name;        //Имя файла
            public int index;               //Индексы (на всякий случай сохраню)

        };

        public class HeaderStruct : IDisposable
        {
            public byte[] header;          //IGA\x1A заголовок
            public int count;              //Количество элементов в заголовке (должно быть 11)
            public int table_size;         //Длина таблицы с файлами и таблицами о сжатых блоках
            public int file_count;         //Количество файлов
            public int chunks_sz;          //Размер одного выравненного сжатого куска (если их несколько, то они делятся на эту длину и считаются кусками)
            public int unknown1;           //Неизвестное значение (за что оно отвечает, я так и не понял)
            public int unknown2;           //Тоже неизвестно, за что отвечает.
            public int zero1;              //Тут постоянно 0. Возможно, всё-таки unknown2 должен быть типа long
            public int big_chunks_count;   //Количество элементов больших сжатых файлов
            public int small_chunks_count; //Количество элементов маленьких сжатых файлов
            public int name_offset;        //Смещение к таблице имени файлов
            public int zero2;              //Странное значение. Возможно, оно тоже относится к name_offset и оно должно быть типа long
            public int name_table_sz;      //Длина таблицы имени файлов
            public int one;                //Это значение постоянно равно единице

            public int[] IDs;              //Какие-то отсортированные идентификаторы для файлов

            public table[] file_table;         //Структура таблицы файлов
            public short[] big_chunks_table;   //Массив из типа Int16 с данными о таблице сжатых блоков больших файлов
            public byte[]  small_chunks_table; //Массив байтов для таблицы сжатых блоков маленьких файлов

            //Предполагаю, после таблицы small_shunks_table идёт выравнивание таблицы до размера, кратного 4...

            public byte[] unknown_data; //Не понимаю, за что отвечает последний кусок, но он постоянно равен 0x18

            public HeaderStruct() { }

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

        private void button1_Click(object sender, EventArgs e) //Обзор для папок к PAK файлам или выбор одного PAK файла
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

        //Пересортировка таблицы с файлами (для более удобного пересчитывания смещения файлов)
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

                        table_resort[m].offset = table_resort[m + 1].offset;
                        table_resort[m].size = table_resort[m + 1].size;
                        table_resort[m].c_size = table_resort[m + 1].c_size;
                        table_resort[m].order1 = table_resort[m + 1].order1;
                        table_resort[m].order2 = table_resort[m + 1].order2;
                        table_resort[m].block_offset = table_resort[m + 1].block_offset;
                        table_resort[m].compression_flag = table_resort[m + 1].compression_flag;
                        table_resort[m].file_name = table_resort[m + 1].file_name;
                        table_resort[m].index = table_resort[m + 1].index;

                        table_resort[m + 1].offset = tmp_offset;
                        table_resort[m + 1].order1 = tmp_order1;
                        table_resort[m + 1].order2 = tmp_order2;
                        table_resort[m + 1].block_offset = tmp_blk_off;
                        table_resort[m + 1].compression_flag = tmp_com_fl;
                        table_resort[m + 1].size = tmp_size;
                        table_resort[m + 1].c_size = tmp_c_size;
                        table_resort[m + 1].file_name = tmp_file_name;
                        table_resort[m + 1].index = tmp_index;
                    }
                }
            }
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

                return "Количество файлов в папке не соответствует количеству файлов в архиве! Найдено " + fi.Length + ", а должно быть " + head.file_count;
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
                head.file_table[i].c_size = -1; //Пригодится, если окажется, что флаг не равен -1
                head.file_table[i].block_offset = br.ReadInt16();
                head.file_table[i].compression_flag = br.ReadInt16();
                head.file_table[i].block_offset = -1;
                head.file_table[i].compression_flag = -1;
                head.file_table[i].index = i;
            }

             header_offset += (16 * head.file_count);
             new_head_offset += (16 * head.file_count);
             file_size += (16 * head.file_count);

            head.big_chunks_table = new short[1];
            if (head.big_chunks_count > 0)
            {
                head.big_chunks_table = new short[head.big_chunks_count];

                for (int i = 0; i < head.big_chunks_count; i++)
                {
                    head.big_chunks_table[i] = br.ReadInt16();
                    header_offset += 2;
                }
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
                            return "Файл архива " + output_path + " не найден в папке!";
                        }
                    }
                    else
                    {
                        //Какой же убогий костыль благодаря сраному ограничению Windows 7!
                        br.Close();
                        bw.Close();
                        fs.Close();
                        fr.Close();

                        if (File.Exists(output_path + ".tmp")) File.Delete(output_path + ".tmp");
                        GC.Collect();
                        return "Длина пути к файлу больше 255 символов. Сократите, пожалуйста, путь к ресурсам для правильной работы утилиты.";
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

                //Запись данных в файл
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

        private void button2_Click(object sender, EventArgs e) //Выбор папки с с ресурсами
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
                    else listBox1.Items.Add("Проверьте на наличие файлов pak или папок для пересборки архивов");
                    
                }
            }           
        }

        private void Packer_Tool_Form_Load(object sender, EventArgs e)
        {
            onlyOneRB.Checked = true;
        }

    }
}
