using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ComputerVisionSandbox
{
    public partial class Form1 : Form
    {
        private Image<Bgr, Byte> _imgSource;

        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
            using (openFileDialog1)
            {
                openFileDialog1.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    _imgSource = new Image<Bgr, Byte>((Bitmap)Image.FromFile(openFileDialog1.FileName));//.Resize(pictureBox1.Width, pictureBox1.Height, Inter.Linear);

                    Image<Gray, byte> foregroundMask;
                    Image<Gray, byte> colorMask;
                    Image<Gray, byte> liquidMask;
                    Image<Gray, byte> cropedImage;


                    Image<Bgr, byte> foreground = TestClass.Calculate(_imgSource, out foregroundMask, out colorMask, out liquidMask, out cropedImage);


                    pictureBox1.Image = _imgSource.Bitmap;
                    pictureBox2.Image = foregroundMask.Bitmap;
                    pictureBox3.Image = colorMask.Bitmap;
                    pictureBox4.Image = liquidMask.Bitmap;
                    pictureBox5.Image = cropedImage.Bitmap;
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
