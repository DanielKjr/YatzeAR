using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using YatzeAR.DTO;

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
        public ProcessedDice OnFrame(Mat rawFrame, Mat drawFrame)
        {
            if (drawFrame == null)
            {
                drawFrame = new Mat();
                rawFrame.CopyTo(drawFrame);
            }

            if (rawFrame != null)
            {
                Mat binaryFrame = AR.ConvertToBinaryFrame(rawFrame);

                VectorOfVectorOfPoint contours = AR.DouglasPeuckerFilter(binaryFrame);
                VectorOfVectorOfPoint drawableContours = new VectorOfVectorOfPoint();

                List<Dice> dices = AR.FindHomography(contours, binaryFrame);

                foreach (var dice in dices)
                {
                    Mat resizedBinaryFrame = AR.ResizeBinaryFrame(dice.Mat, 50);

                    byte[,] diceArray = AR.FrameToByteArray(resizedBinaryFrame);

                    int contourArea = (int)CvInvoke.ContourArea(dice.Contour);

                    if (contourArea < 1500)
                    {
                        dice.Number = CountPips(diceArray, 40, 100);

                        if (dice.Number > 0)
                        {
                            AR.DrawPipCountAsText(dice.Number, dice.Contour, drawFrame);

                            AR.DrawAreaAsText(contourArea, dice, drawFrame);

                            drawableContours.Push(dice.Contour);
                        }
                    }
                }

                CvInvoke.DrawContours(drawFrame, drawableContours, -1, new MCvScalar(255, 0, 0), 2);

                return new ProcessedDice { Dices = dices, DrawnFrame = drawFrame };
            }

            return new ProcessedDice();
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
    }
}