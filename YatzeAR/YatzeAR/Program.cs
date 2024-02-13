using Emgu.CV;
using YatzeAR.Configuration;
using YatzeAR.YatzyLogik;

namespace YatzeAR
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var camService = new CameraService(0);
            var diceAR = new DiceAR();
            var playerAR = new PlayerAR();

            var users = Configurator.Configurate(camService, true);

            while (true)
            {
                CapturedImage capture = camService.Capture(); //Fetch new image
                Mat anotherFrame = CameraService.LatestCapture; //Grab latest frame

                if (capture.HasNewFrame)
                {
                    var a = playerAR.OnFrame(capture.Frame, null);
                    var b = diceAR.OnFrame(capture.Frame, a.DrawnFrame);

                    camService.DisplayImage(b.DrawnFrame);
                    //ConsoleTurnHandlerDebug();
                }
            }
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