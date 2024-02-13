using Emgu.CV;
using YatzeAR.Configuration;
using YatzeAR.YatzyLogik;

namespace YatzeAR
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var unifiedCapturer = new UnifiedVideo(0);
                var diceAR = new DiceAR();

            var users = Configurator.Configurate(unifiedCapturer, true);

            while (true)
            {
                CapturedImage image = unifiedCapturer.Capture(); //Fetch new image
                Mat anotherFrame = UnifiedVideo.LatestCapture; //Grab latest frame

                if (image.GrabSuccess)
                {
                    diceAR.OnFrame(image.Frame);
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