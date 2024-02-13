using Emgu.CV;
using Emgu.CV.Util;
using System.Drawing;

namespace YatzeAR.YatzyLogik
{
    public class User
    {
        public Rectangle Contour { get; set; } = new Rectangle();
        public string Marker { get; set; } = default!;
        public string Name { get; set; } = default!;
        public List<YatzyRule> Rules { get; set; } = new List<YatzyRule>();
        public int Score { get; set; }
        public int TurnOrder { get; set; }

	
	}
}