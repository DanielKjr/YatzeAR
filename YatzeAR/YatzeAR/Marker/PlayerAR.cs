using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using YatzeAR.Marker;
using YatzeAR.YatzyLogik;

namespace YatzeAR
{
    public class PlayerAR : FrameLoop
    {
        public List<User> FoundPlayerMarkers = new List<User>();
        private string? _image;
        private Matrix<float>? distCoeffs;
        private Matrix<float>? intrinsics;
        private VideoCapture vCap;

        public PlayerAR()
        {
            vCap = new VideoCapture(0);
            LoadImg();
        }

        public static VectorOfVectorOfPoint GetValidContours(VectorOfVectorOfPoint contours)
        {
            VectorOfVectorOfPoint validContours = new VectorOfVectorOfPoint();
            for (int i = 0; i < contours.Size; i++)
            {
                VectorOfPoint contour = contours[i];

                // Reduce number of points
                VectorOfPoint approxPoly = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(contour, approxPoly, 6, true);

                // Valid contours have 4 points
                if (approxPoly.Size == 4)
                {
                    double contourLength = CvInvoke.ArcLength(approxPoly, true);
                    double contourArea = CvInvoke.ContourArea(approxPoly, true);

                    // Valid contours must also be within the specified size and correct orientation
                    bool validSize = contourLength > 300 && contourLength < 900;
                    bool validOrientation = contourArea > 0;

                    if (validSize && validOrientation)
                        validContours.Push(approxPoly);
                }
            }

            return validContours;
        }

        public void LoadImg()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string[] tmp = Directory.GetFiles(currentDir, "players_png.png");
            _image = tmp[0];
            AR.ReadIntrinsicsFromFile(out intrinsics, out distCoeffs);
        }

        public override void OnFrame()
        {
            if (_image != null)
            {
                FoundPlayerMarkers.Clear();

                Mat frame = CvInvoke.Imread(_image);

                Mat binaryPlayer = AR.ConvertToBinaryFrame(frame);

                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(binaryPlayer, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                VectorOfVectorOfPoint validContours = GetValidContours(contours);
                CvInvoke.DrawContours(frame, validContours, -1, new MCvScalar(255, 0, 0));

                VectorOfMat undistortedPlayers = UndistortPlayerFromContours(frame, validContours);

                for (int i = 0; i < undistortedPlayers.Size; i++)
                {
                    byte[,] centerValues = GetPlayerCenterValues(undistortedPlayers[i]);

                    bool diceFound = Player.TryFindPlayer(centerValues, out string playerName, out int orientIndex);
                    if (!diceFound)
                        continue;

                    bool success = FindPlayerPerspectiveMatrix(orientIndex, validContours[i], out Matrix<float> worldToScreenMatrix);
                    if (!success)
                        continue;

                    FoundPlayerMarkers.Add(new User() { Marker = playerName, Contour = validContours[i] });

                    Matrix<float> originScreen = new Matrix<float>(new float[] { .5f, .5f, 0f, 1 });

                    CvInvoke.PutText(frame, playerName, AR.WorldToScreen(originScreen, worldToScreenMatrix), FontFace.HersheyPlain, 1d, new MCvScalar(255, 0, 255), 1);
                }
            }
        }

        private static byte[,] GetPlayerCenterValues(Mat warpedPlayer)
        {
            Mat grayPlayer = new Mat();
            CvInvoke.CvtColor(warpedPlayer, grayPlayer, ColorConversion.Bgr2Gray);

            Mat binaryPlayer = new Mat();
            CvInvoke.Threshold(grayPlayer, binaryPlayer, 0, 255, ThresholdType.Otsu);

            int gridSize = Player.WARPED_PLAYER_SIZE / Player.PLAYER_GRID_COUNT;
            int halfGridSize = gridSize / 2;

            byte[,] centerValues = new byte[Player.PLAYER_GRID_COUNT, Player.PLAYER_GRID_COUNT];
            for (int y = 0; y < Player.PLAYER_GRID_COUNT; y++)
            {
                for (int x = 0; x < Player.PLAYER_GRID_COUNT; x++)
                {
                    byte[] centerValue = binaryPlayer.GetRawData(new[] {
                            (x * gridSize) + halfGridSize,
                            (y * gridSize) + halfGridSize
                        });

                    centerValues[x, y] = centerValue[0];
                }
            }

            return centerValues;
        }

        private bool FindPlayerPerspectiveMatrix(int orientIndex, VectorOfPoint contour, out Matrix<float> worldToScreenMatrix)
        {
            worldToScreenMatrix = null;

            MCvPoint3D32f[] objOrient = AR.WorldCoors[orientIndex];
            PointF[] contourPoints = contour.ToArray().Select(x => new PointF(x.X, x.Y)).ToArray();

            Matrix<float> rotationVector = new Matrix<float>(3, 1);
            Matrix<float> translationVector = new Matrix<float>(3, 1);
            bool pnpSolved = CvInvoke.SolvePnP(objOrient, contourPoints, intrinsics, distCoeffs, rotationVector, translationVector);

            if (!pnpSolved)
                return false;

            Matrix<float> rotationMatrix = new Matrix<float>(3, 3);
            CvInvoke.Rodrigues(rotationVector, rotationMatrix);

            float[,] rValues = rotationMatrix.Data;
            float[,] tValues = translationVector.Data;

            Matrix<float> rtMatrix = new Matrix<float>(new float[,] {
                    { rValues[0,0], rValues[0,1], rValues[0,2], tValues[0,0] },
                    { rValues[1,0], rValues[1,1], rValues[1,2], tValues[1,0] },
                    { rValues[2,0], rValues[2,1], rValues[2,2], tValues[2,0] }
                });

            worldToScreenMatrix = intrinsics * rtMatrix;
            return true;
        }

        private VectorOfMat UndistortPlayerFromContours(Mat image, VectorOfVectorOfPoint validContours)
        {
            VectorOfMat undistortedPlayers = new VectorOfMat();
            for (int i = 0; i < validContours.Size; i++)
            {
                VectorOfPoint contour = validContours[i];
                Mat homography = CvInvoke.FindHomography(contour, Player.PLAYER_SCREEN_COORDS, RobustEstimationAlgorithm.Ransac);

                Mat playerContent = new Mat();
                CvInvoke.WarpPerspective(image, playerContent, homography, new Size(Player.WARPED_PLAYER_SIZE, Player.WARPED_PLAYER_SIZE));

                undistortedPlayers.Push(playerContent);
            }

            return undistortedPlayers;
        }
    }
}