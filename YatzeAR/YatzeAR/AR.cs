using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace YatzeAR
{
    public static class AR
    {
        public static readonly VectorOfPoint MARKER_SCREEN_COORDS = new VectorOfPoint(new[] {
            new Point(0, 0),
            new Point(300, 0),
            new Point(300, 300),
            new Point(0, 300)
        });

        public static readonly MCvPoint3D32f[][] WorldCoors = new[] {
            new MCvPoint3D32f[]{
                new MCvPoint3D32f(0, 0, 0),
                new MCvPoint3D32f(1, 0, 0),
                new MCvPoint3D32f(1, 1, 0),
                new MCvPoint3D32f(0, 1, 0)
            },
            new MCvPoint3D32f[]{
                new MCvPoint3D32f(1, 0, 0),
                new MCvPoint3D32f(1, 1, 0),
                new MCvPoint3D32f(0, 1, 0),
                new MCvPoint3D32f(0, 0, 0)
            },
            new MCvPoint3D32f[]{
                new MCvPoint3D32f(1, 1, 0),
                new MCvPoint3D32f(0, 1, 0),
                new MCvPoint3D32f(0, 0, 0),
                new MCvPoint3D32f(1, 0, 0)
            },
            new MCvPoint3D32f[]{
                new MCvPoint3D32f(0, 1, 0),
                new MCvPoint3D32f(0, 0, 0),
                new MCvPoint3D32f(1, 0, 0),
                new MCvPoint3D32f(1, 1, 0)
            }
        };

        /// <summary>
        /// Converts incoming frame into a binary Mat
        /// </summary>
        /// <param name="frame"></param>
        /// <returns>Binary Mat</returns>
        public static Mat ConvertToBinaryFrame(Mat frame)
        {
            Mat grayFrame = new Mat();
            CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray);

            Mat binaryFrame = new Mat();
            CvInvoke.Threshold(grayFrame, binaryFrame, 120, 255, ThresholdType.Otsu);

            return binaryFrame;
        }

        /// <summary>
        /// Finds Contours and filters them using Douglas Peucker's algorithm
        /// <para><paramref name="areaLessThanContourSize"/> is whether less or greater than should be applied to the calculation</para>
        /// <para><paramref name="curveSize"/> is for the Peucker calculations Curve size</para>
        /// </summary>
        /// <param name="binaryImg"></param>
        /// <param name="curveSize"></param>
        /// <param name="minContourSize"></param>
        /// <param name="areaLessThanContourSize"></param>
        /// <returns></returns>
        public static VectorOfVectorOfPoint DouglasPeuckerFilter(Mat binaryImg, int curveSize = 10, bool areaLessThanContourSize = true, int minContourSize = 0)
        {
            VectorOfVectorOfPoint rawContours = new VectorOfVectorOfPoint();
            VectorOfVectorOfPoint approxContours = new VectorOfVectorOfPoint();

            CvInvoke.FindContours(binaryImg, rawContours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < rawContours.Size; i++)
            {
                VectorOfPoint contour = rawContours[i];
                VectorOfPoint approx = new VectorOfPoint();

                CvInvoke.ApproxPolyDP(contour, approx, curveSize, true);

                if (approx.Size == 4)
                {
                    double contourArea = CvInvoke.ContourArea(approx, true);
                    bool area = true;

                    if (areaLessThanContourSize)
                    {
                        area = contourArea < minContourSize;
                    }
                    else
                    {
                        area = contourArea > minContourSize;
                    }

                    if (area)
                    {
                        approxContours.Push(approx);
                    }
                }
            }

            return approxContours;
        }

        /// <summary>
        /// Draws Contour Area as text unto incoming frame
        /// </summary>
        /// <param name="DPCounter"></param>
        /// <param name="drawFrame"></param>
        public static void DrawAreaAsText(VectorOfVectorOfPoint DPCounter, Mat drawFrame)
        {
            for (int i = 0; i < DPCounter.Size; i++)
            {
                Rectangle boundingRectangle = CvInvoke.BoundingRectangle(DPCounter[i]);
                int area = (int)CvInvoke.ContourArea(DPCounter[i]);

                // Display the area size on the image
                Point areaTextLocation = new Point(boundingRectangle.X, boundingRectangle.Y - 10); // Position the text above the contour
                CvInvoke.PutText(drawFrame, $"{area}", areaTextLocation, FontFace.HersheySimplex, 0.5f, new MCvScalar(0, 255, 0));
            }
        }

        /// <summary>
        /// Draws a number of pips onto the dice
        /// </summary>
        /// <param name="numberOfPips"></param>
        /// <param name="boundingRectangle"></param>
        /// <param name="drawFrame"></param>
        /// <param name="fontThickness"></param>
        public static void DrawPipCountAsText(int numberOfPips, VectorOfPoint contour, Mat drawFrame, int fontThickness = 2)
        {
            Rectangle boundingRectangle = CvInvoke.BoundingRectangle(contour);

            Point centerOfDice = new Point(boundingRectangle.X + boundingRectangle.Width / 2 - 5, boundingRectangle.Y + boundingRectangle.Height / 2 + 5);

            CvInvoke.PutText(drawFrame, numberOfPips.ToString(), centerOfDice, FontFace.HersheySimplex, 1.0, new MCvScalar(0, 0, 255), fontThickness);
        }

        /// <summary>
        /// Warps incoming frame into multiple mats of correct orientation using Homography
        /// </summary>
        /// <param name="DPContours"></param>
        /// <param name="rawFrame"></param>
        /// <param name="size"></param>
        /// <returns>Warped and oriented list of Mats</returns>
        public static List<Dice> FindHomography(VectorOfVectorOfPoint DPContours, Mat rawFrame, int size = 300)
        {
            Point[] pointArray = new Point[4] { new Point(0, 0), new Point(size, 0), new Point(size, 300), new Point(0, size) };
            VectorOfPoint dest = new VectorOfPoint(pointArray);
            List<Dice> dices = new List<Dice>();

            for (int i = 0; i < DPContours.Size; i++)
            {
                VectorOfPoint contour = DPContours[i];

                Mat output = new Mat();

                Mat mapMatrix = CvInvoke.FindHomography(contour, dest, RobustEstimationAlgorithm.Ransac);

                CvInvoke.WarpPerspective(rawFrame, output, mapMatrix, new Size(size, size));

                dices.Add(new Dice
                {
                    Mat = output,
                    Contour = contour,
                });
            }

            return dices;
        }

        /// <summary>
        /// Converts to incoming binary frame into a byte array
        /// </summary>
        /// <param name="binaryFrame"></param>
        /// <returns></returns>
        public static byte[,] FrameToByteArray(Mat binaryFrame)
        {
            Image<Gray, byte> binaryImage = binaryFrame.ToImage<Gray, byte>();

            int xSize = binaryFrame.Width;
            int ySize = binaryFrame.Height;

            byte[,] diceArray = new byte[xSize, ySize];

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    diceArray[x, y] = binaryImage.Data[y, x, 0];
                }
            }

            return diceArray;
        }

        /// <summary>
        /// Reads camera specific intrinsics data from file.
        /// </summary>
        /// <param name="intrinsics"></param>
        /// <param name="distCoeffs"></param>
        public static void ReadIntrinsicsFromFile(out Matrix<float> intrinsics, out Matrix<float> distCoeffs)
        {
            Mat intrinsicsMat = new Mat();
            Mat distCoeffsMat = new Mat();

            using FileStorage fs = new FileStorage("intrinsics.json", FileStorage.Mode.Read);

            FileNode intrinsicsNode = fs.GetNode("Intrinsics");
            FileNode distCoeffsNode = fs.GetNode("DistCoeffs");

            intrinsicsNode.ReadMat(intrinsicsMat);
            distCoeffsNode.ReadMat(distCoeffsMat);

            intrinsics = new Matrix<float>(3, 3);
            distCoeffs = new Matrix<float>(1, 5);

            intrinsicsMat.ConvertTo(intrinsics, DepthType.Cv32F);
            distCoeffsMat.ConvertTo(distCoeffs, DepthType.Cv32F);
        }

        /// <summary>
        /// Resizes a binary Mat to desired size and reapplies Otsu filter to ensure to 'smoothing' appears.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="newSize"></param>
        /// <returns>Resized mat</returns>
        public static Mat ResizeBinaryFrame(Mat frame, int newSize)
        {
            Mat resized = new Mat();
            CvInvoke.Resize(frame, resized, new Size(newSize, newSize));

            Mat binaryFrame = new Mat();
            CvInvoke.Threshold(resized, binaryFrame, 120, 255, ThresholdType.Otsu);

            return binaryFrame;
        }

        public static Point WorldToScreen(Matrix<float> worldPoint, Matrix<float> projection)
        {
            Matrix<float> result = projection * worldPoint;
            return new Point((int)(result[0, 0] / result[2, 0]), (int)(result[1, 0] / result[2, 0]));
        }
    }
}