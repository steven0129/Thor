using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thor
{
    public partial class Form1 : Form
    {
        PictureBox[] picboxArr = new PictureBox[256];
        int counter = 0;
        string[] info_csv;
        string out_info = "";
        bool[] noiseArr = new bool[256];

        public Form1()
        {
            InitializeComponent();
        }

        private void 載入圖片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "載入圖片";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                textBoxIn.Text = fileName;
                string[] fileNameArr = fileName.Split(new char[] { '\\' });
                string currName = fileNameArr[fileNameArr.Length - 1];

                Bitmap img = new Bitmap(fileName);
                int blockWidth = 240;
                int blockHeight = 135;

                for (int idxY = 0; idxY < 16; idxY++)
                {
                    for (int idxX = 0; idxX < 16; idxX++)
                    {
                        Bitmap block = new Bitmap(blockWidth, blockHeight);
                        Graphics graphic = Graphics.FromImage(block);
                        graphic.DrawImage(img, new Rectangle(0, 0, blockWidth, blockHeight), new Rectangle(blockWidth * idxX, blockHeight * idxY, blockWidth, blockHeight), GraphicsUnit.Pixel);

                        picboxArr[idxY * 16 + idxX].Image = block;
                    }
                }
            }
        }

        private void btnOut_Click(object sender, EventArgs e)
        {
            openFileDialog2.Title = "匯出Info檔";
            openFileDialog2.Filter = "csv file (*.csv)|*.csv";
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                out_info = openFileDialog2.FileName;
                textBoxOut.Text = out_info;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < picboxArr.Length; i++)
            {
                picboxArr[i] = new PictureBox();
                picboxArr[i].Size = new Size(30, 15);
                picboxArr[i].Anchor = (AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom);
                picboxArr[i].SizeMode = PictureBoxSizeMode.Zoom;
                picboxArr[i].Click += new EventHandler(this.picbox_handler);
                displayTable.Controls.Add(picboxArr[i]);
            }
        }

        private void picbox_handler(object sender, EventArgs e)
        {
            PictureBox picbox = (PictureBox)sender;
            Graphics g = picbox.CreateGraphics();
            g.DrawRectangle(new Pen(Color.Red, 4), 0, 0, picbox.Width, picbox.Height);
            noiseArr[Array.IndexOf(picboxArr, sender)] = true;
        }

        private void btnIn_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "載入Info檔";
            openFileDialog1.Filter = "csv file (*.csv)|*.csv";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                counter = int.Parse(File.ReadAllText(Path.GetDirectoryName(fileName) + "/temp.txt"));
                textBoxIn.Text = fileName;

                info_csv = File.ReadAllLines(fileName).Skip(1).ToArray();
                string[] info = info_csv[counter].Split(new char[] { ',' });
                this.load_img(Path.GetDirectoryName(fileName) + "/" + info[0]);

                labelFileName.Text = info[0];
                labelLabel.Text = info[1];
                labelOriginName.Text = info[2];
            }
        }

        private void load_img(string fileName)
        {
            fileName = fileName.Replace('/', '\\');
            string[] fileNameSplit = fileName.Split('\\');
            Bitmap img = new Bitmap(fileName);
            int blockWidth = 240;
            int blockHeight = 135;

            for (int idxY = 0; idxY < 16; idxY++)
            {
                for (int idxX = 0; idxX < 16; idxX++)
                {
                    Bitmap block = new Bitmap(blockWidth, blockHeight);
                    Graphics graphic = Graphics.FromImage(block);
                    graphic.DrawImage(img, new Rectangle(0, 0, blockWidth, blockHeight), new Rectangle(blockWidth * idxX, blockHeight * idxY, blockWidth, blockHeight), GraphicsUnit.Pixel);
                    picboxArr[idxY * 16 + idxX].Image = block;
                }
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            btnSave.Enabled = true;
            string fileName = openFileDialog1.FileName;
            if (counter != 0)
            {
                counter--;
                string[] info = info_csv[counter].Split(new char[] { ',' });
                this.load_img(Path.GetDirectoryName(fileName) + "/" + info[0]);

                labelFileName.Text = info[0];
                labelLabel.Text = info[1];
                labelOriginName.Text = info[2];

                for (int i = 0; i < noiseArr.Length; i++) noiseArr[i] = false;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            btnSave.Enabled = true;
            string fileName = openFileDialog1.FileName;
            if (counter != info_csv.Length)
            {
                counter++;
                string[] info = info_csv[counter].Split(new char[] { ',' });
                this.load_img(Path.GetDirectoryName(fileName) + "/" + info[0]);

                labelFileName.Text = info[0];
                labelLabel.Text = info[1];
                labelOriginName.Text = info[2];

                for (int i = 0; i < noiseArr.Length; i++) noiseArr[i] = false;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string[] info = info_csv[counter].Split(new char[] { ',' });
            string out_dir = Path.GetDirectoryName(out_info);
            for (int i = 0; i < picboxArr.Length; i++)
            {
                PictureBox picbox = picboxArr[i];

                string out_name = $"{info[0].Substring(0, info[0].Length - 4)}_{i}.bmp";
                picbox.Image.Save($"{out_dir}/{out_name}", ImageFormat.Bmp);
                if (noiseArr[i] == true)
                    File.AppendAllText(openFileDialog2.FileName, $"\n{out_name},Noise,{info[0]},{info[2]}");
                else
                    File.AppendAllText(openFileDialog2.FileName, $"\n{out_name},{info[1]},{info[0]},{info[2]}");
            }

            btnSave.Enabled = false;
            File.WriteAllText(Path.GetDirectoryName(openFileDialog1.FileName) + "/temp.txt", counter.ToString());
        }
    }
}
