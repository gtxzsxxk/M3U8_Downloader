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
using System.Text.RegularExpressions;
using System.Threading;

namespace M3U8下载
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        string savepath = "", exp_name, url, target_m3u8, domain, URL_PATH;
        HttpDldFile downloader = new HttpDldFile();
        Dictionary<string, string> r2url = new Dictionary<string, string>();//分辨率对应的集合
        Thread downT;

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        List<string> t_list = new List<string>();

        void DownloadT()
        {
            target_m3u8 = r2url[comboBox1.Text];
            Uri downtargeturl = new Uri("http://" + domain + "/" + target_m3u8);
            downloader.Download(downtargeturl.ToString(), savepath + "\\temp.m3u8");
            string tmp = File.ReadAllText(savepath + "\\temp.m3u8");
            URL_PATH = downtargeturl.ToString();
            for (int i = URL_PATH.Length - 1; i >= 0; i--)
            {
                if (URL_PATH[i] == '/')
                {
                    break;
                }
                URL_PATH = URL_PATH.Substring(0, URL_PATH.Length - 1);
            }
            foreach (var i in tmp.Split('\n')) if (i.Length > 0) if (i[0] != '#') t_list.Add(i);
            string savetmp = savepath + "\\temp.t";
            string fsave = savepath + "\\" + exp_name;
            FileStream fs = new FileStream(fsave, FileMode.OpenOrCreate);

            int index = 0;
            foreach (var i in t_list)
            {
                progressBar1.Value = Convert.ToInt32(((double)((double)index / (double)t_list.Count)) * 100);
                downtargeturl = new Uri(URL_PATH + "/" + i);
                downloader.Download(downtargeturl.ToString(), savetmp);
                byte[] vs = File.ReadAllBytes(savetmp);
                long clength = fs.Length;
                fs.Position = clength;
                fs.Write(vs, 0, vs.Length);
                File.Delete(savetmp);
                index++;
            }
            fs.Close();
            MessageBox.Show("下载完毕", "M3U8下载器");
            Thread.CurrentThread.Abort();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            downT = new Thread(DownloadT);
            downT.IsBackground = true;
            downT.Start();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog vs = new FolderBrowserDialog();
            if (vs.ShowDialog() == DialogResult.OK)
            {
                savepath = vs.SelectedPath;
                textBox2.Text = savepath;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            t_list.Clear();
            exp_name = textBox3.Text;
            url = textBox1.Text;
            Uri u = new Uri(url);
            domain = u.Host;
            if (string.IsNullOrWhiteSpace(savepath) || string.IsNullOrWhiteSpace(exp_name))
            {
                MessageBox.Show("信息完整", "M3U8下载器"); return;
            }
            downloader.Download(url, savepath + "\\main.m3u8");
            if (!File.Exists(savepath + "\\main.m3u8"))
            {
                MessageBox.Show("无法下载m3u8文件", "M3U8下载器"); return;
            }
            /*
#EXTM3U
#EXT-X-STREAM-INF:PROGRAM-ID=1, BANDWIDTH=460800, RESOLUTION=480x270
/asp/hls/450/0303000a/3/default/d528201c9276bcef23a58f3be1937d9f/450.m3u8
#EXT-X-STREAM-INF:PROGRAM-ID=1, BANDWIDTH=870400, RESOLUTION=480x270
/asp/hls/850/0303000a/3/default/d528201c9276bcef23a58f3be1937d9f/850.m3u8
#EXT-X-STREAM-INF:PROGRAM-ID=1, BANDWIDTH=1228800, RESOLUTION=1280x720
/asp/hls/1200/0303000a/3/default/d528201c9276bcef23a58f3be1937d9f/1200.m3u8
             */
            string mainm = File.ReadAllText(savepath + "\\main.m3u8");
            for (int i = 0; i < mainm.Split('\n').Length; i++)
            {
                var source = mainm.Split('\n');
                var array = mainm.Split('\n')[i];
                if (array.Contains("RESOLUTION"))
                {
                    int index = 0;
                    for (int j = 0; j < array.Length; j++)
                    {
                        if (array[j] == 'N' && array[j - 1] == 'O')
                        {
                            index = j + 2; break;
                        }
                    }
                    try
                    {
                        r2url.Add(array.Substring(index), mainm.Split('\n')[i + 1]);
                        comboBox1.Items.Add(array.Substring(index));
                    }
                    catch { }
                }
            }
            comboBox1.SelectedItem = comboBox1.Items[comboBox1.Items.Count - 1];
            File.Delete(savepath + "\\main.m3u8");
        }
    }
}
