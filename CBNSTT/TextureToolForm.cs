using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;

namespace CBNSTT
{
    public partial class TextureToolForm : Form
    {
        public TextureToolForm()
        {
            InitializeComponent();
        }

        

        public class formats
        {
            public byte[] header; //Заголовки для PVR файлов
            public byte[] code; //EXID из текстурного файла
            public string format; //Пишется формат текстуры

            public formats() { }

            public formats(byte[] _header, byte[] _code, string _format)
            {
                this.header = _header;
                this.code = _code;
                this.format = _format;
            }

        }

        List<formats> check_format = new List<formats>();

        public void add_format(byte[] header, byte[] code, string format)
        {
            check_format.Add(new formats(header, code, format));
        }

        public string GetFilePath(string path)
        {
            int len = path.Length - 1;

            while(path[len] != MainForm.slash)
            {
                len--;

                if (len <= 0) return null;
            }

            string tmp = path.Remove(len, path.Length - len);

            return tmp;
        }

        public string GetFileName(string path)
        {
            int len = path.Length - 1;

            while (path[len] != MainForm.slash)
            {
                len--;

                if (len <= 0) return null;
            }

            string tmp = path.Remove(0, len + 1);

            return tmp;
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            try
            {
                FileFolderDialog fbd = new FileFolderDialog();

                if(fbd.ShowDialog() == DialogResult.OK)
                {
                    bool UseTool = File.Exists(String.Format("{0}{1}PVRTexToolCLI", AppDomain.CurrentDomain.BaseDirectory, MainForm.slash));

                    DirectoryInfo di = new DirectoryInfo(fbd.SelectedPath);
                    FileInfo[] fi = di.GetFiles("*.igz");

                    string arg = "";

                    if (fi.Length > 0)
                    {
                        for(int i = 0; i < fi.Length; i++)
                        {
                            FileStream fs = new FileStream(fi[i].FullName, FileMode.Open);
                            BinaryReader br = new BinaryReader(fs);
                            byte[] header = br.ReadBytes(4);

                            if (ASCIIEncoding.ASCII.GetString(header) != "\x01ZGI") listBox1.Items.Add("File " + fi[i].Name + " doesn't support. Make sure this file for texture format.");
                            else
                            {
                                br.BaseStream.Seek(16, SeekOrigin.Begin);

                                int[] offsets = new int[3];
                                int[] sizes = new int[3];
                                int count = br.ReadInt32();
                                br.BaseStream.Seek(4, SeekOrigin.Current);                                

                                for(int j = 0; j < 3; j++) //Считываем смещения и размеры для 3-х блоков
                                {
                                    offsets[j] = br.ReadInt32();
                                    sizes[j] = br.ReadInt32();
                                    br.BaseStream.Seek(8, SeekOrigin.Current);
                                }

                                if (sizes[1] == 0xC0) //Обычно это длина блока с информацией о текстуре
                                {
                                    br.BaseStream.Seek(offsets[0], SeekOrigin.Begin);
                                    int offset = offsets[0];
                                    int block_sz = 0;

                                    byte[] code = null;

                                    int index = -1;

                                    for(int c = 0; c < count; c++)
                                    {
                                        header = br.ReadBytes(4);

                                        if(Encoding.ASCII.GetString(header) == "EXID")
                                        {
                                            br.BaseStream.Seek(8, SeekOrigin.Current);
                                            int block_off = br.ReadInt32() - 16; //For correct offset to pixel format
                                            br.BaseStream.Seek(block_off, SeekOrigin.Current);
                                            code = br.ReadBytes(4);

                                            for(int f = 0; f < check_format.Count; f++)
                                            {
                                                if(BitConverter.ToString(code) == BitConverter.ToString(check_format[f].code))
                                                {
                                                    index = f;
                                                    break;
                                                }
                                            }

                                            break;
                                        }

                                        br.BaseStream.Seek(4, SeekOrigin.Current);
                                        block_sz = br.ReadInt32();
                                        offset += block_sz - 8;
                                        br.BaseStream.Seek(block_sz - 12, SeekOrigin.Current);
                                    }

                                    if (index != -1)
                                    {
                                        br.BaseStream.Seek(offsets[1] + 72, SeekOrigin.Begin);
                                        short width = br.ReadInt16();
                                        short height = br.ReadInt16();
                                        short faces = br.ReadInt16(); //Не уверен насчёт faces
                                        short mips = br.ReadInt16(); //Количество мип-мапов
                                        short array_member = br.ReadInt16();

                                        br.BaseStream.Seek(22, SeekOrigin.Current);

                                        int size = br.ReadInt32();

                                        br.BaseStream.Seek(offsets[2], SeekOrigin.Begin);

                                        byte[] content = br.ReadBytes(sizes[2]);
                                        header = check_format[index].header;

                                        byte[] tmp = new byte[2];
                                        tmp = BitConverter.GetBytes(width);
                                        Array.Copy(tmp, 0, header, 28, tmp.Length);
                                        tmp = new byte[2];
                                        tmp = BitConverter.GetBytes(height);
                                        Array.Copy(tmp, 0, header, 24, tmp.Length);
                                        tmp = new byte[2];
                                        tmp = BitConverter.GetBytes(mips);
                                        Array.Copy(tmp, 0, header, 44, tmp.Length);

                                        string pvr_path = fi[i].FullName.Remove(fi[i].FullName.Length - 3, 3) + "pvr";

                                        if (File.Exists(pvr_path)) File.Delete(pvr_path); //Чтобы прога из-за такой тупости не упала

                                        FileStream fw = new FileStream(pvr_path, FileMode.CreateNew);
                                        fw.Write(header, 0, header.Length);
                                        fw.Write(content, 0, content.Length);
                                        fw.Close();
                                        
                                        if (UseTool)
                                        {
                                            string path = GetFilePath(fi[i].FullName);
                                            string file_name = fi[i].Name;

                                            if (File.Exists(path + MainForm.slash + "tmp" + i.ToString() + ".pvr")) File.Delete(path + MainForm.slash + "tmp" + i.ToString() + ".pvr");
                                            FileStream pvr = new FileStream(path + MainForm.slash + "tmp" + i.ToString() + ".pvr", FileMode.CreateNew);
                                            pvr.Write(header, 0, header.Length);
                                            pvr.Write(content, 0, content.Length);
                                            pvr.Close();

                                            File.Delete(fi[i].FullName.Replace(".igz", ".pvr"));


                                            arg += "\"" + AppDomain.CurrentDomain.BaseDirectory + MainForm.slash + "PVRTexToolCLI.exe\" -i \"" + fi[i].Directory + MainForm.slash + "tmp" + i + ".pvr\" -d -f r8g8b8a8 -flip y\r\n";
                                            arg += "del \"" + fi[i].Directory + MainForm.slash + "tmp" + i + ".Out.pvr\"\r\n";
                                            arg += "del \"" + fi[i].Directory + MainForm.slash + "tmp" + i + ".pvr\"\r\n";
                                            arg += "ren \"" + fi[i].Directory + MainForm.slash + "tmp" + i + ".png\" \"" + fi[i].Name.Replace(".igz", ".png") + "\"\r\n";
                                        }
                                        else listBox1.Items.Add("File " + fi[i].Name + " successfully exported");

                                        header = null;
                                        content = null;
                                    }
                                    else listBox1.Items.Add("Texture format of file " + fi[i].Name + " doesn't support tool");
                                }
                                else listBox1.Items.Add("Unknown format. Please send file " + fi[i].Name);
                            }

                            br.Close();
                            fs.Close();
                        }

                        if(UseTool && arg != "")
                        {
                            arg += "del \"" + AppDomain.CurrentDomain.BaseDirectory + MainForm.slash + "batnik.bat" + "\"";
                            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + MainForm.slash + "batnik.bat", arg, Encoding.GetEncoding(866));

                            string argument = "/c \"" + AppDomain.CurrentDomain.BaseDirectory + MainForm.slash + "batnik.bat\"";

                            Process exec = new Process();

                            ProcessStartInfo start_info = new ProcessStartInfo("CMD.EXE", argument);
                            exec.StartInfo = start_info;
                            exec.Start();
                            exec.WaitForExit();

                            listBox1.Items.Add("File(s) extracted and converted into png format");

                        }
                    }
                    else listBox1.Items.Add("Not found igz files");
                }
            }
            catch(Exception ex)
            {
                string error_str = ex.Data + "\t" + ex.Message + "\r\n";

                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + MainForm.slash + "session_error.log")) File.Delete(AppDomain.CurrentDomain.BaseDirectory + MainForm.slash + "session_error.log");

                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + MainForm.slash + "session_error.log", error_str);

                listBox1.Items.Add("Произошла ошибка. Отчёт сохранён в файл " + AppDomain.CurrentDomain.BaseDirectory + MainForm.slash + "season_error.log");
            }
        }

        private void ImportBtn_Click(object sender, EventArgs e)
        {
            /*try
            {*/
            FileFolderDialog fbd = new FileFolderDialog();

            // fbd.ShowNewFolderButton = false;

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    bool UseTool = File.Exists(AppDomain.CurrentDomain.BaseDirectory + MainForm.slash + "PVRTexToolCLI");

                    DirectoryInfo di = new DirectoryInfo(fbd.SelectedPath);
                    FileInfo[] fi = di.GetFiles("*.igz");
                    FileInfo[] pngfi = di.GetFiles("*.png");
                    FileInfo[] pvrfi = di.GetFiles("*.pvr");
                    FileInfo[] importfi = pvrfi;

                    if(fi.Length > 0 && (fi.Length == pngfi.Length) && (fi.Length == pvrfi.Length))
                    {
                        if (MessageBox.Show("PVR (Да) или PNG (Нет)?", "Выберите формат", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        {
                            importfi = pngfi;
                        }
                        else importfi = pngfi;
                    }
                    else if(fi.Length > 0 && fi.Length == pngfi.Length && fi.Length != pvrfi.Length)
                    {
                        importfi = pngfi;
                    }
                    else if(fi.Length > 0 && fi.Length == pngfi.Length && fi.Length != pvrfi.Length)
                    {
                        importfi = pvrfi;
                    }

                    if (!UseTool && importfi[0].Extension == ".png")
                    {
                        listBox1.Items.Add("Need a PVRTexTool for convert png files into pvr!");
                        goto fail;
                    }

                        string arg = "";

                for (int o = 0; o < importfi.Length; o++)
                {
                    for (int i = 0; i < fi.Length; i++)
                    {
                        if (importfi[o].FullName.Remove(importfi[o].FullName.Length - 4, 4) == fi[i].FullName.Remove(fi[i].FullName.Length - 4, 4))
                        {
                            FileStream fs = new FileStream(fi[i].FullName, FileMode.Open);
                            BinaryReader br = new BinaryReader(fs);
                            byte[] header = br.ReadBytes(4);

                            if (ASCIIEncoding.ASCII.GetString(header) != "\x01ZGI")
                            {
                                listBox1.Items.Add("File " + fi[i].Name + " doesn't support. Make sure that file is texture.");
                                br.Close();
                                fs.Close();
                            }
                            else
                            {
                                br.BaseStream.Seek(24, SeekOrigin.Begin);

                                int[] offsets = new int[3];
                                int[] sizes = new int[3];

                                for (int j = 0; j < 3; j++)
                                {
                                    offsets[j] = br.ReadInt32();
                                    sizes[j] = br.ReadInt32();
                                    br.BaseStream.Seek(8, SeekOrigin.Current);
                                }

                                if (sizes[1] == 0xC0)
                                {

                                    br.BaseStream.Seek(offsets[0] + 8, SeekOrigin.Begin);

                                    int offset = 0;

                                    for (int j = 0; j < 3; j++)
                                    {
                                        offset = br.ReadInt32();
                                        br.BaseStream.Seek(offset - 4, SeekOrigin.Current);
                                    }

                                    br.BaseStream.Seek(8, SeekOrigin.Current);

                                    byte[] code = br.ReadBytes(4);

                                    int index = -1;

                                    br.BaseStream.Seek(offsets[1] + 0x48, SeekOrigin.Begin);
                                    short width = br.ReadInt16();
                                    short height = br.ReadInt16();
                                    short faces = br.ReadInt16(); //Faces?
                                    short mips = br.ReadInt16();
                                    short ar_mem = br.ReadInt16(); //Array members?

                                    if (faces == 1 && ar_mem == 1)
                                    {
                                        int tmp_width = width;
                                        int tmp_height = height;

                                        br.BaseStream.Seek(22, SeekOrigin.Current);

                                        int size = br.ReadInt32();

                                        int r8g8b8a8_offset = 0, dxt1_offset = 0, dxt5_offset = 0;

                                        for (int m = 0; m < mips; m++)
                                        {
                                            r8g8b8a8_offset += (tmp_width * tmp_height * 4);
                                            dxt1_offset += ((tmp_width * tmp_height) / 2);
                                            dxt5_offset += (tmp_width * tmp_height);

                                            //if(tmp_width / 2)
                                            tmp_width /= 2;
                                            tmp_height /= 2;

                                            if (tmp_width == 0) tmp_width = 1;
                                            if (tmp_height == 0) tmp_height = 1;
                                        }

                                        if (r8g8b8a8_offset == size) index = 0;
                                        if (dxt1_offset == size) index = 1;
                                        if (dxt5_offset == size) index = 2;

                                        if (index != -1)
                                        {
                                            //br.BaseStream.Seek(offsets[2], SeekOrigin.Begin);


                                            if (UseTool)
                                            {
                                                //string path = GetFilePath(new_file_path);
                                                //string file_name = GetFileName(new_file_path);
                                                string tmp = importfi[i].DirectoryName + MainForm.slash + "tmp" + i + ".png";
                                                if (File.Exists(tmp)) File.Delete(tmp);
                                                File.Move(importfi[i].FullName, tmp);

                                                arg += "\"" + AppDomain.CurrentDomain.BaseDirectory + "PVRTexToolCLI\" -i \"" + tmp + "\" -o \"" + tmp.Replace(".png", ".pvr") + "\" -flip y -f " + check_format[index].format + ",UBN,lRGB -m " + mips.ToString() + "\r\n";
                                                arg += "if exist \"" + fi[i].Directory + MainForm.slash + "tmp" + i + ".Out.pvr\" del \"" + fi[i].Directory + MainForm.slash + "tmp" + i + ".Out.pvr\"\r\n";
                                                arg += "ren \"" + fi[i].Directory + MainForm.slash + "tmp" + i + ".png\" \"" + fi[i].Name.Replace(".igz", ".png") + "\"\r\n";
                                                arg += "ren \"" + fi[i].Directory + MainForm.slash + "tmp" + i + ".pvr\" \"" + fi[i].Name.Replace(".igz", ".pvr") + "\"\r\n";
                                            }
                                            else
                                            {
                                                byte[] pvr_tex = File.ReadAllBytes(importfi[o].FullName);
                                                byte[] tmp = new byte[8];
                                                Array.Copy(pvr_tex, 8, tmp, 0, tmp.Length);

                                                string format = "Unknown";

                                                switch (BitConverter.ToInt64(tmp, 0))
                                                {
                                                    case 7:
                                                        format = "BC1";
                                                        break;

                                                    case 9:
                                                        format = "BC2";
                                                        break;

                                                    case 11:
                                                        format = "BC3";
                                                        break;

                                                    case 0x808080861626772:
                                                        format = "r8g8b8a8";
                                                        break;
                                                }

                                                if (format != "Unknown")
                                                {
                                                    tmp = new byte[4];

                                                    Array.Copy(pvr_tex, 48, tmp, 0, tmp.Length);

                                                    int offs = 52 + BitConverter.ToInt32(tmp, 0);
                                                    if (size == pvr_tex.Length - offs)
                                                    {
                                                        br.BaseStream.Seek(0, SeekOrigin.Begin);
                                                        tmp = br.ReadBytes(offsets[2]);

                                                        br.Close();
                                                        fs.Close();

                                                        //TODO: Find out way for correctly close files stream...

                                                        if (File.Exists(fi[i].FullName)) File.Delete(fi[i].FullName);

                                                        FileStream fw = new FileStream(fi[i].FullName, FileMode.CreateNew);
                                                        fw.Write(tmp, 0, tmp.Length);

                                                        tmp = new byte[size];
                                                        Array.Copy(pvr_tex, offs, tmp, 0, tmp.Length);
                                                        fw.Write(tmp, 0, tmp.Length);
                                                        tmp = null;

                                                        listBox1.Items.Add("File " + fi[i].Name + " successfully imported!");
                                                    }
                                                    else listBox1.Items.Add("Something wrong. I'll check later.");
                                                }
                                                else listBox1.Items.Add("Unknown PVR file format. Please send me for research.");
                                            }
                                        }
                                        else listBox1.Items.Add("Please send me this file: " + fi[i].Name + ". It needs check for correctly file format.");
                                    }
                                    else listBox1.Items.Add("Texture format this file " + fi[i].Name + " wasn't found.");
                                }
                                else listBox1.Items.Add("Unknown format. Please send me file " + fi[i].Name);

                                if (br != null) br.Close();
                                if (fs != null) fs.Close();
                            }
                        }
                    }
                }

                        if (UseTool && arg != "")
                        {
                            arg += "del \"" + AppDomain.CurrentDomain.BaseDirectory + MainForm.slash + "batnik.bat" + "\"";
                            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + MainForm.slash + "batnik.bat", arg, Encoding.GetEncoding(866));

                            string argument = "/c \"" + AppDomain.CurrentDomain.BaseDirectory + MainForm.slash + "batnik.bat\"";

                            Process exec = new Process();

                            ProcessStartInfo start_info = new ProcessStartInfo("CMD.EXE", argument);
                            exec.StartInfo = start_info;
                            exec.Start();
                            exec.WaitForExit();

                        listBox1.Items.Add("Convertion complete. Replacing textures in igz files...");

                        for(int i = 0; i < fi.Length; i++)
                        {
                        //System.Threading.Thread.Sleep(1000);
                        string path = fi[i].FullName.Replace(".igz", ".pvr");

                            FileStream fs = new FileStream(fi[i].FullName, FileMode.Open);
                            BinaryReader br = new BinaryReader(fs);

                            byte[] header = br.ReadBytes(4);

                                br.BaseStream.Seek(24, SeekOrigin.Begin);

                                int[] offsets = new int[3];
                                int[] sizes = new int[3];

                                for (int j = 0; j < 3; j++)
                                {
                                    offsets[j] = br.ReadInt32();
                                    sizes[j] = br.ReadInt32();
                                    br.BaseStream.Seek(8, SeekOrigin.Current);
                                }

                                    br.BaseStream.Seek(0, SeekOrigin.Begin);
                                    byte[] tmp = br.ReadBytes(offsets[2]);

                            br.Close();
                            fs.Close();

                            if (File.Exists(fi[i].FullName)) File.Delete(fi[i].FullName);

                        if (File.Exists(path))
                        {

                            FileStream fw = new FileStream(fi[i].FullName, FileMode.CreateNew);
                            fw.Write(tmp, 0, tmp.Length);

                            byte[] pvr_tex = File.ReadAllBytes(path);
                            tmp = new byte[8];
                            Array.Copy(pvr_tex, 8, tmp, 0, tmp.Length);

                            tmp = new byte[4];

                            Array.Copy(pvr_tex, 48, tmp, 0, tmp.Length);

                            int offs = 52 + BitConverter.ToInt32(tmp, 0);

                            /*br.BaseStream.Seek(0, SeekOrigin.Begin);
                            tmp = br.ReadBytes(offsets[2]);*/

                            tmp = new byte[pvr_tex.Length - offs];
                            Array.Copy(pvr_tex, offs, tmp, 0, tmp.Length);
                            fw.Write(tmp, 0, tmp.Length);
                            tmp = null;

                            fw.Close();
                            File.Delete(path);
                            listBox1.Items.Add("File " + fi[i].Name + " successfully imported.");
                        }
                        else listBox1.Items.Add("File " + fi[i].Name + " didn't imported because pvr file doesn't exist.");
                        }

                        }
                    fail:
                    int error = -1;
                }
            /*}
            catch(Exception ex)
            {
                listBox1.Items.Add(ex.Data + ": " + ex.Message);
            }*/
        }

        private void TextureToolForm_Load(object sender, EventArgs e)
        {
            byte[] code_rgba8888 = { 0xDE, 0x08, 0x46, 0x99 };
            byte[] tmp = { 0x50, 0x56, 0x52, 0x03, 0x00, 0x00, 0x00, 0x00, 0x72, 0x67, 0x62, 0x61, 0x08, 0x08, 0x08, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            add_format(tmp, code_rgba8888, "r8g8b8a8"); //8888RGBA
            tmp = null;

            byte[] code_bc1 = { 0xCD, 0x06, 0x3B, 0x9D };
            byte[] code_bc1_switch = { 0x51, 0x28, 0x28, 0x1B };
            byte[] tmp2 = { 0x50, 0x56, 0x52, 0x03, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            add_format(tmp2, code_bc1, "BC1"); //DXT1
            add_format(tmp2, code_bc1_switch, "BC1"); //DXT1_switch
            tmp2 = null;

            byte[] code_bc3 = { 0x39, 0x88, 0x88, 0xDA };
            byte[] code_bc3_switch = { 0xCD, 0x6E, 0x45, 0x37 };
            byte[] tmp4 = { 0x50, 0x56, 0x52, 0x03, 0x00, 0x00, 0x00, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            add_format(tmp4, code_bc3, "BC3"); //DXT5
            add_format(tmp4, code_bc3_switch, "BC3"); //DXT5_switch
            tmp4 = null;

            byte[] code_ati2n = { 0x18, 0x47, 0xB9, 0x78 };

            GC.Collect();
        }

        private void TextureToolForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            check_format.Clear();
        }
    }
}
