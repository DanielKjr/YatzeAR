using YatzeAR.YatzyLogik;

namespace YatzeAR
{
	public class Program
	{
		static void Main(string[] args)
		{
			DiceAR diceAR = new DiceAR();
			diceAR.Run();
			//ConsoleTurnHandlerDebug();
		}


		public static void ConsoleTurnHandlerDebug()
		{
			User user = new User("TestUser");
			User userTwo = new User("TestTwo");


			List<User> users = new List<User>
			{
				user,
				userTwo
			};
			users.ForEach(i => i.Rules = YatzyScoreboardReader.GetRules());
			TurnHandler turnHandler = new TurnHandler(users);


			bool gameIsRunning = true;
			while (gameIsRunning)
			{
				Console.WriteLine("Current User: " + turnHandler.currentUser.Name);
				int index = 0;
				turnHandler.currentUser.Rules.ForEach(i =>
				{
					Console.WriteLine("Index: "+ index + " " + i.Rule + " Current points " + i.Points);
					index++;

				});
				Console.WriteLine("Choose a rule to fill in");
				int choice = Convert.ToInt32(Console.ReadLine());
				YatzyRule rule = turnHandler.SelectRule(choice);
				Console.WriteLine("Enter the result of dice roll");
				int value = Convert.ToInt32(Console.ReadLine());
				turnHandler.RegisterResult(rule, value);
				index = 0;
				
				Console.Clear();
			}

		}
	}
}