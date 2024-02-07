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

        public static Mat ResizeBinaryFrame(Mat frame, Size newSize)
        {
            Mat resized = new Mat();
            CvInvoke.Resize(frame, resized, newSize);

            Mat binaryFrame = new Mat();
            CvInvoke.Threshold(resized, binaryFrame, 120, 255, ThresholdType.Otsu);

            return binaryFrame;
        }

        public static VectorOfVectorOfPoint DANIELGetContours(Mat binaryFrame)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(binaryFrame, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            return contours;
        }

        public static VectorOfVectorOfPoint DANIELGetValidContours(VectorOfVectorOfPoint contours)
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

        public static VectorOfMat DANIELUndistortMarkersFromContour(Mat image, VectorOfVectorOfPoint validContours)
        {
            VectorOfMat undistortedMarkers = new VectorOfMat();

            for (int i = 0; i < validContours.Size; i++)
            {
                VectorOfPoint contour = validContours[i];
                Mat homography = CvInvoke.FindHomography(contour, MARKER_SCREEN_COORDS, RobustEstimationAlgorithm.Ransac);

                Mat markerContent = new Mat();
                CvInvoke.WarpPerspective(image, markerContent, homography, new Size(60, 60));

                undistortedMarkers.Push(markerContent);
            }

            return undistortedMarkers;
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

                    if(minContourSize <= 0)
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
        /// Draws Contour Area unto incoming frame
        /// </summary>
        /// <param name="DPCounter"></param>
        /// <param name="rawFrame"></param>
        public static void DrawArea(VectorOfVectorOfPoint DPCounter, Mat rawFrame)
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
        /// Warps incoming frame into multiple mats of correct orientation using Homography
        /// </summary>
        /// <param name="DPContours"></param>
        /// <param name="rawImg"></param>
        /// <param name="size"></param>
        /// <returns>Warped and oriented list of Mats</returns>
        public static List<CorrectedDice> FindHomography(VectorOfVectorOfPoint DPContours, Mat rawImg, int size = 300)
        {
            Point[] pointArray = new Point[4] { new Point(0, 0), new Point(size, 0), new Point(size, 300), new Point(0, size) };
            VectorOfPoint dest = new VectorOfPoint(pointArray);
            List<CorrectedDice> correctedDice = new List<CorrectedDice>();

            for (int i = 0; i < DPContours.Size; i++)
            {
                VectorOfPoint contour = DPContours[i];

                Mat output = new Mat();

                Mat mapMatrix = CvInvoke.FindHomography(contour, dest, RobustEstimationAlgorithm.Ransac);

                CvInvoke.WarpPerspective(rawImg, output, mapMatrix, new Size(size, size));

                correctedDice.Add(new CorrectedDice
                {
                    Mat = output,
                    Contour = contour,
                });
            }

            return correctedDice;
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
    }
    public class CorrectedDice
    {
        public Mat Mat { get; set; } = new Mat();
        public VectorOfPoint Contour { get; set; } = new VectorOfPoint();
    }
}