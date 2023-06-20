using OpenCvSharp;

namespace YoloV8Net.Segment
{
    /// <summary>
    /// segment result
    /// </summary>
    public class SegPrediction
    {
        /// <summary>
        /// label
        /// </summary>
        public SegClass? Label { get; init; }

        /// <summary>
        /// Box
        /// </summary>
        public Rect Rectangle { get; init; }

        /// <summary>
        /// score
        /// </summary>
        public float Score { get; set; }

        /// <summary>
        /// orginal mask 32
        /// </summary>
        public Mat Mask { get; set; }

        /// <summary>
        /// rgb mask
        /// </summary>
        public Mat RgbMask { get; set; }
    }
}
