using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using YatzeAR.DTO;
using YatzeAR.Marker;
using YatzeAR.YatzyLogik;

namespace YatzeAR
{
    public class PlayerAR
    {
        private Matrix<float>? distCoeffs;
        private Matrix<float>? intrinsics;

        public PlayerAR()
        {
            AR.ReadIntrinsicsFromFile(out intrinsics, out distCoeffs);
        }

        /// <summary>
        /// Input a raw camera feed and a frame to draw upon to search for Player Markers
        /// </summary>
        /// <param name="rawFrame"></param>
        /// <param name="drawFrame"></param>
        /// <returns>Object with list of Users and drawn frame containing info</returns>
        public ProcessedMarkers OnFrame(Mat rawFrame, Mat drawFrame)
        {
            List<User> foundUsers = new List<User>();

            if (drawFrame == null)
            {
                drawFrame = new Mat();
                rawFrame.CopyTo(drawFrame);
            }

            if (rawFrame != null)
            {
                Mat binaryPlayer = AR.ConvertToBinaryFrame(rawFrame);

                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(binaryPlayer, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                VectorOfVectorOfPoint validContours = AR.GetValidContours(contours);
                CvInvoke.DrawContours(rawFrame, validContours, -1, new MCvScalar(255, 0, 0));

                VectorOfMat undistortedPlayers = UndistortPlayerFromContours(rawFrame, validContours);

                for (int i = 0; i < undistortedPlayers.Size; i++)
                {
                    byte[,] centerValues = GetPlayerCenterValues(undistortedPlayers[i]);

                    bool diceFound = Player.TryFindPlayer(centerValues, out string playerName, out int orientIndex);
                    if (!diceFound)
                        continue;

                    bool success = FindPlayerPerspectiveMatrix(orientIndex, validContours[i], out Matrix<float> worldToScreenMatrix);
                    if (!success)
                        continue;

                    foundUsers.Add(new User() { Marker = playerName, Contour = validContours[i] });

                    Matrix<float> originScreen = new Matrix<float>(new float[] { .5f, .5f, 0f, 1 });

                    CvInvoke.PutText(drawFrame, playerName, AR.WorldToScreen(originScreen, worldToScreenMatrix), FontFace.HersheyPlain, 1d, new MCvScalar(255, 0, 255), 1);
                }
            }

            return new ProcessedMarkers { Users = foundUsers, DrawnFrame = rawFrame };
        }

        public List<User> UpdateUserContour(List<User> users, Mat rawFrame)
        {
            var foundUsers = OnFrame(rawFrame, null);

            foreach (var found in foundUsers.Users)
            {
                foreach (var user in users)
                {
                    if (user.Marker == found.Marker)
                    {
                        user.Contour = found.Contour;
                        break;
                    }
                }
            }

            return users;
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
            worldToScreenMatrix = default!;

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