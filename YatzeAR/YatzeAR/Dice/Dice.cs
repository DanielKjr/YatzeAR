using Emgu.CV;
using Emgu.CV.Util;

namespace YatzeAR
{
    public class Dice
    {
        public VectorOfPoint Contour { get; set; } = new VectorOfPoint();
        public Mat Mat { get; set; } = new Mat();
        public int Number { get; set; }
    }
}