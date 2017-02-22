using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace syt_dl {
    public partial class gui : Form {
        public gui() {
            InitializeComponent();
        }
        settings setting = new syt_dl.settings();
        public static string filepath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sytdl";

        //MAKES FORM MOVABLE
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private void gui_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
            if (e.Button == MouseButtons.Right) {
                contextMenuStrip1.Show(System.Windows.Forms.Cursor.Position);
            }
        }

        private void gui_Load(object sender, EventArgs e) {
            //Console.WriteLine(Calls.isMinimized("Syt-dl"));
            MessageBox.Show("github");
            if (Directory.Exists(filepath) == false) {
                this.Hide();
                setting.Hide();
                Program.update();
                this.Show();
            }
        }

        private void pictureBox1_MouseHover(object sender, EventArgs e) {
            this.BackgroundImage = Properties.Resources.highlight;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e) {
            this.BackgroundImage = Properties.Resources.background;
        }

        private void gui_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
            this.BackgroundImage = Properties.Resources.ondrag;
        }

        private void gui_DragLeave(object sender, EventArgs e) {
            this.BackgroundImage = Properties.Resources.background;
        }

        private void gui_DragDrop(object sender, DragEventArgs e) {
            this.Hide();
            setting.Hide();
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string[] args = new string[] { "-f", Program.guiformat};
            if(setting.checkBox2State() == true) {
                args = new string[] { "-f", Program.guiformat, "-i" };
            }
            Console.WriteLine("");
            foreach (string file in files) Calls.writeColor(file, ConsoleColor.Yellow);
            Program.convertVideo(files[0], args);
            this.BackgroundImage = Properties.Resources.background;
            this.Show();
        }

        private void pictureBox1_Click(object sender, EventArgs e) {
            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                this.Hide();
                setting.Hide();
                string[] args = new string[] { "-f", Program.guiformat };
                if (setting.checkBox2State() == true) {
                    args = new string[] { "-f", Program.guiformat, "-i" };
                }
                Console.WriteLine("");
                Calls.writeColor(openFileDialog1.FileName, ConsoleColor.Yellow);
                Program.convertVideo(openFileDialog1.FileName, args);
                this.Show();
            }
        }

        private void gui_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control) {
                this.Hide();
                setting.Hide();
                string[] args = new string[] { "-f", Program.guiformat };
                if (setting.checkBox2State() == true) {
                    args = new string[] { "-f", Program.guiformat, "-i" };
                }
                string url = Clipboard.GetText();
                Console.WriteLine("");
                Calls.writeColor(url, ConsoleColor.Yellow);
                Program.download(url, args);
                this.Show();
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e) {
            setting.ShowDialog();
        }

        private void timer1_Tick(object sender, EventArgs e) {
            if(Calls.isMinimized("Syt-dl") == true) {
                //this.Hide();
                this.WindowState = FormWindowState.Minimized;
            } else {
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Hide();
            setting.Hide();
            string[] args = new string[] { "-f", Program.guiformat };
            if (setting.checkBox2State() == true) {
                args = new string[] { "-f", Program.guiformat, "-i" };
            }
            string url = Clipboard.GetText();
            Console.WriteLine("");
            Calls.writeColor(url, ConsoleColor.Yellow);
            Program.download(url, args);
            this.Show();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
            Environment.Exit(1);
        }
    }
}
