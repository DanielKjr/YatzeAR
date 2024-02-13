using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;

namespace YatzeAR
{
    public class CapturedImage
    {
        public Mat Frame { get; set; } = new Mat();
        public bool GrabSuccess { get; set; }
    }
}
