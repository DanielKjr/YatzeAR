using Emgu.CV.Util;

namespace YatzeAR.YatzyLogik
{
    public class User
    {
        public VectorOfPoint Contour { get; set; } = new VectorOfPoint();
        public string Marker { get; set; } = default!;
        public string Name { get; set; } = default!;
        public List<YatzyRule> Rules { get; set; } = new List<YatzyRule>();
        public int Score { get; set; }
        public int TurnOrder { get; set; }
    }
}