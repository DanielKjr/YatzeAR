using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YatzeAR.Configuration;
using YatzeAR.YatzyLogik;

namespace YatzeAR
{
	public class GameLoop
	{
		private CameraService camService = new CameraService(1);
		private DiceAR diceAR = new DiceAR();
		private PlayerAR playerAR = new PlayerAR();
		private TurnHandler turnHandler;

		public GameLoop()
		{
			List<User> users = Configurator.Configurate(camService);
			users.ForEach(i => i.Rules = YatzyScoreboardReader.GetRules());
			turnHandler = new TurnHandler(users);
		}



		public void Run()
		{

			while (true)
			{
				CapturedImage capture = camService.Capture(); //Fetch new image
				Mat anotherFrame = CameraService.LatestCapture; //Grab latest frame

				if (capture.HasNewFrame)
				{
					var a = playerAR.OnFrame(capture.Frame, null);
					var b = diceAR.OnFrame(capture.Frame, a.DrawnFrame);
					playerAR.UpdateUserContour(turnHandler.Users, a.DrawnFrame);
					DrawStuff(a.DrawnFrame);
				
					if(CvInvoke.WaitKey(5) != -1)
					{
						turnHandler.SubmitDice(b.Dices);
					}
					camService.DisplayImage(a.DrawnFrame);

					//ConsoleTurnHandlerDebug();
				}
			}
		}

		private  void DrawStuff(Mat frame)
		{
			foreach (var user in turnHandler.Users)
			{
				
				DrawInfo(user.Name, frame, user);
				DrawInfo("Turn order "+user.TurnOrder.ToString(), frame, user, 30);
				DrawInfo("Score: "+ user.Score, frame, user, 50);

				if(turnHandler.currentUser == user)
					DrawInfo("Roll for: " + turnHandler.CurrentRule.Rule, frame, user, 70);
				

			}
		}
		//doven måde at få farve på
		private  MCvScalar GetColor(TurnHandler turnHandler, User user)
		{
			if (turnHandler.currentUser == user)
				return new MCvScalar(255, 255, 0);
			else
				return new MCvScalar(0, 255, 0);
		}

		private  void DrawInfo(string info, Mat frame, User user, int yDifference = 10)
		{
			Point point = new Point(user.Rectangle.X, user.Rectangle.Y + yDifference);
			CvInvoke.PutText(frame, info, point, FontFace.HersheyPlain, 1.0, new MCvScalar(0, 0, 0), 3);
			CvInvoke.PutText(frame, info, point, FontFace.HersheyPlain, 1.0, GetColor(turnHandler, user), 1);
		}
	}
}
