using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace YatzeAR
{
    public class DiceAR : FrameLoop
    {
        private Matrix<float>? distCoeffs;
        private string? image;
        private Matrix<float>? intrinsics;
        private VideoCapture videoCapture;

        public DiceAR(int camIndex = 1)
        {
#if DEBUG
            string currentDir = Directory.GetCurrentDirectory();
            string[] tmp = Directory.GetFiles(currentDir, "dice5.png");
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

                VectorOfVectorOfPoint DPCounter = AR.DouglasPeuckerFilter(binaryFrame);

                AR.DrawArea(DPCounter, rawFrame);

                List<CorrectedDice> correctedMats = AR.FindHomography(DPCounter, binaryFrame);

                CountDips(correctedMats, rawFrame);

                CvInvoke.DrawContours(rawFrame, DPCounter, -1, new MCvScalar(255, 0, 0), 2);
                CvInvoke.Imshow("normal", rawFrame);
            }
        }
        private void CountDips(List<CorrectedDice> mats, Mat rawFrame)
        {
            int hi = 0;
            foreach (var mat in mats)
            {
                hi++;
                Size calculationSize = new Size(50, 50);

                Mat binaryFrame = AR.ResizeBinaryFrame(mat.Mat, calculationSize);

                Image<Gray, byte> binaryImage = binaryFrame.ToImage<Gray, byte>();

                Rectangle boundingRectangle = CvInvoke.BoundingRectangle(mat.Contour);

                // Create a byte array of the appropriate size
                byte[,] diceArray = new byte[calculationSize.Width, calculationSize.Height];

                // Fill the array with the pixel values from the dice region of the image
                for (int x = 0; x < calculationSize.Width; x++)
                {
                    for (int y = 0; y < calculationSize.Height; y++)
                    {
                        diceArray[x, y] = binaryImage.Data[y, x, 0];
                    }
                }

                CvInvoke.Imshow(hi.ToString(), binaryFrame);
                //int numberOfPips = Recognize(diceArray, 3);
                int numberOfPips = CountPips(diceArray, 40, 100);

                // Write the number of pips onto the dice surface in the original image
                Point centerOfDice = new Point(boundingRectangle.X + boundingRectangle.Width / 2 - 5, boundingRectangle.Y + boundingRectangle.Height / 2 + 5);
                CvInvoke.PutText(rawFrame, numberOfPips.ToString(), centerOfDice, FontFace.HersheySimplex, 1.0, new MCvScalar(0, 0, 255), 2);
            }
        }
        private int Recognize(byte[,] input, int searchSize = 3)
        {
            //OutToConsole(input);
            int xGrid = input.GetLength(0) / 6;
            int yGrid = input.GetLength(1) / 6;
            int foundDots = 0;
            List<string> saver = new List<string>();

            for (int x = 1; x < 6; x+=2)
            {
                for (int y = 1; y < 6; y += 2)
                {
                    if (SearchArea(input,xGrid*x,yGrid*y,searchSize))
                    {
                        foundDots++;
                        saver.Add($"FOUND-{xGrid*x}-{yGrid*y}  -  DIR:{input[xGrid*x,yGrid*y]}");
                    }
                    else
                    {
                        saver.Add($"EMPTY-{xGrid * x}-{yGrid * y}  -  DIR:{input[xGrid * x, yGrid * y]}");
                    }
                }
            }
            if (foundDots > 6) foundDots = 6;

            return foundDots;
        }

        private bool SearchArea(byte[,] input, int searchX, int searchY, int searchSize)
        {
            for (int x = searchSize * -1; x < searchSize; x++)
            {
                for (int y = searchSize * -1; y < searchSize; y++)
                {
                    if (input[searchX + x, searchY + y] == 0)
                    {
                        return true;
                    }
                }
            }


            return false;
        }

        private bool OutToConsole(byte[,] input)
        {
            int xc = 0;
            int yc = 0;
            for (int y = 0; y < input.GetLength(1); y++)
            {
                yc = 0;
                xc++;
                for (int x = 0; x < input.GetLength(0); x++)
                {
                    yc++;
                    string s = "0";
                    if (input[x, y] == 255) s = "1";
                    Console.Write(s);
                }
                Console.WriteLine();
            }
            Console.WriteLine("\n\n");

            return false;
        }
        private int CountPips(byte[,] diceArray, int min=30, int max=60)
        {
            int width = diceArray.GetLength(0);
            int height = diceArray.GetLength(1);
            bool[,] visited = new bool[width, height];
            int numberOfPips = 0;
            List<int> blobCount = new List<int>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (diceArray[x, y] == 0 && !visited[x, y]) // If the pixel is black and has not been visited
                    {
                        // Start a depth-first search from this pixel
                        Stack<Point> stack = new Stack<Point>();
                        stack.Push(new Point(x, y));
                        int blobSize = 0;
                        while (stack.Count > 0)
                        {
                            Point p = stack.Pop();
                            if (p.X >= 0 && p.X < width && p.Y >= 0 && p.Y < height && diceArray[p.X, p.Y] == 0 && !visited[p.X, p.Y])
                            {
                                visited[p.X, p.Y] = true;
                                blobSize++;
                                stack.Push(new Point(p.X - 1, p.Y));
                                stack.Push(new Point(p.X + 1, p.Y));
                                stack.Push(new Point(p.X, p.Y - 1));
                                stack.Push(new Point(p.X, p.Y + 1));
                            }
                        }
                        blobCount.Add(blobSize);

                        // Only count blobs that are within a certain size range
                        if (blobSize >= min && blobSize <= max)
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