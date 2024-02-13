using Emgu.CV;
using System.Drawing;
using YatzeAR.Configuration;
using YatzeAR.Marker;
using YatzeAR.YatzyLogik;

namespace YatzeAR
{
	public class Program
	{
		static void Main(string[] args)
		{

			VideoCapture vCap = new VideoCapture(1);

			var users = Configurator.Configurate(vCap);

			users.ForEach(i => i.Rules = YatzyScoreboardReader.GetRules());

			users.First().Rules.ForEach(i => Console.WriteLine(i.Rule));
			TurnHandler turnHandler = new TurnHandler(users);
			PlayerAR markerDetection = new PlayerAR(true);
	
			
			while (true)
			{
				Mat frame = new Mat();
				bool frameGrabbed = vCap.Read(frame);


				var diceAR = new DiceAR(true, 1, 30);


				if (CvInvoke.PollKey() != -1)
				{
					List<Dice> roll = diceAR.OnFrame(frame);
					turnHandler.HandleTurn(roll);
				}
				UI.UI.DrawUserInfo(turnHandler, frame);
				markerDetection.UpdateUserContour(users);
				CvInvoke.Imshow("frame", frame);
				//UI.UI.DrawUserInfo(turnHandler, frame);
			}
		
		

			//UpdateUserContour
			//ConsoleTurnHandlerDebug();

		}








		public static void ConsoleTurnHandlerDebug()
		{
			User user = new User() { Name = "TestUser" };
			User userTwo = new User() { Name = "TestTwo" };


			List<User> users = new List<User>
			{
				user,
				userTwo
			};
			users.ForEach(i => i.Rules = YatzyScoreboardReader.GetRules());
			TurnHandler turnHandler = new TurnHandler(users);

			List<Dice> dice = new List<Dice>
			{
				new Dice(){Number = 1},
				new Dice(){Number = 1},
				new Dice(){Number = 1},
				new Dice(){Number= 2},
				new Dice(){Number = 2},
			};

			if (turnHandler.SubmitDice(dice))
			{
				//mellem 1 og 5 terninger, registrerer points og skifter tur
				Console.WriteLine("Succes");
			}
			else
			{
				//Forkert antal terninger, gør ingenting - kør detection igen måske
				Console.WriteLine("Fejl");
			}


		}
	}
}