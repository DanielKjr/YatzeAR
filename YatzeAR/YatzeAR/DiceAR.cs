using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace YatzeAR
{
    public class DiceAR : FrameLoop
    {
        private int count = 0;
        private Matrix<float>? distCoeffs;
        private int homo = 0;
        private string? image;
        private Matrix<float>? intrinsics;
        private VideoCapture videoCapture;

        public DiceAR(int camIndex = 1)
        {
#if DEBUG
            string currentDir = Directory.GetCurrentDirectory();
            string[] tmp = Directory.GetFiles(currentDir, "dice4.png");
            image = tmp[0];
#endif

            videoCapture = new VideoCapture(camIndex);

            AR.ReadIntrinsicsFromFile(out intrinsics, out distCoeffs);
        }

        public override void OnFrame()
        {
            if (image != null)
            {
                Mat rawFrame = CvInvoke.Imread(image);
                //bool grabbed = videoCapture.Read(rawFrame);
                //if (!grabbed) return;

                Mat binaryFrame = AR.ConvertToBinaryFrame(rawFrame);
#if DEBUG
                CvInvoke.Imshow("Binary", binaryFrame);
#endif
                VectorOfVectorOfPoint DPCounter = AR.DouglasPeuckerFilter(binaryFrame);

                AR.DrawArea(DPCounter, rawFrame);

                List<Mat> correctedMats = AR.FindHomography(DPCounter, binaryFrame);
                DisplayNextCorrectedImage(correctedMats);

                CountDips(DPCounter, binaryFrame, rawFrame);

                CvInvoke.DrawContours(rawFrame, DPCounter, -1, new MCvScalar(255, 0, 0), 2);
                CvInvoke.Imshow("normal", rawFrame);
            }
        }
        private void CountDips(VectorOfVectorOfPoint DPCounter, Mat binaryFrame, Mat rawFrame)
        {
            for (int i = 0; i < DPCounter.Size; i++)
            {
                Image<Gray, byte> binaryImage = binaryFrame.ToImage<Gray, byte>();
                Rectangle boundingRectangle = CvInvoke.BoundingRectangle(DPCounter[i]);

                // If the contour passes all the filters, it's probably a dice
                int diceWidth = boundingRectangle.Width;
                int diceHeight = boundingRectangle.Height;

                // Create a byte array of the appropriate size
                byte[,] diceArray = new byte[diceWidth, diceHeight];
                // Fill the array with the pixel values from the dice region of the image
                for (int x = 0; x < diceWidth; x++)
                {
                    for (int y = 0; y < diceHeight; y++)
                    {
                        diceArray[x, y] = binaryImage.Data[y + boundingRectangle.Top, x + boundingRectangle.Left, 0];
                    }
                }

                int numberOfPips = Recognize(diceArray);

                // Write the number of pips onto the dice surface in the original image
                Point centerOfDice = new Point(boundingRectangle.X + boundingRectangle.Width / 2, boundingRectangle.Y + boundingRectangle.Height / 2);
                CvInvoke.PutText(rawFrame, numberOfPips.ToString(), centerOfDice, FontFace.HersheySimplex, 1.0, new MCvScalar(0, 0, 255));
            }
        }

        private void DisplayNextCorrectedImage(List<Mat> mats)
        {
            try
            {
                CvInvoke.Imshow("homo", mats[homo]);
                if (count > 30) { homo++; count = 0; }
                if (homo >= mats.Count) homo = 0;
                count++;
            }
            catch { }
        }
        private int Recognize(byte[,] input)
        {
            int xTotal = input.GetLength(0);
            int yTotal = input.GetLength(1);

            int xGrid = xTotal / 3;
            int yGrid = yTotal / 3;

            int xOffset = 2;
            int yOffset = 2;

            int foundDots = 0;
            int searchSize = 2;

            if (SearchArea(input, xOffset, yOffset, searchSize))
            {
                foundDots++;
            }
            if (SearchArea(input, xOffset + xGrid, yOffset, searchSize))
            {
                foundDots++;
            }
            if (SearchArea(input, xOffset + (xGrid*2), yOffset, searchSize))
            {
                foundDots++;
            }


            return foundDots;
        }

        private bool SearchArea(byte[,] input, int searchX, int searchY, int searchArea)
        {
            for (int x = searchArea * -1; x < searchArea; x++)
            {
                for (int y = searchArea * -1; y < searchArea; y++)
                {
                    if (input[searchX + x, searchY + y] == 0)
                    {
                        return true;
                    }
                }
            }


            return false;
        }
    }
}