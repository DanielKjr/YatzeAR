using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace YatzeAR
{
    public class Dice
    {
       
        public int Number { get; set; }
        public Mat Mat { get; set; } = new Mat();
        public VectorOfPoint Contour { get; set; } = new VectorOfPoint();
    }
}