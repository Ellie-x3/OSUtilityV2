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
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace OSUtility2
{
    public partial class Form1 : Form {

        string path = "C:\\Users\\" + Environment.GetEnvironmentVariable("username") + "\\AppData\\Roaming\\Oneshot";
        string save_path = "C:\\Users\\" + Environment.GetEnvironmentVariable("username") + "\\AppData\\Roaming\\Oneshot\\SaveLocation";
        public string doc_path_file = "C:\\Users\\" + Environment.GetEnvironmentVariable("username") + "\\Documents\\DOCUMENT.oneshot.txt";
        public string doc_path = "C:\\Users\\" + Environment.GetEnvironmentVariable("username") + "\\Documents";

        private Timer t;

        public void InitTimer() {
            t = new Timer();
            t.Tick += new EventHandler(t_Tick);
            t.Interval = 1000;
            t.Start();
        }

        private void t_Tick(object sender, EventArgs e) {
            check_clover();
        }

        public Form1()
        {
            InitializeComponent();
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void Form1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void allRadCheckChanged(Object sender, EventArgs e) {
            check_button();
        }

        private void check_button() {
            foreach (Control c in this.Controls)
            {
                if (c is RadioButton)
                {
                    RadioButton r = c as RadioButton;

                    if (r.Checked == true)
                    {
                        r.ForeColor = System.Drawing.Color.Green;
                    }
                    else { 
                        r.ForeColor = System.Drawing.ColorTranslator.FromHtml("#DC143C");
                    }
                }
            }
        }

        private void check_clover() {
            Process[] clover = Process.GetProcessesByName("_______");

            if (clover.Length == 0)
            {
                picX.Visible = true;
                picCheck.Visible = false;
            }
            else {
                picX.Visible = false;
                picCheck.Visible = true;
            }
        }

        private void kill_niko() {
            string def_save_file = path + "\\save.dat";
            if (File.Exists(def_save_file))
            {
                File.Delete(def_save_file);
                MessageBox.Show("Rip Niko");
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (check_os_path_exists(path) && check_os_saves_path_exists(save_path))
            {
                kill_niko();

                List<string> all_rad = new List<string>();
                Dictionary<string, string> folder_num = new Dictionary<string, string>();
                string checked_button = "";

                foreach (Control c in this.Controls)
                {
                    if (c is RadioButton)
                    {
                        RadioButton r = c as RadioButton;
                        all_rad.Add(r.Name);

                        if (r.Checked == true)
                        {
                            checked_button = r.Name;
                        }
                    }
                }

                if (checked_button == "") {
                    MessageBox.Show("Please select a save file");
                    return;
                }

                all_rad.Reverse();

                for (int i = 0; i <= all_rad.Count - 1; i++)
                {
                    folder_num.Add(all_rad[i], (i + 1).ToString());
                }

                string file_name = "save.dat";
                string path_src = save_path + "\\" + folder_num[checked_button];
                string path_dest = path;

                string src_file = System.IO.Path.Combine(path_src, file_name);
                string dest_file = System.IO.Path.Combine(path_dest, file_name);

                System.IO.File.Copy(src_file, dest_file, true);
            }
            else {
                MessageBox.Show("Could not find required paths. Please try reinstalling OS Utility V2");
            }        
        }

        private void btnNewGame_Click(object sender, EventArgs e)
        {
            if (check_os_path_exists(path)) {
                kill_niko();
            }
        }

        private bool check_os_path_exists(string _path){

            if (Directory.Exists(_path))
            {
                return true;
            }
            else
            {
                MessageBox.Show("You do not have oneshot installed / not installed at default directory!!!");
                return false;
            }
        }

        private bool check_os_saves_path_exists(string _path)
        { 
            if (Directory.Exists(_path))
            {
                return true;
            }
            else
            {
                MessageBox.Show("Saves are lost. Please reinstall OS Utility V2");
                return false;
            }
        }

        private bool check_doc_path_exists(string _path) {
            if (File.Exists(_path))
            {
                return true;
            }
            else
            {
                lblCode.Text = ".....";
                return false;
            }
        }

        private void start_watching() {
            FileSystemWatcher watcher = new FileSystemWatcher();

            watcher.Path = doc_path;
            watcher.Filter = "DOCUMENT.oneshot.txt";
            watcher.Changed += watcher_changed;
            watcher.Created += watcher_changed;
            watcher.EnableRaisingEvents = true;
        }
        private void watcher_changed(object sender, FileSystemEventArgs e) {
            string pattern = @"\d{6}";  
            Regex rg = new Regex(pattern);

            wait_for_file(doc_path_file);

            string code = System.IO.File.ReadAllText(doc_path_file);

            MatchCollection match = rg.Matches(code);

            for (int count = 0; count < match.Count; count++)
                code = match[count].Value;

            this.lblCode.BeginInvoke((MethodInvoker)delegate { this.lblCode.Text = code; });
        }

        private bool is_file_ready(string _file) {
            try
            {
                using (FileStream inputstream = File.Open(_file, FileMode.Open, FileAccess.Read, FileShare.None))
                    return inputstream.Length > 0;
            }
            catch (Exception) {
                return false;
            }
        }

        private void wait_for_file(string _file) {
            while (!is_file_ready(doc_path_file)) { };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitTimer();
            start_watching();
            picX.Visible = true;
            check_doc_path_exists(doc_path_file);
        }
    }
}
