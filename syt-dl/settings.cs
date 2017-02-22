using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace syt_dl {
    public partial class settings : Form {
        public settings() {
            InitializeComponent();
        }

        private void settings_Load(object sender, EventArgs e) {
            
        }

        private void button1_Click(object sender, EventArgs e) {
            Console.WriteLine("");
            Calls.writeColor("Updating via gui", ConsoleColor.Yellow);
            this.Hide();
            Program.gui.Hide();
            Program.update();
            Program.gui.Show();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            Program.guiformatindex = comboBox1.SelectedIndex;
            Program.guiformat = comboBox1.SelectedItem.ToString();
        }

        private void settings_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true;
            this.Hide();
        }

        private void settings_VisibleChanged(object sender, EventArgs e) {
            if(this.Visible == true) {
                comboBox1.SelectedIndex = Program.guiformatindex;
                string youtubedl = Program.sendCommandOutput("youtube-dl", "--version", false);
                string ffmpegdata = Program.sendCommandOutput("ffmpeg", "-version", false);
                string[] ffmpegarray = ffmpegdata.Split(new string[] { "Copyright" }, StringSplitOptions.None);
                string ffmpeg = ffmpegarray[0].Replace("ffmpeg", "").Replace("version", "");
                label3.Text = "V. " + youtubedl;
                label5.Text = "V. " + ffmpeg;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            Program.gui.TopMost = checkBox1.Checked;
        }

        public bool checkBox2State() {
            return checkBox2.Checked;
        }
    }
}
