using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YatzeAR
{
	internal class DiceTest : FrameLoop
	{
		private string? _image;
		private Matrix<float>? intrinsics;
		private Matrix<float>? distCoeffs;

		private VideoCapture _videoCapture;

		public DiceTest()
		{
			_videoCapture = new VideoCapture(1);

			LoadImg();
		}

		public void LoadImg()
		{
			string currentDir = Directory.GetCurrentDirectory();
			string[] tmp = Directory.GetFiles(currentDir, "dice40.png");
			_image = tmp[0];
			ReadIntrinsicsFromFile(out intrinsics, out distCoeffs);
		}

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

		public override void OnFrame()
		{
			if (_image != null)
			{
				Mat frame = CvInvoke.Imread(_image);

				//Mat frame = new Mat();
				//bool frameGrabbed = _videoCapture.Read(frame);
				//if (!frameGrabbed)
				//{
				//	Console.Write("Failed to grab frame");
				//	return;
				//}

				Mat gray = new Mat();
				CvInvoke.CvtColor(frame, gray, ColorConversion.Bgr2Gray);

				Mat binary = new Mat();
				CvInvoke.Threshold(gray, binary, 120, 255, ThresholdType.Otsu);

				VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
				Mat hierachy = new Mat();
				CvInvoke.FindContours(binary, contours, hierachy, RetrType.List, ChainApproxMethod.ChainApproxSimple);

				//// 5.02.2024
				CvInvoke.DrawContours(binary, contours, -1, new MCvScalar(255, 0, 0));
				CvInvoke.Imshow("binary", binary);

				//jeg har reduceret/givet en tolerance på 8 
				VectorOfVectorOfPoint validContours = AR.GetValidContours(contours);

                // 5.02.2024
                CvInvoke.DrawContours(frame, validContours, -1, new MCvScalar(255, 0, 0));
                CvInvoke.Imshow("newContousPicture", frame);

                Image<Gray, byte> binaryImage = binary.ToImage<Gray, byte>();

				//TODO disse afhænger af afstanden fra kameraet så skal justeres
				double minArea = 800; // Set this to the minimum dice area you expect
				double maxArea = 1700; // Set this to the maximum dice area you expect

				//aspect ratio burde være fint som det er
				double minAspectRatio = 0.9; // Dice are roughly square
				double maxAspectRatio = 1.1; // Dice are roughly square

				for (int i = 0; i < validContours.Size; i++)
				{
					Rectangle boundingRectangle = CvInvoke.BoundingRectangle(validContours[i]);
					double area = CvInvoke.ContourArea(validContours[i]);
#if DEBUG
					// Display the area size on the image
					Point areaTextLocation = new Point(boundingRectangle.X, boundingRectangle.Y - 10); // Position the text above the contour
					CvInvoke.PutText(frame, $"Area: {area}", areaTextLocation, FontFace.HersheySimplex, 0.5f, new MCvScalar(0, 255, 0));
#endif
					// Filter based on size
					if (area < minArea || area > maxArea)
						continue;
					

					// Filter based on aspect ratio
					double aspectRatio = (double)boundingRectangle.Width / boundingRectangle.Height;
					if (aspectRatio < minAspectRatio || aspectRatio > maxAspectRatio)
						continue;
					

					// If the contour passes all the filters, it's probably a dice
					int diceWidth = boundingRectangle.Width;
					int diceHeight = boundingRectangle.Height;

					// Create a byte array of the appropriate size
					byte[,] diceArray = new byte[diceWidth, diceHeight];
					// Fill the array with the pixel values from the dice region of the image
					for (int x = 0; x < diceWidth; x++)
					{
						for (int y = 0; y < diceHeight; y++)
						{
							diceArray[x, y] = binaryImage.Data[y + boundingRectangle.Top, x + boundingRectangle.Left, 0];
						}
					}

					
					// Process the diceArray to count the number of pips
					int numberOfPips = CountPips(diceArray);

					// Write the number of pips onto the dice surface in the original image
					Point centerOfDice = new Point(boundingRectangle.X + boundingRectangle.Width / 2, boundingRectangle.Y + boundingRectangle.Height / 2);
					CvInvoke.PutText(frame, numberOfPips.ToString(), centerOfDice, FontFace.HersheySimplex, 1.0, new MCvScalar(0, 0, 255));
					CvInvoke.Imshow("Dice", frame);
				}
				CvInvoke.DrawContours(frame, validContours, -1, new MCvScalar(0, 255, 0), 1);
				CvInvoke.Imshow("Dice", frame);
			}
		}

		//Det her er lidt overkill at køre konstant, så hvis det skal bruges burde det nok være når man trykker på en knap
		private int CountPips(byte[,] diceArray)
		{
			int width = diceArray.GetLength(0);
			int height = diceArray.GetLength(1);
			bool[,] visited = new bool[width, height];
			int numberOfPips = 0;

			int minBlobSize = 30; // Set this to the minimum expected pip size
			//TODO max blob size (som i sort prik) går an på hvor stor terningen er eller hvor tæt på billedet er
			int maxBlobSize = 60; // Set this to the maximum expected pip size

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
						// Only count blobs that are within a certain size range
						if (blobSize >= minBlobSize && blobSize <= maxBlobSize)
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
