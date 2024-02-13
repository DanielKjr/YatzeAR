using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace YatzeAR
{
    public class DiceAR
    {
        private byte blobColor = 0;
        private Matrix<float>? distCoeffs;
        private Matrix<float>? intrinsics;

        public DiceAR(bool colorInvertedDice = false)
        {
            if (colorInvertedDice)
            {
                blobColor = 255;
            }

            AR.ReadIntrinsicsFromFile(out intrinsics, out distCoeffs);
        }

        /// <summary>
        /// Main loop for Dice AR detection and logic.
        /// </summary>
        public List<Dice> OnFrame(Mat rawFrame)
        {
            if (rawFrame != null)
            {
                Mat binaryFrame = AR.ConvertToBinaryFrame(rawFrame);

                VectorOfVectorOfPoint contours = AR.DouglasPeuckerFilter(binaryFrame);

                List<Dice> dices = AR.FindHomography(contours, binaryFrame);

                ProcessDice(dices, rawFrame);

                AR.DrawAreaAsText(contours, rawFrame);

                CvInvoke.DrawContours(rawFrame, contours, -1, new MCvScalar(255, 0, 0), 2);
                CvInvoke.Imshow("normal", rawFrame);

                return dices;
            }

            return new List<Dice>();
        }

        /// <summary>
        /// Daniels masterpiece Depth First Search method for counting pips of a dice.
        /// <para>Needs to be tuned for size of dice given that 'pips' will be different</para>
        /// <para>Typical pip size for 50x50 dice is 60 to 90</para>
        /// </summary>
        /// <param name="diceArray"></param>
        /// <param name="blobMinSize"></param>
        /// <param name="blobMaxSize"></param>
        /// <returns></returns>
        private int CountPips(byte[,] diceArray, int blobMinSize = 30, int blobMaxSize = 60)
        {
            int width = diceArray.GetLength(0);
            int height = diceArray.GetLength(1);
            bool[,] visited = new bool[width, height];
            int numberOfPips = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (diceArray[x, y] == blobColor && !visited[x, y]) // If the pixel is black and has not been visited
                    {
                        // Start a depth-first search from this pixel
                        Stack<Point> stack = new Stack<Point>();
                        stack.Push(new Point(x, y));
                        int blobSize = 0;

                        while (stack.Count > 0)
                        {
                            Point p = stack.Pop();
                            if (p.X >= 0 && p.X < width && p.Y >= 0 && p.Y < height) //bounding box
                            {
                                if (diceArray[p.X, p.Y] == blobColor && !visited[p.X, p.Y]) //Is blob color and unvisited
                                {
                                    visited[p.X, p.Y] = true;
                                    blobSize++;
                                    stack.Push(new Point(p.X - 1, p.Y));
                                    stack.Push(new Point(p.X + 1, p.Y));
                                    stack.Push(new Point(p.X, p.Y - 1));
                                    stack.Push(new Point(p.X, p.Y + 1));
                                }
                            }
                        }

                        // Only count blobs that are within a certain size range
                        if (blobSize >= blobMinSize && blobSize <= blobMaxSize)
                        {
                            numberOfPips++;
                        }
                    }
                }
            }

            return numberOfPips;
        }

        /// <summary>
        /// Count number of pips, display it, and add pip count into Dices list for usage in Yatze logic.
        /// </summary>
        /// <param name="Dices"></param>
        /// <param name="rawFrame"></param>
        /// <param name="diceSize"></param>
        private void ProcessDice(List<Dice> Dices, Mat rawFrame, int diceSize = 50)
        {
            foreach (var dice in Dices)
            {
                Mat binaryFrame = AR.ResizeBinaryFrame(dice.Mat, diceSize);

                Image<Gray, byte> binaryImage = binaryFrame.ToImage<Gray, byte>();

                Rectangle boundingRectangle = CvInvoke.BoundingRectangle(dice.Contour);

                // Create a byte array of the appropriate size
                byte[,] diceArray = new byte[diceSize, diceSize];

                // Fill the array with the pixel values from the dice region of the image
                for (int x = 0; x < diceSize; x++)
                {
                    for (int y = 0; y < diceSize; y++)
                    {
                        diceArray[x, y] = binaryImage.Data[y, x, 0];
                    }
                }

                //Count pips using Daniels DFS method. In image size 50x50 the blob size is typically 55-90
                int numberOfPips = CountPips(diceArray, 40, 100);
                dice.Number = numberOfPips;

                AR.DrawPipCountAsText(numberOfPips, boundingRectangle, rawFrame);
            }
        }
    }
}