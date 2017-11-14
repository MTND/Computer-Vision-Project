using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Emgu.CV.Structure;

namespace ComputerVisionSandbox
{
    static class TestClass
    {

        public static Image<Bgr, byte> Calculate(Image<Bgr, byte> img, out Image<Gray, byte> foregroundMask, out Image<Gray, byte> colorMask, out Image<Gray, byte> liquidMask, out Image<Gray, byte> cropedImage)
        {
            foregroundMask = RemoveBackground(img);
            Image<Gray, byte> foregroundMaskCopy = foregroundMask.Copy();
            colorMask = MapLiquidColor(img);
            liquidMask = new Image<Gray, byte>(img.Width, img.Height);

            CvInvoke.BitwiseAnd(foregroundMask, colorMask, liquidMask);
            
            VectorOfVectorOfPoint liquidMaskContours = FindContoursOfMapedImage(liquidMask.Clone());
            VectorOfPoint liquidContour = FindLiquidContour(liquidMaskContours);

            int heightOfLiquid = FindHeightOfLiquid(liquidContour);
            SetRegionOfInterest(foregroundMaskCopy, heightOfLiquid);
            cropedImage = CropImage(foregroundMaskCopy);

            VectorOfVectorOfPoint cropedImageContours = FindContoursOfMapedImage(cropedImage.Clone());
            VectorOfPoint cropedContour = FindLiquidContour(cropedImageContours);

            DrawContour(liquidContour, img);
            DrawContour(cropedContour, img);

            int liquidArea = (int)CvInvoke.ContourArea(liquidContour, false);
            int cropedArea = (int)CvInvoke.ContourArea(cropedContour, false);
            int percentage = (liquidArea * 100) / (liquidArea + cropedArea);
            Console.WriteLine("Contour area: " + percentage);

            liquidArea = CvInvoke.CountNonZero(liquidMask);
            cropedArea = CvInvoke.CountNonZero(cropedImage);
            percentage = (liquidArea * 100) / (liquidArea + cropedArea);
            Console.WriteLine("Pixel area:" + percentage);

            liquidArea = (int)CvInvoke.ContourArea(liquidContour, false);
            cropedArea = CvInvoke.CountNonZero(cropedImage);
            percentage = (liquidArea * 100) / (liquidArea + cropedArea);
            Console.WriteLine("Mixed area:" + percentage);

            return img;
        }

        public static Image<Gray, byte> RemoveBackground(Image<Bgr, byte> img)
        {
            Image<Bgr, byte> newImg = img.Clone();
            Rectangle rect = new Rectangle(1, 1, newImg.Width, newImg.Height);
            Image<Gray, byte> gray = newImg.GrabCut(rect, 3);
            gray = gray.ThresholdBinary(new Gray(2), new Gray(255));
            return gray;
        }

        public static Image<Gray, byte> MapLiquidColor(Image<Bgr, byte> _imgInput)
        {
            using (Image<Hsv, Byte> hsv = _imgInput.Convert<Hsv, Byte>())
            {
                CvInvoke.GaussianBlur(hsv, hsv, new Size(13, 13), 0, 0);
                Image<Gray, Byte>[] channels = hsv.Split();

                try
                {
                    ScalarArray SA1 = new ScalarArray(new MCvScalar(30));
                    ScalarArray SA2 = new ScalarArray(new MCvScalar(220));
                    CvInvoke.InRange(channels[0], SA1, SA2, channels[0]);
                    channels[0]._Not();

                    channels[1]._ThresholdBinary(new Gray(110), new Gray(255.0));

                    CvInvoke.BitwiseAnd(channels[0], channels[1], channels[0]);
                    CvInvoke.Erode(channels[0], channels[0], CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(5, 5), new Point(-1, -1)), new Point(-1, -1), 3, BorderType.Reflect, default(MCvScalar));

                }
                finally
                {
                    channels[1].Dispose();
                    channels[2].Dispose();
                }

                return channels[0];
            }
        }

        public static VectorOfVectorOfPoint FindContoursOfMapedImage(Image<Gray, byte> img)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

            CvInvoke.FindContours(img, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            return contours;
        }

        public static VectorOfPoint FindLiquidContour(VectorOfVectorOfPoint allContours)
        {
            VectorOfPoint contour = new VectorOfPoint();
            VectorOfPoint liquidContour = new VectorOfPoint();
            double maxArea = 0;
            for (int i = 0; i < allContours.Size; i++)
            {
                CvInvoke.ApproxPolyDP(allContours[i], contour, CvInvoke.ArcLength(allContours[i], true) * 0.01, true);

                if (CvInvoke.ContourArea(contour, false) > maxArea)
                {
                    maxArea = CvInvoke.ContourArea(contour, false);
                    liquidContour = contour;
                }
            }

            return liquidContour;
        }

        public static int FindHeightOfLiquid(VectorOfPoint liquidContour)
        {
            int minY = int.MaxValue;
            Point[] points = liquidContour.ToArray();

            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].Y < minY)
                {
                    minY = points[i].Y;
                }
            }

            return minY;
        }

        public static void SetRegionOfInterest(Image<Gray, byte> img, int height)
        {
            Rectangle roi = new Rectangle(0, 0, img.Width, height);
            img.ROI = roi;
        }

        private static Image<Gray, byte> CropImage(Image<Gray, byte> img)
        {
            Image<Gray, byte> cropedImage = img.Clone();
            return cropedImage;
        }

        public static void DrawContour(VectorOfPoint contour, Image<Bgr, byte> img)
        {
            Point[] points;
            points = contour.ToArray();
            img.DrawPolyline(points, true, new Bgr(Color.Aqua), 1);
        }

    }
}
