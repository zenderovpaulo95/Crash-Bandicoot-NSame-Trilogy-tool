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
using System.Threading;

namespace CBNSTT
{
    public partial class TextEditForm : Form
    {
        public TextEditForm()
        {
            InitializeComponent();
        }

        public class FileStruct : IDisposable
        {
            public byte[] head_pad; //Header with padded block
            public byte[] end_block; //last block (it must be 3 blocks)
            public List<byte[]> sub_blocks; //Subblocks in second block

            public FileStruct() { }
            public FileStruct(byte[] _head_pad, byte[] _end_block, List<byte[]> _sub_blocks)
            {
                this.head_pad = _head_pad;
                this.end_block = _end_block;
                this.sub_blocks = _sub_blocks;
            }

            public void Dispose()
            {
                head_pad = null;
                end_block = null;
                sub_blocks.Clear();
            }
        }

        public static int pad_size(int len, int chunk)
        {
            if(len % chunk != 0)
            {
                while (len % chunk != 0) len++;
            }

            return len;
        }

        public static int get_string(byte[] content, int off)
        {
            char ch = '1';
            int len = 0;

            while(ch != '\0')
            {
                ch = (char)content[off];
                off++;
                len++;
            }

            return len;
        }

        public void SendMessage(string message)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new SendMessage(SendMessage), message);
                Thread.Sleep(5);
            }
            else
            {
                listBox1.Items.Add(message);
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
                listBox1.SelectedIndex = -1;
            }
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            FileFolderDialog ffd = new FileFolderDialog();

            if (ffd.ShowDialog() == DialogResult.OK)
            {
                string sel_path = ffd.SelectedPath;
                listBox1.Items.Clear();

                var threadExport = new Threads();
                threadExport.SendMes += SendMessage;

                var processExport = new Thread(new ParameterizedThreadStart(threadExport.threadExtract));
                processExport.Start(sel_path);
            }
        }

        private void ImportBtn_Click(object sender, EventArgs e)
        {
            FileFolderDialog ffd = new FileFolderDialog();

            if (ffd.ShowDialog() == DialogResult.OK)
            {
                listBox1.Items.Clear();

                List<string> parameters = new List<string>();
                parameters.Add(ffd.SelectedPath);
                parameters.Add(Convert.ToString(removeTextCB.Checked));

                var threadImport = new Threads();
                threadImport.SendMes += SendMessage;

                var processImport = new Thread(new ParameterizedThreadStart(threadImport.threadReplace));
                processImport.Start(parameters);
            }
        }
    }
}
