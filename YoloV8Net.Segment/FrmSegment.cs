using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Diagnostics;

namespace YoloV8Net.Segment
{
    public partial class FrmSegment : Form
    {
        private IPredictor yoloSegment;
        private Stopwatch watchClock = new Stopwatch();

        public FrmSegment()
        {
            InitializeComponent();
        }

        private void FrmSegment_Load(object sender, EventArgs e)
        {
            yoloSegment = YoloV8Segment.Create("model/yolov8n-seg.onnx", null, true);
        }

        private void btnOpenImg_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofg = new OpenFileDialog();
            if (ofg.ShowDialog() == DialogResult.OK)
            {
                watchClock.Reset();

                Mat image = new Mat(ofg.FileName);
                watchClock.Start();
                var predictions = yoloSegment.Predict(image);
                watchClock.Stop();
                lblResult.Text = $"Num of Predictions ：{predictions.Length}，Elapsed Time ：{watchClock.ElapsedMilliseconds} ms";
                if (predictions.Length > 0)
                {
                    Mat showImg = DrawResult(predictions, image);
                    pictureBox1.Image = BitmapConverter.ToBitmap(showImg);
                }
                else
                {
                    pictureBox1.Image = BitmapConverter.ToBitmap(image);
                }
            }
        }

        private static Mat DrawResult(SegPrediction[] result, Mat image)
        {
            var originalImageHeight = image.Height;
            var originalImageWidth = image.Width;

            Mat masked_img = new Mat();
            // 将识别结果绘制到图片上
            for (int i = 0; i < result.Count(); i++)
            {
                var x = Math.Max(result[i].Rectangle.X, 0);
                var y = Math.Max(result[i].Rectangle.Y, 0);
                var width = Math.Min(originalImageWidth - x, result[i].Rectangle.Width);
                var height = Math.Min(originalImageHeight - y, result[i].Rectangle.Height);
                Cv2.Rectangle(image,
                    new OpenCvSharp.Point(x, y),
                    new OpenCvSharp.Point(x + width, y + height),
                    new Scalar(0, 0, 255), 2, LineTypes.Link8);
                Cv2.Rectangle(image, new OpenCvSharp.Point(x, y - 20),
                    new OpenCvSharp.Point(result[i].Rectangle.BottomRight.X, result[i].Rectangle.TopLeft.Y), new Scalar(0, 255, 255), -1);
                Cv2.PutText(image, result[i].Label.Name + "-" + result[i].Score.ToString("0.00"),
                    new OpenCvSharp.Point((int)result[i].Rectangle.X, (int)result[i].Rectangle.Y - 10),
                    HersheyFonts.HersheySimplex, 0.6, new Scalar(0, 0, 0), 1);
                Cv2.AddWeighted(image, 0.5, result[i].RgbMask, 0.5, 0, masked_img);
            }
            return masked_img;
        }
    }
}