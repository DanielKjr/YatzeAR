using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;

namespace YatzeAR
{
    public class DiceAR
    {
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
                CvInvoke.ApproxPolyDP(cont, approxPoly, 4, true);
                if (approxPoly.Size == 4)
                {
                    double contourLenght = CvInvoke.ArcLength(approxPoly, true);
                    double contourArea = CvInvoke.ContourArea(approxPoly, true);

                    bool validSize = contourLenght > 100 && contourLenght < 700;
                    bool validOrientation = contourArea > 0;

                    if (validSize && validOrientation)
                        validContours.Push(approxPoly);
                }
            }
            return validContours;
        }
    }
}