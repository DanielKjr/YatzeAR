using Emgu.CV;

namespace YatzeAR
{
    public class CameraService
    {
        private int frameTime;
        private VideoCapture vCap;

        public CameraService(int camIndex = 1, int desiredFPS = 60)
        {
            vCap = new VideoCapture(camIndex);

            frameTime = (int)1000 / desiredFPS;
        }

        public static Mat LatestCapture { get; private set; } = new Mat();

        public CapturedImage Capture()
        {
            Mat rawFrame = new Mat();
            bool grabbed = vCap.Read(rawFrame);

            LatestCapture = rawFrame;

            return new CapturedImage { Frame = rawFrame, HasNewFrame = grabbed };
        }

        public void DisplayImage(Mat frame)
        {
            CvInvoke.Imshow("YatzyAR", frame);
            CvInvoke.WaitKey(frameTime);
        }

        public CapturedImage LoadDebugImage(string imageName)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string[] images = Directory.GetFiles(currentDir, imageName);

            Mat debugImage = CvInvoke.Imread(images[0]);

            LatestCapture = debugImage;

            return new CapturedImage { Frame = debugImage, HasNewFrame = true };
        }
    }
}