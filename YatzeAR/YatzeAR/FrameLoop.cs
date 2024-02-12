using Emgu.CV;

namespace YatzeAR
{
    public abstract class FrameLoop
    {
        private bool shouldRun;

        public abstract void OnFrame();

        public void Run()
        {
            shouldRun = true;
            while (shouldRun)
            {
                OnFrame();
                CvInvoke.WaitKey(1);
            }
        }

        public void Stop()
        {
            shouldRun = false;
        }
    }
}