using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YatzeAR
{
    public class GameConfigurator
    {
        private static int playerNum = 1;
        private static int expectedAmountOfPlayer = 1;
        public static void Configurate()
        {
            List<Player> players = new List<Player>();
            Console.Write("Please input amount of players for this game: ");
            Console.ReadKey();

            for (int i = 0; i < expectedAmountOfPlayer; i++)
            {
                Console.Write($"Please input name for Player{playerNum}");
                string nameInput = Console.ReadLine() ?? $"Player {i}";

                players.Add( new Player { Name = nameInput });
            }
        }
    }
}
