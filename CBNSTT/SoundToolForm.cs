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
    public partial class SoundToolForm : Form
    {
        public SoundToolForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FileFolderDialog ffd = new FileFolderDialog();

            if(ffd.ShowDialog() == DialogResult.OK)
            {
                DirectoryInfo di = new DirectoryInfo(ffd.SelectedPath);

                FileInfo[] fi = di.GetFiles("*.*", SearchOption.AllDirectories);

                if(fi.Length > 0)
                {
                    if (listBox1.Items.Count > 0) listBox1.Items.Clear();

                    for(int i = 0; i < fi.Length; i++)
                    {
                        if(fi[i].Extension == ".igz")
                        {
                            try
                            {
                                byte[] content = File.ReadAllBytes(fi[i].FullName);

                                if (Encoding.ASCII.GetString(content).Contains("FSB5") && Encoding.ASCII.GetString(content).IndexOf("\x01ZGI") == 0)
                                {
                                    byte[] tmp = new byte[4];

                                    Array.Copy(content, 40, tmp, 0, tmp.Length);
                                    int offset = BitConverter.ToInt32(tmp, 0);
                                    tmp = new byte[4];
                                    Array.Copy(content, 44, tmp, 0, tmp.Length);
                                    int size = BitConverter.ToInt32(tmp, 0);

                                    byte[] fsb_tmp = new byte[size];
                                    //tmp = new byte[size];
                                    Array.Copy(content, offset, fsb_tmp, 0, fsb_tmp.Length);
                                    tmp = new byte[4];
                                    Array.Copy(fsb_tmp, 64, tmp, 0, tmp.Length);
                                    int fsb_sz = BitConverter.ToInt32(tmp, 0);
                                    

                                    content = new byte[fsb_sz - 128];
                                    Array.Copy(fsb_tmp, 208, content, 0, content.Length);

                                    string new_path_name = fi[i].FullName.Replace(".igz", ".mp2");

                                    if (File.Exists(new_path_name)) File.Delete(new_path_name);

                                    File.WriteAllBytes(new_path_name, content);

                                    listBox1.Items.Add("File " + fi[i].Name + " successfully extracted!");
                                }
                                content = null;
                            }
                            catch(Exception ex)
                            {
                                string err_str = ex.Message + "\r\n";

                                File.WriteAllText(Application.StartupPath + "error.log", err_str);
                                listBox1.Items.Add("Error's report file in " + Application.StartupPath + "error.log");
                            }
                        }
                        else if(fi[i].Extension == ".snd")
                        {
                            try
                            {
                                FileStream fs = new FileStream(fi[i].FullName, FileMode.Open);
                                BinaryReader br = new BinaryReader(fs);
                                byte[] header = br.ReadBytes(4);
                                if (Encoding.ASCII.GetString(header) == "FSB5")
                                {
                                    br.BaseStream.Seek(20, SeekOrigin.Begin);
                                    int size_mp2 = br.ReadInt32();
                                    br.BaseStream.Seek(128, SeekOrigin.Begin);

                                    byte[] content = br.ReadBytes(size_mp2);

                                    string new_file_path = fi[i].FullName.Replace(".snd", ".mp2");

                                    if (File.Exists(new_file_path)) File.Delete(new_file_path);

                                    File.WriteAllBytes(new_file_path, content);

                                    content = null;
                                    header = null;

                                    listBox1.Items.Add("File " + fi[i].Name + " successfully extracted!");
                                }
                                else listBox1.Items.Add("Unknown file's format " + fi[i].Name);

                                br.Close();
                                fs.Close();
                            }
                            catch(Exception ex)
                            {
                                string err_str = ex.Message + "\r\n";

                                File.WriteAllText(Application.StartupPath + "error.log", err_str);
                                listBox1.Items.Add("Error's report in file " + Application.StartupPath + "error.log");
                            }
                        }
                    }
                }
            }

        }

        public string RemoveExtension(string path, string extension)
        {
            if (path.IndexOf(extension) > 0) return path.Remove(path.IndexOf(extension), extension.Length);
            return path;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FileFolderDialog ffd = new FileFolderDialog();

            if(ffd.ShowDialog() == DialogResult.OK)
            {
                DirectoryInfo di = new DirectoryInfo(ffd.SelectedPath);
                FileInfo[] fi = di.GetFiles("*.*", SearchOption.AllDirectories);

                if(fi.Length > 0)
                {
                    for(int i = 0; i < fi.Length; i++)
                    {
                        if(i + 1 < fi.Length)
                        {
                            if ((fi[i].Extension == ".igz" && fi[i + 1].Extension == ".mp2")
                                && (RemoveExtension(fi[i].Name, ".igz") == RemoveExtension(fi[i + 1].Name, ".mp2")))
                            {
                                byte[] mp2_content = File.ReadAllBytes(fi[i + 1].FullName);
                                byte[] igz_Content = File.ReadAllBytes(fi[i].FullName);

                                int offset = -1;
                                int size = -1;

                                byte[] tmp = new byte[4];

                                Array.Copy(igz_Content, 40, tmp, 0, tmp.Length);
                                offset = BitConverter.ToInt32(tmp, 0);

                                tmp = new byte[4];

                                Array.Copy(igz_Content, 44, tmp, 0, tmp.Length);
                                size = BitConverter.ToInt32(tmp, 0);

                                tmp = new byte[size];
                                Array.Copy(igz_Content, offset, tmp, 0, tmp.Length);
                                byte[] fsb_tmp = new byte[4];
                                Array.Copy(tmp, 64, fsb_tmp, 0, fsb_tmp.Length);
                                int fsb_sz = BitConverter.ToInt32(fsb_tmp, 0);

                                if (fsb_sz - 128 == mp2_content.Length)
                                {
                                    Array.Copy(mp2_content, 0, tmp, 208, mp2_content.Length);
                                    Array.Copy(tmp, 0, igz_Content, offset, tmp.Length);

                                    File.WriteAllBytes(fi[i].FullName, igz_Content);

                                    listBox1.Items.Add("File " + fi[i].Name + " successfully modded");
                                }
                                else listBox1.Items.Add("Length of file " + fi[i].Name + " doesn't fit length of file " + fi[i + 1].Name);
                            }
                            else if((fi[i].Extension == ".mp2" && fi[i + 1].Extension == ".snd")
                                && (RemoveExtension(fi[i].Name, ".mp2") == RemoveExtension(fi[i + 1].Name, ".snd")))
                            {
                                byte[] mp2_content = File.ReadAllBytes(fi[i].FullName);
                                byte[] snd_content = File.ReadAllBytes(fi[i + 1].FullName);

                                if(snd_content.Length - 128 == mp2_content.Length) //Temporary solution.
                                {
                                    Array.Copy(mp2_content, 0, snd_content, 128, mp2_content.Length);
                                    if (File.Exists(fi[i + 1].FullName)) File.Delete(fi[i + 1].FullName);
                                    File.WriteAllBytes(fi[i + 1].FullName, snd_content);

                                    mp2_content = null;
                                    snd_content = null;

                                    listBox1.Items.Add("File " + fi[i + 1].Name + " successfully modded.");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
