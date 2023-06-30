using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace BataAiAssist
{
    /// <summary>
    /// deal with tensor
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// 将Yolo格式转换成PP
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float[] Xywh2xyxy(float[] source)
        {
            var result = new float[4];

            result[0] = source[0] - source[2] / 2f;
            result[1] = source[1] - source[3] / 2f;
            result[2] = source[0] + source[2] / 2f;
            result[3] = source[1] + source[3] / 2f;

            return result;
        }

        /// <summary>
        /// Zoom Resize
        /// </summary>
        /// <param name="src"></param>
        /// <param name="wr"></param>
        /// <param name="hr"></param>
        /// <param name="isZoomIn"></param>
        /// <returns></returns>
        public static Mat ResizeImgCenter(Mat src, int wr, int hr, bool isZoomIn = false)
        {
            Mat dst_img = new Mat();
            int iw = src.Size(1);
            int ih = src.Size(0);
            Trace.WriteLine($"iw = {iw}, ih = {ih}, hr = {hr}");
            float scale = Math.Min(wr / (float)iw, hr / (float)ih);
            int nw = (int)(scale * iw);
            int nh = (int)(scale * ih);
            Cv2.Resize(src, dst_img, new OpenCvSharp.Size(nw, nh), 0, 0, InterpolationFlags.Linear);
            Mat bg;
            if (isZoomIn)
            {
                bg = new Mat(new OpenCvSharp.Size(wr, hr), MatType.CV_8UC3, new Scalar(0, 0, 0));
            }
            else
            {
                bg = new Mat(new OpenCvSharp.Size(wr, hr), MatType.CV_8UC3, new Scalar(128, 128, 128));
            }
            Mat mask = new Mat();
            //Cv2.CvtColor(dst_img, mask, ColorConversionCodes.BGR2GRAY);

            Mat roi = new Mat(bg, new Rect((int)(wr - nw) / 2, (int)(hr - nh) / 2, nw, nh));
            dst_img.CopyTo(roi, mask);
            Cv2.CvtColor(bg, bg, ColorConversionCodes.BGR2RGB);
            return bg;
        }

        /// <summary>
        /// Set Orig Image to TL
        /// </summary>
        /// <param name="src"></param>
        /// <param name="wr"></param>
        /// <param name="hr"></param>
        /// <returns></returns>
        public static Mat ResizeImgTL(Mat src, int wr, int hr)
        {
            int max_axis = src.Cols > src.Rows ? src.Cols : src.Rows;
            Mat dst_img = Mat.Zeros(new OpenCvSharp.Size(max_axis, max_axis), MatType.CV_8UC3);
            Rect roi = new Rect(0, 0, src.Cols, src.Rows);
            src.CopyTo(new Mat(dst_img, roi));
            Cv2.CvtColor(dst_img, dst_img, ColorConversionCodes.BGR2RGB);
            Mat resize_image = new Mat();
            Cv2.Resize(dst_img, resize_image, new OpenCvSharp.Size(wr, hr));
            return resize_image;
        }

        /// <summary>
        /// OpencvSharp.Mat转Tensor
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Tensor<float> Mat2NormTensor(Mat input)
        {
            Tensor<float> output = new DenseTensor<float>(new[] { 1, 3, input.Height, input.Width });
            unsafe
            {
                //for (int r = 0; r < input.Rows; r++)
                //{
                //    IntPtr dst_ptrColor = input.Ptr(r);
                //    byte* dst_dataColor = (byte*)dst_ptrColor.ToPointer();
                //    for (int c = 0; c < input.Cols; c++)
                //    {
                //        output[0, 0, r, c] = (float)(dst_dataColor[c * 3 + 0] / 255.0);
                //        output[0, 1, r, c] = (float)(dst_dataColor[c * 3 + 1] / 255.0);
                //        output[0, 2, r, c] = (float)(dst_dataColor[c * 3 + 2] / 255.0);
                //    }
                //}
                Parallel.For(0, input.Height, (r) =>
                {
                    IntPtr dst_ptrColor = input.Ptr(r);
                    byte* dst_dataColor = (byte*)dst_ptrColor.ToPointer();
                    Parallel.For(0, input.Width, (c) =>
                    {
                        output[0, 0, r, c] = (float)(dst_dataColor[c * 3 + 0] / 255.0); // r
                        output[0, 1, r, c] = (float)(dst_dataColor[c * 3 + 1] / 255.0); // g
                        output[0, 2, r, c] = (float)(dst_dataColor[c * 3 + 2] / 255.0); // b
                    });
                });
            }
            return output;
        }

        /// <summary>
        /// Bitmap转Tensor
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Tensor<float> ExtractPixels(Mat image)
        {
            var bitmap = BitmapConverter.ToBitmap(image);

            var rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;

            var tensor = new DenseTensor<float>(new[] { 1, 3, bitmap.Height, bitmap.Width });

            unsafe // speed up conversion by direct work with memory
            {
                Parallel.For(0, bitmapData.Height, (y) =>
                {
                    byte* row = (byte*)bitmapData.Scan0 + (y * bitmapData.Stride);

                    Parallel.For(0, bitmapData.Width, (x) =>
                    {
                        tensor[0, 0, y, x] = row[x * bytesPerPixel + 0] / 255.0F; // r
                        tensor[0, 1, y, x] = row[x * bytesPerPixel + 1] / 255.0F; // g
                        tensor[0, 2, y, x] = row[x * bytesPerPixel + 2] / 255.0F; // b
                    });
                });

                bitmap.UnlockBits(bitmapData);
            }

            return tensor;
        }

        /// <summary>
        /// edge crop
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        /// <summary>
        ///sigmoid func
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float Sigmoid(float value)
        {
            return 1 / (1 + (float)Math.Exp(-value));
        }
    }
}
