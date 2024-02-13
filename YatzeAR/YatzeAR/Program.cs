using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Stitching;
using Emgu.CV.Structure;
using System.Drawing;
using YatzeAR.Configuration;
using YatzeAR.YatzyLogik;

namespace YatzeAR
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var camService = new CameraService(1);
            var diceAR = new DiceAR();
            var playerAR = new PlayerAR();

            var users = Configurator.Configurate(camService, true);
            users.ForEach(i => i.Rules = YatzyScoreboardReader.GetRules());
            TurnHandler turnHandler = new TurnHandler(users);

            while (true)
            {
                CapturedImage capture = camService.Capture(); //Fetch new image
                Mat anotherFrame = CameraService.LatestCapture; //Grab latest frame

                if (capture.HasNewFrame)
                {
                    var a = playerAR.OnFrame(capture.Frame, null);
                    //var b = diceAR.OnFrame(capture.Frame, a.DrawnFrame);
					DrawStuff(turnHandler, a.DrawnFrame);
                    playerAR.UpdateUserContour(turnHandler.Users, a.DrawnFrame);
					camService.DisplayImage(a.DrawnFrame);
                  
                    //ConsoleTurnHandlerDebug();
                }
            }
        }

        private static void DrawStuff(TurnHandler turnHandler, Mat frame) {
			foreach (var item in turnHandler.Users)
			{       
					Point top = new Point(item.Contour.X + item.Contour.Height, item.Contour.Y);
					CvInvoke.PutText(frame, "User: " + item.Name, top, FontFace.HersheyPlain, 1.0, GetColor(turnHandler, item), 1);		
			}
		}
        //doven måde at få farve på
        private static MCvScalar GetColor(TurnHandler turnHandler, User user)
        {
            if (turnHandler.currentUser == user)
                return new MCvScalar(255, 255, 0);
            else
                return new MCvScalar(255, 0, 255);
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