using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PixelTheiw
{
    public partial class Form1 : Form
    {
        private List<Bitmap> _bitmaps = new List<Bitmap>();
        private Random _random = new Random();
        public Form1()
        {
            InitializeComponent();
        }



        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = null;
                _bitmaps.Clear();
                var bitmap = new Bitmap(openFileDialog1.FileName);
                RunProcessing(bitmap);
            }
        }

        private void RunProcessing(Bitmap bitmap)
        {
            trackBar1.Enabled = false;
            Task.Run(() =>
            {
                
                new Thread(() =>
                {
                    pictureBox2.Visible = true;
                    var pixels = GetPixels(bitmap);
                    var pixelStep = (bitmap.Width * bitmap.Height) / 100;
                    var currentPixelsStep = new List<Pixel>(pixels.Count - pixelStep);
                    progressBar1.Value = 0;
                    for (int i = 0; i < trackBar1.Maximum; i++)
                    {
                        if (i == 0)
                        {
                            _bitmaps.Add(new Bitmap(bitmap.Width, bitmap.Height));
                        }
                        else
                        {
                            for (int j = 0; j < pixelStep; j++)
                            {
                                var index = _random.Next(pixels.Count);
                                currentPixelsStep.Add(pixels[index]);
                                pixels.RemoveAt(index);
                            }
                            var currentBitmap = new Bitmap(bitmap.Width, bitmap.Height);

                            foreach (var pixel in currentPixelsStep)
                            {
                                currentBitmap.SetPixel(pixel.Point.X, pixel.Point.Y, pixel.Color);
                            }
                            _bitmaps.Add(currentBitmap);
                        }
                        Text = (i + 1).ToString() + "%";
                        ProgressBarUpdate(i);

                    }
                    _bitmaps.Add(bitmap);
                    trackBar1.Value = 100;
                    pictureBox1.Image = bitmap;
                }).Start();
                
            });
            trackBar1.Enabled = true;
        }
        private void ProgressBarUpdate(int i)
        {
            
            if (progressBar1.Maximum == i)
            {
                progressBar1.Maximum = i + 1;
                progressBar1.Value = i + 1;
                progressBar1.Maximum = i;
            }
            else
            {
                progressBar1.Value = i + 1;
            }
        }

        private List<Pixel> GetPixels(Bitmap bitmap)
        {
            var pixels = new List<Pixel>(bitmap.Width * bitmap.Height);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    pixels.Add(new Pixel
                    {
                        Point = new Point { X = x, Y = y },
                        Color = bitmap.GetPixel(x, y)
                    });

                }
            }
            return pixels;

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

            if (_bitmaps == null || _bitmaps.Count == 0)
            {
                return;
            }
            pictureBox1.Image = _bitmaps[trackBar1.Value];
            Text = trackBar1.Value.ToString() + "%";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.AllowDrop = true;
        }

        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(DataFormats.FileDrop);
            if (data != null)
            {
                var fileNames = data as string[];
                if (fileNames.Length > 0)
                {
                    pictureBox1.Image = null;
                    _bitmaps.Clear();
                    Bitmap bitmap = (Bitmap)Image.FromFile(fileNames[0]);
                    RunProcessing(bitmap);
                }
            }
        }

        private void pictureBox1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }
    }
}
