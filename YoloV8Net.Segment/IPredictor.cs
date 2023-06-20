using OpenCvSharp;

namespace YoloV8Net.Segment
{
    public interface IPredictor
        : IDisposable
    {
        string? InputColumnName { get; }
        string? OutputColumnName { get; }

        int ModelInputHeight { get; }
        int ModelInputWidth { get; }

        int ModelOutputDimensions { get; }

        SegPrediction[] Predict(Mat img);
    }
}
