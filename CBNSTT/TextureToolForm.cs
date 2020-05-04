using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;

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
            public byte[] kxt_code; //Для kxt файлов
            public string format; //Пишется формат текстуры

            public formats() { }

            public formats(byte[] _header, byte[] _code, byte[] _kxt_code, string _format)
            {
                this.header = _header;
                this.code = _code;
                this.kxt_code = _kxt_code;
                this.format = _format;
            }

        }

        List<formats> check_format = new List<formats>();

        public void add_format(byte[] header, byte[] code, byte[] kxt_code, string format)
        {
            check_format.Add(new formats(header, code, kxt_code, format));
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

        private static Image RotateImg(Image img)
        {
            //create an empty Bitmap image
            Bitmap bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            //turn the Bitmap into a Graphics object
            Graphics gfx = Graphics.FromImage(bmp);

            //now we set the rotation point to the center of our image
            gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);

            //now rotate the image
            gfx.RotateTransform(180);

            gfx.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);

            //set the InterpolationMode to HighQualityBicubic so to ensure a high
            //quality image once it is transformed to the specified size
            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //now draw our new image onto the graphics object
            gfx.DrawImage(img, new Point(0, 0));

            //dispose of our Graphics object
            gfx.Dispose();

            //return the image
            return bmp;
        }
        private void ExportBtn_Click(object sender, EventArgs e)
        {
            try
            {
                FileFolderDialog fbd = new FileFolderDialog();

                if(fbd.ShowDialog() == DialogResult.OK)
                {
                    bool UseTool = File.Exists(MainForm.filePath);

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
                                        //header = check_format[index].header;
                                        header = new byte[check_format[index].kxt_code.Length];
                                        Array.Copy(check_format[index].kxt_code, 0, header, 0, header.Length);
                                        //header = check_format[index].kxt_code;

                                        byte[] tmp = new byte[2];
                                        tmp = BitConverter.GetBytes(width);
                                        //Array.Copy(tmp, 0, header, 28, tmp.Length);
                                        Array.Copy(tmp, 0, header, 36, tmp.Length);
                                        tmp = new byte[2];
                                        tmp = BitConverter.GetBytes(height);
                                        //Array.Copy(tmp, 0, header, 24, tmp.Length);
                                        Array.Copy(tmp, 0, header, 40, tmp.Length);
                                        tmp = new byte[2];
                                        tmp = BitConverter.GetBytes(1);
                                        //Array.Copy(tmp, 0, header, 44, tmp.Length);
                                        Array.Copy(tmp, 0, header, 56, tmp.Length);
                                        size = width * height;

                                        switch(check_format[index].format)
                                        {
                                            case "DXT1":
                                                size /= 2;
                                                break;

                                            case "ARGB_8888":
                                                size *= 4;
                                                break;
                                        }

                                        tmp = BitConverter.GetBytes(size);
                                        Array.Copy(tmp, 0, header, header.Length - 4, tmp.Length);

                                        string pvr_path = fi[i].FullName.Remove(fi[i].FullName.Length - 3, 3) + "ktx";

                                        if (File.Exists(pvr_path)) File.Delete(pvr_path); //Чтобы прога из-за такой тупости не упала

                                        FileStream fw = new FileStream(pvr_path, FileMode.CreateNew);
                                        fw.Write(header, 0, header.Length);
                                        fw.Write(content, 0, content.Length);
                                        fw.Close();
                                        
                                        if (UseTool)
                                        {
                                            string path = GetFilePath(fi[i].FullName);
                                            string file_name = fi[i].Name;

                                            if (File.Exists(path + MainForm.slash + "tmp" + i.ToString() + ".ktx")) File.Delete(path + MainForm.slash + "tmp" + i.ToString() + ".ktx");

                                            File.Move(fi[i].FullName.Remove(fi[i].FullName.Length - 4, 4) + ".ktx", path + MainForm.slash + "tmp" + i.ToString() + ".ktx");

                                            arg = String.Format("-fd {0} \"{1}\" \"{2}\"", "ARGB_8888", path + MainForm.slash + "tmp" + i.ToString() + ".ktx", path + MainForm.slash + "tmp" + i.ToString() + ".png");
                                            

                                            Process exec = new Process();

                                            ProcessStartInfo start_info = new ProcessStartInfo(MainForm.filePath, arg);
                                            start_info.WindowStyle = ProcessWindowStyle.Minimized;
                                            exec.StartInfo = start_info;
                                            exec.Start();
                                            exec.WaitForExit();

                                            if (File.Exists(fi[i].FullName.Remove(fi[i].FullName.Length - 4, 4) + ".png")) File.Delete(fi[i].FullName.Remove(fi[i].FullName.Length - 4, 4) + ".png");
                                            if (File.Exists(path + MainForm.slash + "tmp" + i.ToString() + ".ktx")) File.Delete(path + MainForm.slash + "tmp" + i.ToString() + ".ktx");

                                            tmp = File.ReadAllBytes(path + MainForm.slash + "tmp" + i.ToString() + ".png");
                                        
                                            MemoryStream ms = new MemoryStream(tmp);
                                            if (File.Exists(path + MainForm.slash + "tmp" + i.ToString() + ".png")) File.Delete(path + MainForm.slash + "tmp" + i.ToString() + ".png");
                                            Image img = Image.FromStream(ms);
                                            img.RotateFlip(RotateFlipType.RotateNoneFlipY);
                                            FileStream fws = new FileStream(fi[i].FullName.Remove(fi[i].FullName.Length - 4, 4) + ".png", FileMode.CreateNew);
                                            img.Save(fws, System.Drawing.Imaging.ImageFormat.Png);
                                            fws.Close();
                                            img.Dispose();
                                            img = null;
                                            ms.Close();
                                        }

                                        listBox1.Items.Add("File " + fi[i].Name + " successfully exported");

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

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                //bool UseTool = File.Exists(AppDomain.CurrentDomain.BaseDirectory + MainForm.slash + "PVRTexToolCLI");
                bool UseTool = File.Exists(MainForm.filePath);

                DirectoryInfo di = new DirectoryInfo(fbd.SelectedPath);
                    FileInfo[] fi = di.GetFiles("*.igz");
                    FileInfo[] pngfi = di.GetFiles("*.png");
                    FileInfo[] pvrfi = di.GetFiles("*.ktx");
                    FileInfo[] importfi = pvrfi;

                    if(fi.Length > 0 && (fi.Length == pngfi.Length) && (fi.Length == pvrfi.Length))
                    {
                        if (MessageBox.Show("KTX (Yes) or PNG (No)?", "Choose file format", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        {
                            importfi = pngfi;
                        }
                        else UseTool = false;
                    }
                    else if(fi.Length > 0 && fi.Length == pngfi.Length && fi.Length != pvrfi.Length)
                    {
                        importfi = pngfi;
                    }
                    else if(fi.Length > 0 && fi.Length == pngfi.Length && fi.Length != pvrfi.Length)
                    {
                        importfi = pvrfi;
                        UseTool = false;
                    }

                    if (!UseTool && importfi[0].Extension == ".png")
                    {
                        listBox1.Items.Add("Need a AMD compressonator for convert png files into ktx!");
                        return;
                    }

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

                                    br.BaseStream.Seek(offsets[0], SeekOrigin.Begin);

                                    int offset = offsets[0];

                                    byte[] check = null;

                                    while (offset < offsets[1])
                                    {
                                        if (offset >= offsets[1])
                                        {
                                            listBox1.Items.Add("This is not a texture file");
                                            goto fail;
                                        }
                                        check = br.ReadBytes(4);
                                        if (Encoding.ASCII.GetString(check) == "EXID") break;
                                        offset += 8;
                                        br.BaseStream.Seek(4, SeekOrigin.Current);
                                        check = br.ReadBytes(4);
                                        offset += BitConverter.ToInt32(check, 0) - 8;
                                        br.BaseStream.Seek(BitConverter.ToInt32(check, 0) - 12, SeekOrigin.Current);
                                    }

                                    br.BaseStream.Seek(4, SeekOrigin.Current);
                                    int size = br.ReadInt32();
                                    size -= 16;
                                    br.BaseStream.Seek(4, SeekOrigin.Current);
                                    byte[] block = br.ReadBytes(size);
                                    offset = 0;

                                    byte[] code = null;

                                    int index = -1;
                                    bool found = false;

                                    while (offset < block.Length)
                                    {
                                        if (offset >= block.Length)
                                        {
                                            listBox1.Items.Add("This is not a texture file");
                                            goto fail;
                                        }

                                        code = new byte[4];
                                        Array.Copy(block, offset, code, 0, code.Length);
                                        
                                        for(int a = 0; a < check_format.Count; a++)
                                        {
                                            if(BitConverter.ToString(code) == BitConverter.ToString(check_format[a].code))
                                            {
                                                index = a;
                                                found = true;
                                                break;
                                            }
                                        }

                                        offset++;

                                        if (found) break;
                                    }

                                    br.BaseStream.Seek(offsets[1] + 0x48, SeekOrigin.Begin);
                                    short width = br.ReadInt16();
                                    short height = br.ReadInt16();
                                    short faces = br.ReadInt16(); //Faces?
                                    short mips = br.ReadInt16();
                                    short ar_mem = br.ReadInt16(); //Array members?

                                    if (faces == 1 && ar_mem == 1)
                                    {
                                        if (index != -1)
                                        {
                                            //br.BaseStream.Seek(offsets[2], SeekOrigin.Begin);

                                            string mod_file = importfi[o].FullName;
                                            if (UseTool)
                                            {
                                                string tmp_path = importfi[o].DirectoryName + MainForm.slash + "tmp" + o + ".png";
                                                if (File.Exists(tmp_path)) File.Delete(tmp_path);
                                                File.Copy(importfi[o].FullName, tmp_path);

                                                byte[] png_tmp = File.ReadAllBytes(tmp_path);
                                                MemoryStream ms = new MemoryStream(png_tmp);
                                                Image img = Image.FromStream(ms);
                                                img.RotateFlip(RotateFlipType.RotateNoneFlipY);
                                                if (File.Exists(tmp_path)) File.Delete(tmp_path);
                                                FileStream fws = new FileStream(tmp_path, FileMode.CreateNew);
                                                img.Save(fws, System.Drawing.Imaging.ImageFormat.Png);
                                                fws.Close();
                                                ms.Close();
                                                img.Dispose();
                                                img = null;

                                                Process exec = new Process();

                                                string argument = String.Format("-fd {0} -miplevels {1} \"{2}\" \"{3}\"", check_format[index].format, mips - 1, tmp_path, tmp_path.Remove(tmp_path.Length - 4, 4) + ".ktx");
                                                ProcessStartInfo start_info = new ProcessStartInfo(MainForm.filePath, argument);
                                                start_info.WindowStyle = ProcessWindowStyle.Minimized;
                                                exec.StartInfo = start_info;
                                                exec.Start();
                                                exec.WaitForExit();

                                                if (File.Exists(tmp_path)) File.Delete(tmp_path);
                                                if (File.Exists(importfi[o].FullName.Remove(importfi[o].FullName.Length - 4, 4) + ".ktx")) File.Delete(importfi[o].FullName.Remove(importfi[o].FullName.Length - 4, 4) + ".ktx");
                                                mod_file = importfi[o].FullName.Remove(importfi[o].FullName.Length - 4, 4) + ".ktx";
                                                if (File.Exists(tmp_path.Remove(tmp_path.Length - 4, 4) + ".ktx")) File.Move(tmp_path.Remove(tmp_path.Length - 4, 4) + ".ktx", mod_file);
                                            }
                                            //else
                                            //{
                                            #region PvrTex junk
                                            /*byte[] pvr_tex = File.ReadAllBytes(importfi[o].FullName);
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
                                            }*/
                                            #endregion

                                                byte[] kxt_tex = File.ReadAllBytes(mod_file);
                                                byte[] tmp = new byte[4];
                                                Array.Copy(kxt_tex, 28, tmp, 0, tmp.Length);

                                                string format = "Unknown";

                                                switch (BitConverter.ToInt32(tmp, 0))
                                                {
                                                    case 0x83F1:
                                                        //format = "BC1";
                                                        format = "DXT1";
                                                        break;

                                                    case 0x83F3:
                                                        //format = "BC3";
                                                        format = "DXT5";
                                                        break;

                                                    case 0x8058:
                                                        //format = "r8g8b8a8";
                                                        format = "ARGB_8888";
                                                        break;

                                                    case 0x8DBC:
                                                        format = "ATI2N";
                                                        break;
                                                }

                                                if (format != "Unknown")
                                                {
                                                    /*tmp = new byte[4];

                                                    Array.Copy(kxt_tex, 48, tmp, 0, tmp.Length);

                                                    int offs = 52 + BitConverter.ToInt32(tmp, 0);*/
                                                    int offs = 0x40;

                                                    if (sizes[2] == kxt_tex.Length - offs - (4 * mips))
                                                    {
                                                        br.BaseStream.Seek(0, SeekOrigin.Begin);
                                                        tmp = br.ReadBytes(offsets[2]);
                                                        br.Close();
                                                        fs.Close();

                                                        //TODO: Find out way for correctly close files stream...

                                                        if (File.Exists(fi[i].FullName)) File.Delete(fi[i].FullName);

                                                        FileStream fw = new FileStream(fi[i].FullName, FileMode.CreateNew);
                                                        fw.Write(tmp, 0, tmp.Length);

                                                        offset = 0x40; //offset to size block;

                                                    for (int m = 0; m < mips; m++)
                                                    {
                                                        tmp = new byte[4];
                                                        Array.Copy(kxt_tex, offset, tmp, 0, tmp.Length);
                                                        offset += 4;

                                                        size = BitConverter.ToInt32(tmp, 0);
                                                        tmp = new byte[size];
                                                        Array.Copy(kxt_tex, offset, tmp, 0, tmp.Length);
                                                        offset += size;

                                                        fw.Write(tmp, 0, tmp.Length);
                                                    }
                                                        tmp = null;

                                                        listBox1.Items.Add("File " + fi[i].Name + " successfully imported!");

                                                    if (File.Exists(importfi[o].FullName.Remove(importfi[o].FullName.Length - 4, 4) + ".ktx")) File.Delete(importfi[o].FullName.Remove(importfi[o].FullName.Length - 4, 4) + ".ktx");
                                                }
                                                    else listBox1.Items.Add("Something wrong. I'll check later.");
                                                }
                                                else listBox1.Items.Add("Unknown KTX file format. Please send me for research.");
                                            //}
                                        }
                                        else listBox1.Items.Add("Please send me this file: " + fi[i].Name + ". It needs check for correctly file format.");
                                    }
                                    else listBox1.Items.Add("Texture format this file " + fi[i].Name + " wasn't found.");
                                }
                                else listBox1.Items.Add("Unknown format. Please send me file " + fi[i].Name);

                                fail:
                                if (br != null) br.Close();
                                if (fs != null) fs.Close();
                            }
                        }
                    }
                }
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
            byte[] kxt_code_argb8888 = { 0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A, 0x01, 0x02, 0x03, 0x04, 0x01, 0x14, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x08, 0x19, 0x00, 0x00, 0x58, 0x80, 0x00, 0x00, 0x08, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] tmp = { 0x50, 0x56, 0x52, 0x03, 0x00, 0x00, 0x00, 0x00, 0x72, 0x67, 0x62, 0x61, 0x08, 0x08, 0x08, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            add_format(tmp, code_rgba8888, kxt_code_argb8888, "ARGB_8888"); //8888RGBA
            tmp = null;

            byte[] code_bc1 = { 0xCD, 0x06, 0x3B, 0x9D };
            byte[] code_bc1_switch = { 0x51, 0x28, 0x28, 0x1B };
            byte[] kxt_code_bc1 = { 0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A, 0x01, 0x02, 0x03, 0x04, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF1, 0x83, 0x00, 0x00, 0x08, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] tmp2 = { 0x50, 0x56, 0x52, 0x03, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            add_format(tmp2, code_bc1, kxt_code_bc1, "DXT1"); //DXT1
            add_format(tmp2, code_bc1_switch, kxt_code_bc1, "DXT1"); //DXT1_switch
            tmp2 = null;

            byte[] code_bc3 = { 0x39, 0x88, 0x88, 0xDA };
            byte[] code_bc3_switch = { 0xCD, 0x6E, 0x45, 0x37 };
            byte[] kxt_code_bc3 = { 0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A, 0x01, 0x02, 0x03, 0x04, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF3, 0x83, 0x00, 0x00, 0x08, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] tmp4 = { 0x50, 0x56, 0x52, 0x03, 0x00, 0x00, 0x00, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            add_format(tmp4, code_bc3, kxt_code_bc3, "DXT5"); //DXT5
            add_format(tmp4, code_bc3_switch, kxt_code_bc3, "DXT5"); //DXT5_switch
            tmp4 = null;

            byte[] code_ati2n = { 0x18, 0x47, 0xB9, 0x78 };
            byte[] kxt_code_ati2n = { 0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A, 0x01, 0x02, 0x03, 0x04, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xBC, 0x8D, 0x00, 0x00, 0x08, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            add_format(null, code_ati2n, kxt_code_ati2n, "ATI2N");

            GC.Collect();
        }

        private void TextureToolForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            check_format.Clear();
        }
    }
}
