﻿using Emgu.CV;
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
			GameLoop gameLoop = new GameLoop();
			gameLoop.Run();
	
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