using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CBNSTT
{
    public delegate void SendMessage(string message);
    public delegate void SendProgress(int progress);

    public class Threads
    {
        public event SendMessage SendMes;
        public event SendProgress SendProg;


        public void TextWorker(string full_file_name, string file_name, bool write, bool remove_txt)
        {
            int count = 0; //Subblocks from first blocks
            int[] offsets = new int[2];
            int[] sizes = new int[2];

            int str_count = 0; //Count of strings in TSTR block
            int str_size = 0; //Strings' length
            int size = 0; //TSTR block's length

            string[] strs; //Strings' massive
            int offset = 16; //Default string offset in lng file

            byte[] tmp;      //Buffer

            offset = 16;
            byte[] file = File.ReadAllBytes(full_file_name);
            tmp = new byte[4];

            Array.Copy(file, offset, tmp, 0, tmp.Length);
            count = BitConverter.ToInt32(tmp, 0);

            TextEditForm.FileStruct file_struct = new TextEditForm.FileStruct();
            file_struct.sub_blocks = new List<byte[]>();

            offset += 8;

            for (int j = 0; j < 2; j++)
            {
                tmp = new byte[4];
                Array.Copy(file, offset, tmp, 0, tmp.Length);
                offset += 4;
                offsets[j] = BitConverter.ToInt32(tmp, 0);

                tmp = new byte[4];
                Array.Copy(file, offset, tmp, 0, tmp.Length);
                offset += 12;
                sizes[j] = BitConverter.ToInt32(tmp, 0);
            }

            file_struct.head_pad = new byte[offsets[0]];
            Array.Copy(file, 0, file_struct.head_pad, 0, file_struct.head_pad.Length);

            file_struct.end_block = new byte[sizes[1]];
            Array.Copy(file, offsets[1], file_struct.end_block, 0, file_struct.end_block.Length);

            offset = 0;

            for (int k = 0; k < count; k++)
            {
                tmp = new byte[4];
                Array.Copy(file, offsets[0] + offset + 8, tmp, 0, tmp.Length);
                size = BitConverter.ToInt32(tmp, 0);

                tmp = new byte[size];
                Array.Copy(file, offsets[0] + offset, tmp, 0, tmp.Length);
                file_struct.sub_blocks.Add(tmp);

                offset += size;
            }

            tmp = new byte[4];
            Array.Copy(file_struct.sub_blocks[0], 4, tmp, 0, tmp.Length);
            str_count = BitConverter.ToInt32(tmp, 0);

            strs = new string[str_count];

            offset = 16;

            for (int k = 0; k < str_count; k++)
            {
                str_size = TextEditForm.get_string(file_struct.sub_blocks[0], offset);
                tmp = new byte[str_size - 1];

                Array.Copy(file_struct.sub_blocks[0], offset, tmp, 0, tmp.Length);
                strs[k] = Encoding.UTF8.GetString(tmp);

                if (strs[k].Contains("\r\n")) strs[k] = strs[k].Replace("\r\n", "</br>");
                else if (strs[k].Contains("\n")) strs[k] = strs[k].Replace("\n", "</n>");

                offset += TextEditForm.pad_size(str_size, 2);
            }

            if (write)
            {
                if (strs.Length > 0)
                {
                    if (File.Exists(full_file_name.Replace(".lng", ".txt"))) File.Delete(full_file_name.Replace(".lng", ".txt"));
                    FileStream fw = new FileStream(full_file_name.Replace(".lng", ".txt"), FileMode.CreateNew);
                    StreamWriter sw = new StreamWriter(fw, Encoding.UTF8);

                    for (int k = 0; k < strs.Length; k++)
                    {
                        sw.WriteLine(strs[k]);
                    }

                    sw.Close();
                    fw.Close();

                    file_struct.Dispose();

                    //listBox1.Items.Add("File " + file_name + " successfully extracted!");
                    SendMes("File " + file_name + " successfully extracted!");
                }
            }
            else
            {
                if (strs.Length > 0)
                {
                    if (File.Exists(full_file_name.ToLower().Replace(".lng", ".txt")))
                    {
                        string[] new_strs = File.ReadAllLines(full_file_name.ToLower().Replace(".lng", ".txt"));

                        if (new_strs.Length == strs.Length)
                        {
                            strs = new_strs;

                            byte[] header = { 0x54, 0x53, 0x54, 0x52 }; //Заголовок TSTR
                            byte[] count_b = BitConverter.GetBytes((int)strs.Length);
                            byte[] len_b = new byte[4]; //Потом посчитаю
                            byte[] off_b = BitConverter.GetBytes((int)16);

                            List<byte[]> tmp_mas = new List<byte[]>();
                            int bl_sz = 16;

                            for (int k = 0; k < str_count; k++)
                            {
                                if (strs[k].Contains("</br>") || strs[k].Contains("<br>"))
                                {
                                    if (strs[k].Contains("</br>")) strs[k] = strs[k].Replace("</br>", "\r\n");
                                    else strs[k] = strs[k].Replace("<br>", "\r\n");
                                }
                                else if (strs[k].Contains("</n>") || strs[k].Contains("<n>"))
                                {
                                    if (strs[k].Contains("</n>")) strs[k] = strs[k].Replace("</n>", "\n");
                                    else strs[k] = strs[k].Replace("<n>", "\n");
                                }

                                tmp = Encoding.UTF8.GetBytes(strs[k] + "\0");
                                size = TextEditForm.pad_size(tmp.Length, 2);
                                bl_sz += size;

                                tmp_mas.Add(tmp);
                                tmp_mas[k] = new byte[size];
                                Array.Copy(tmp, 0, tmp_mas[k], 0, tmp.Length);
                            }
                            len_b = BitConverter.GetBytes(bl_sz);

                            file_struct.sub_blocks[0] = new byte[bl_sz];

                            Array.Copy(header, 0, file_struct.sub_blocks[0], 0, header.Length);
                            Array.Copy(count_b, 0, file_struct.sub_blocks[0], 4, count_b.Length);
                            Array.Copy(len_b, 0, file_struct.sub_blocks[0], 8, len_b.Length);
                            Array.Copy(off_b, 0, file_struct.sub_blocks[0], 12, off_b.Length);

                            offset = 16;

                            for (int k = 0; k < str_count; k++)
                            {
                                Array.Copy(tmp_mas[k], 0, file_struct.sub_blocks[0], offset, tmp_mas[k].Length);
                                offset += tmp_mas[k].Length;
                            }

                            size = 0;

                            for (int k = 0; k < file_struct.sub_blocks.Count; k++)
                            {
                                size += file_struct.sub_blocks[k].Length;
                            }

                            sizes[0] = size;
                            offsets[1] = offsets[0] + size;

                            tmp = new byte[4];
                            tmp = BitConverter.GetBytes(sizes[0]);
                            Array.Copy(tmp, 0, file_struct.head_pad, 28, tmp.Length);

                            tmp = new byte[4];
                            tmp = BitConverter.GetBytes(offsets[1]);
                            Array.Copy(tmp, 0, file_struct.head_pad, 40, tmp.Length);

                            size += file_struct.head_pad.Length + file_struct.end_block.Length;
                            size = TextEditForm.pad_size(size, 2);

                            tmp = new byte[size];
                            offset = 0;
                            Array.Copy(file_struct.head_pad, 0, tmp, offset, file_struct.head_pad.Length);
                            offset += file_struct.head_pad.Length;

                            for (int k = 0; k < file_struct.sub_blocks.Count; k++)
                            {
                                Array.Copy(file_struct.sub_blocks[k], 0, tmp, offset, file_struct.sub_blocks[k].Length);
                                offset += file_struct.sub_blocks[k].Length;
                            }

                            Array.Copy(file_struct.end_block, 0, tmp, offset, file_struct.end_block.Length);
                            offset += file_struct.end_block.Length;

                            if (File.Exists(full_file_name)) File.Delete(full_file_name);
                            FileStream fw = new FileStream(full_file_name, FileMode.CreateNew);

                            fw.Write(tmp, 0, tmp.Length);

                            fw.Close();

                            file = null;
                            tmp = null;
                            header = null;
                            count_b = null;
                            len_b = null;
                            off_b = null;
                            tmp_mas.Clear();

                            file_struct.Dispose();
                            GC.Collect();
                            //listBox1.Items.Add("File " + file_name + " successfully modded!");
                            SendMes("File " + file_name + " successfully modded!");

                            if (remove_txt) File.Delete(full_file_name.ToLower().Replace(".lng", ".txt"));
                        }
                        else SendMes("Count of strings in file " + file_name + " doesn't fit count of strings in text file!");//listBox1.Items.Add("Count of strings in file " + file_name + " doesn't fit count of strings in text file!");
                    }
                }
            }
        }

        public void threadReplace(object parameters)
        {
            var param = parameters as List<string>;

            string sel_path = param[0];
            bool remove = bool.Parse(param[1]);

            DirectoryInfo di = new DirectoryInfo(sel_path);
            FileInfo[] fi = di.GetFiles("*.lng", SearchOption.AllDirectories);

            if (fi.Length > 0)
            {
                for (int i = 0; i < fi.Length; i++)
                {
                    TextWorker(fi[i].FullName, fi[i].Name, false, remove);
                }
            }
        }

        public void threadExtract(object parameters)
        {
            string sel_path = parameters as string;
            DirectoryInfo di = new DirectoryInfo(sel_path);
            FileInfo[] fi = di.GetFiles("*.lng", SearchOption.AllDirectories);

            if (fi.Length > 0)
            {
                for (int i = 0; i < fi.Length; i++)
                {
                    TextWorker(fi[i].FullName, fi[i].Name, true, false);
                }
            }
        }
    }
}
