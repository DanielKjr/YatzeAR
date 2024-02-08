namespace YatzeAR
{
    public class FPSHandler
    {
        private int frameCount;
        private int frameThreshold;

        /// <summary>
        /// A class to manage Frames Per Second to manage CPU load
        /// </summary>
        /// <param name="desiredFPS"></param>
        public FPSHandler(int desiredFPS)
        {
            frameThreshold = (60 / desiredFPS) - 1;
        }

        /// <summary>
        /// Will skip frames to artificially lower FPS
        /// <para>Mainly useful for lower end laptops</para>
        /// </summary>
        /// <returns></returns>
        public bool ShouldFrameBeRendered()
        {
            frameCount++;

            if (frameCount >= frameThreshold)
            {
                frameCount = 0;
                return true;
            }

            return false;
        }
    }
}