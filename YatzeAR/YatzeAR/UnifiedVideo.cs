using Emgu.CV;

namespace YatzeAR
{
    public class CapturedImage
    {
        public Mat Frame { get; set; } = new Mat();
        public bool GrabSuccess { get; set; }
    }

    public class UnifiedVideo
    {
        private VideoCapture vCap;

        public UnifiedVideo(int camIndex = 1)
        {
            vCap = new VideoCapture(camIndex);
        }

        public static Mat LatestCapture { get; private set; } = new Mat();

        public CapturedImage Capture()
        {
            Mat rawFrame = new Mat();
            bool grabbed = vCap.Read(rawFrame);

            LatestCapture = rawFrame;

            return new CapturedImage { Frame = rawFrame, GrabSuccess = grabbed };
        }

        public CapturedImage LoadDebugImage(string imageName)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string[] images = Directory.GetFiles(currentDir, imageName);

            Mat debugImage = CvInvoke.Imread(images[0]);

            LatestCapture = debugImage;

            return new CapturedImage { Frame = debugImage, GrabSuccess = true };
        }
    }
}