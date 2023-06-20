using BataAiAssist;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using System.Collections.Concurrent;

namespace YoloV8Net.Segment
{
    public class YoloV8Segment
        : PredictorBase, IPredictor
    {
        public YoloV8Segment(string modelPath, string[]? labels = null, bool useCuda = false) : base(modelPath, labels, useCuda)
        {
        }

        public static IPredictor Create(string modelPath, string[]? labels = null, bool useCuda = false)
        {
            return new YoloV8Segment(modelPath, labels, useCuda);
        }

        protected SegPrediction[] ParseOutput(DenseTensor<float> dect, DenseTensor<float> prop, Mat image)
        {
            var result = new ConcurrentBag<SegPrediction>();

            var (w, h) = (image.Width, image.Height); // image w and h
            var (xGain, yGain) = (ModelInputWidth / (float)w, ModelInputHeight / (float)h); // x, y gains
            var gain = Math.Min(xGain, yGain); // gain = resized / original

            var (xPad, yPad) = ((ModelInputWidth - w * gain) / 2, (ModelInputHeight - h * gain) / 2); // left, right pads

            Parallel.For(0, dect.Dimensions[0], i =>
            {
                //divide total length by the elements per prediction
                //int divj = (int)(dect.Length / dect.Dimensions[1]);
                Parallel.For(0, (int)(dect.Length / dect.Dimensions[1]), j =>
                {
                    float xMin = ((dect[i, 0, j] - dect[i, 2, j] / 2) - xPad) / gain; // unpad bbox tlx to original
                    float yMin = ((dect[i, 1, j] - dect[i, 3, j] / 2) - yPad) / gain; // unpad bbox tly to original
                    float xMax = ((dect[i, 0, j] + dect[i, 2, j] / 2) - xPad) / gain; // unpad bbox brx to original
                    float yMax = ((dect[i, 1, j] + dect[i, 3, j] / 2) - yPad) / gain; // unpad bbox bry to original

                    xMin = Utils.Clamp(xMin, 0, w - 0); // clip bbox tlx to boundaries
                    yMin = Utils.Clamp(yMin, 0, h - 0); // clip bbox tly to boundaries
                    xMax = Utils.Clamp(xMax, 0, w - 1); // clip bbox brx to boundaries
                    yMax = Utils.Clamp(yMax, 0, h - 1); // clip bbox bry to boundaries
                    Mat mk = new Mat(1, 32, MatType.CV_32F);
                    //mask
                    for (int m = 0; m < dect.Dimensions[1] - 4 - Classes.Length; m++)
                    {
                        mk.At<float>(0, m) = dect[i, 4 + Classes.Length + m, j];
                    }
                    //Loop Num Class
                    Parallel.For(0, Classes.Length, nc =>
                    {
                        var pred = dect[i, 4 + nc, j];

                        //skip low confidence values
                        if (pred < Confidence) return;
                        result.Add(new SegPrediction()
                        {
                            Label = Classes[nc],
                            Score = pred,
                            Rectangle = new Rect() { X = (int)xMin, Y = (int)yMin, Width = (int)(xMax - xMin), Height = (int)(yMax - yMin) },
                            Mask = mk
                        });
                    });
                });
            });
            //NMS
            SegPrediction[] nms_results = Suppress(result.ToList());
            Mat proto_data = new Mat(32, 25600, MatType.CV_32F, prop.ToArray());
            for (int r = 0; r < nms_results.Length; r++)
            {
                Mat rgb_mask = Mat.Zeros(new OpenCvSharp.Size(image.Width, image.Height), MatType.CV_8UC3);
                Random rd = new Random();
                Rect box = nms_results[r].Rectangle;
                int box_x1 = box.X;
                int box_y1 = box.Y;
                int box_x2 = box.BottomRight.X;
                int box_y2 = box.BottomRight.Y;
                // Segment Result
                Mat original_mask = nms_results[r].Mask * proto_data;
                //Mat original_mask = result.ToList()[10].Mask * proto_data;
                Parallel.For(0, original_mask.Cols, col =>
                {
                    original_mask.At<float>(0, col) = Utils.Sigmoid(original_mask.At<float>(0, col));
                });
                // 1x25600 -> 160x160
                Mat reshape_mask = original_mask.Reshape(1, 160);
                // scale
                float scale = Math.Max(w, h) / 640f;
                int mx1 = Math.Max(0, (int)((box_x1 + xPad) / scale * 0.25));
                int mx2 = Math.Max(0, (int)((box_x2 + xPad) / scale * 0.25));
                int my1 = Math.Max(0, (int)((box_y1 + yPad) / scale * 0.25));
                int my2 = Math.Max(0, (int)((box_y2 + yPad) / scale * 0.25));
                // get roi
                Mat mask_roi = new Mat(reshape_mask, new OpenCvSharp.Range(my1, my2), new OpenCvSharp.Range(mx1, mx2));
                Mat actual_maskm = new Mat();
                Cv2.Resize(mask_roi, actual_maskm, new OpenCvSharp.Size((int)nms_results[r].Rectangle.Width,
                    (int)nms_results[r].Rectangle.Height));
                Cv2.Threshold(actual_maskm, actual_maskm, 0.5, 255, ThresholdTypes.Binary);
                // predict
                Mat bin_mask = new Mat();
                //actual_maskm.ImWrite("actual_maskm.png");
                actual_maskm.ConvertTo(bin_mask, MatType.CV_8UC1);
                if ((nms_results[r].Rectangle.Y + bin_mask.Rows) >= image.Height)
                {
                    box_y2 = (int)image.Height - 1;
                }
                if ((box_x1 + bin_mask.Cols) >= image.Width)
                {
                    box_x2 = (int)image.Width - 1;//image.Width = 3
                }
                // get segment region
                Mat mask = Mat.Zeros(new OpenCvSharp.Size((int)image.Width, (int)image.Height), MatType.CV_8UC1);
                bin_mask = new Mat(bin_mask, new OpenCvSharp.Range(0, box_y2 - box_y1), new OpenCvSharp.Range(0, box_x2 - box_x1));
                Rect roi = new Rect(box_x1, box_y1, box_x2 - box_x1, box_y2 - box_y1);
                bin_mask.CopyTo(new Mat(mask, roi));
                //segment region colorful
                Cv2.Add(rgb_mask, new Scalar(rd.Next(0, 255), rd.Next(0, 255), rd.Next(0, 255)), rgb_mask, mask);
                nms_results[r].RgbMask = rgb_mask;
            }
            return nms_results;
        }

        public override SegPrediction[] Predict(Mat image)
        {
            return ParseOutput(
                    Inference(image)[0], Inference(image)[1], image);
        }
    }
}
