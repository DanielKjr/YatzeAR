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
        /// <para>Then filters out any contours </para>
        /// </summary>
        /// <param name="binaryImg"></param>
        /// <returns></returns>
        public static VectorOfVectorOfPoint DouglasPeuckerFilter(Mat binaryImg, int minContourSize = 0)
        {
            VectorOfVectorOfPoint rawContours = new VectorOfVectorOfPoint();
            VectorOfVectorOfPoint approxContours = new VectorOfVectorOfPoint();

            CvInvoke.FindContours(binaryImg, rawContours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < rawContours.Size; i++)
            {
                VectorOfPoint contour = rawContours[i];
                VectorOfPoint approx = new VectorOfPoint();

                CvInvoke.ApproxPolyDP(contour, approx, 10, true);

                if (approx.Size == 4)
                {
                    double contourArea = CvInvoke.ContourArea(approx, true);
                    bool area = true;

                    if (minContourSize <= 0)
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
        /// <param name="rawFrame"></param>
        public static void DrawAreaAsText(VectorOfVectorOfPoint DPCounter, Mat rawFrame)
        {
            for (int i = 0; i < DPCounter.Size; i++)
            {
                Rectangle boundingRectangle = CvInvoke.BoundingRectangle(DPCounter[i]);
                int area = (int)CvInvoke.ContourArea(DPCounter[i]);

                // Display the area size on the image
                Point areaTextLocation = new Point(boundingRectangle.X, boundingRectangle.Y - 10); // Position the text above the contour
                CvInvoke.PutText(rawFrame, $"{area}", areaTextLocation, FontFace.HersheySimplex, 0.5f, new MCvScalar(0, 255, 0));
            }
        }

        /// <summary>
        /// Draws a number of pips onto the dice
        /// </summary>
        /// <param name="numberOfPips"></param>
        /// <param name="boundingRectangle"></param>
        /// <param name="rawFrame"></param>
        /// <param name="fontThickness"></param>
        public static void DrawPipCountAsText(int numberOfPips, Rectangle boundingRectangle, Mat rawFrame, int fontThickness = 2)
        {
            Point centerOfDice = new Point(boundingRectangle.X + boundingRectangle.Width / 2 - 5, boundingRectangle.Y + boundingRectangle.Height / 2 + 5);
            CvInvoke.PutText(rawFrame, numberOfPips.ToString(), centerOfDice, FontFace.HersheySimplex, 1.0, new MCvScalar(0, 0, 255), fontThickness);
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

        public static VectorOfVectorOfPoint GetValidContours(VectorOfVectorOfPoint contours)
        {
            VectorOfVectorOfPoint validContours = new VectorOfVectorOfPoint();


            for (int i = 0; i < contours.Size; i++)
            {
                VectorOfPoint cont = contours[i];

                VectorOfPoint approxPoly = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(cont, approxPoly, 8, true);

                if (approxPoly.Size == 4)
                {
                    double contourLenght = CvInvoke.ArcLength(approxPoly, true);
                    double contourArea = CvInvoke.ContourArea(approxPoly, true);

                    //TODO skal måske også tjekkes
                    bool validSize = contourLenght > 80 && contourLenght < 700;
                    bool validOrientation = contourArea < 0;

                    if (validSize && validOrientation)
                        validContours.Push(approxPoly);
                }

            }
            return validContours;
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

    }
}