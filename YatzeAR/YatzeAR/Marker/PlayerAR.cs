using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using YatzeAR.Marker;
using YatzeAR.YatzyLogik;

namespace YatzeAR
{
	public class PlayerAR
	{
		private string? _image;
		private Matrix<float>? distCoeffs;
		private Matrix<float>? intrinsics;
		private VideoCapture vCap;
		public Mat Frame { get; set; }
		private bool useCamera = false;

		public PlayerAR(bool useCamera = true, int camIndex = 1)
		{
			if (!useCamera)
			{
				string currentDir = Directory.GetCurrentDirectory();
				string[] tmp = Directory.GetFiles(currentDir, "players_png.png");
				_image = tmp[0];
			}
			this.useCamera = useCamera;

			vCap = new VideoCapture(camIndex);

			AR.ReadIntrinsicsFromFile(out intrinsics, out distCoeffs);
		}

		/// <summary>
		/// Method for detecting User markers
		/// </summary>
		/// <returns>List of all found user markers</returns>
		public List<User> OnFrame()
		{
			List<User> foundUsers = new List<User>();

			if (_image != null || useCamera)
			{
				Mat frame = new Mat();
				bool frameGrabbed = vCap.Read(frame);



				Mat binaryPlayer = AR.ConvertToBinaryFrame(frame);

				VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
				CvInvoke.FindContours(binaryPlayer, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

				VectorOfVectorOfPoint validContours = AR.GetValidContours(contours);
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

					foundUsers.Add(new User() { Marker = playerName, Contour = CvInvoke.BoundingRectangle(validContours[i]) });

					Matrix<float> originScreen = new Matrix<float>(new float[] { .5f, .5f, 0f, 1 });

					CvInvoke.PutText(frame, playerName, AR.WorldToScreen(originScreen, worldToScreenMatrix), FontFace.HersheyPlain, 1d, new MCvScalar(255, 0, 255), 1);
					CvInvoke.Imshow("frame", frame);

				}
			}

			return foundUsers;
		}
		public List<User> OnFrame(VideoCapture videoCapture)
		{
			List<User> foundUsers = new List<User>();

			Frame = frame;


			Mat binaryPlayer = AR.ConvertToBinaryFrame(frame);

			VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
			CvInvoke.FindContours(binaryPlayer, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

			VectorOfVectorOfPoint validContours = AR.GetValidContours(contours);
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

				foundUsers.Add(new User() { Marker = playerName, Contour = CvInvoke.BoundingRectangle(validContours[i]) });

				Matrix<float> originScreen = new Matrix<float>(new float[] { .5f, .5f, 0f, 1 });

				CvInvoke.PutText(frame, playerName, AR.WorldToScreen(originScreen, worldToScreenMatrix), FontFace.HersheyPlain, 1d, new MCvScalar(255, 0, 255), 1);


			}


			return foundUsers;
		}

		public List<User> UpdateUserContour(List<User> users)
		{
			var foundUsers = OnFrame();

			foreach (var found in foundUsers)
			{
				foreach (var user in users)
				{
					//user.Contour = null!;
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