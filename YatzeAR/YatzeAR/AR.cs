using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YatzeAR
{
	public static class AR
	{
		public static Mat GetGrayFrame(Mat frame, ColorConversion conversion)
		{
			Mat grayframe = new Mat();
			CvInvoke.CvtColor(frame, grayframe, conversion);
			return grayframe;
		}

		public static Mat GetBinaryFrame(Mat grayFrame)
		{
			Mat binaryFrame = new Mat();
			CvInvoke.Threshold(grayFrame, binaryFrame, 120, 255, ThresholdType.Otsu);
			return binaryFrame;
		}

		public static Mat CreateBinaryFrame(Mat frame)
		{
			Mat grayFrame = new Mat();
			CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray);

			Mat binaryFrame = new Mat();
			CvInvoke.Threshold(grayFrame, binaryFrame, 120, 255, ThresholdType.Otsu);
			return binaryFrame;
		}

		public static VectorOfVectorOfPoint GetContours(Mat binaryFrame)
		{
			VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
			CvInvoke.FindContours(binaryFrame, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
			return contours;
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

		public static VectorOfMat UndistortMarkersFromContour(Mat image, VectorOfVectorOfPoint validContours)
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
		public static readonly VectorOfPoint MARKER_SCREEN_COORDS = new VectorOfPoint(new[] {
			new Point(0, 0),
			new Point(300, 0),
			new Point(300, 300),
			new Point(0, 300)
		});

	}
}
