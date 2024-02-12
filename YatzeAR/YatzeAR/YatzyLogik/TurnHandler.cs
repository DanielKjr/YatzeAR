using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YatzeAR.YatzyLogik
{
	public class TurnHandler
	{

		public List<User> Users { get; set; }
		public Queue<User> CurrentTurn { get; set; } = new Queue<User>();
		public YatzyRule CurrentRule { get; set; }
		public User currentUser { get { return CurrentTurn.First(); } }

		private int ruleIndex = 0;

		public TurnHandler(List<User> users)
		{
			int turn = 1;
			users.ForEach(user =>
			{
				user.TurnOrder = turn;
				turn++;
				CurrentTurn.Enqueue(user);
			});
			Users = users;
			CurrentRule = currentUser.Rules.First();
		}

		/// <summary>
		/// Returnere false hvis der er fejl og true hvis det er succesfuldt
		/// </summary>
		/// <param name="diceRoll"></param>
		/// <returns></returns>
		public bool SubmitDice(List<Dice> diceRoll)
		{
			try
			{
				CheckIfValidDiceAmount(diceRoll.Count);
				HandleTurn(diceRoll);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public void HandleTurn(List<Dice> diceRoll)
		{
			diceRoll.ForEach(i => CurrentRule.Points += AddValidDice(i));
			EndTurn();
		}

		public int AddValidDice(Dice dice)
		{
			int sum = 0;

			if(CurrentRule.ValidDice.Contains(dice.Number))
				sum += dice.Number;

			return sum;
		}

	
		public void EndTurn()
		{
			User temp = currentUser;
			CurrentTurn.Dequeue();
			CurrentTurn.Enqueue(temp);
			CurrentRule = currentUser.Rules[ruleIndex];
			CheckForSpecialRule();
		}

		public void CheckForSpecialRule()
		{
			//hvis reglen er sum tæller den sammen og slutter turen
			if (CurrentRule.Rule == "Sum" && !CurrentRule.FilledIn)
			{
				int points = currentUser.Rules.Where(currentUser => currentUser.FilledIn).Sum(currentUser => currentUser.Points);
				CurrentRule.Points += points;
				EndTurn();
			}
			//hvis reglen er bonus tjekker den om summen er over 63 og tilføjer 50 point hvis den er
			if (CurrentRule.Rule == "Bonus" && !CurrentRule.FilledIn)
			{
				YatzyRule rule = currentUser.Rules.Where(x => x.Rule == "Sum").First();
				if (rule.Points >= 63)
					CurrentRule.Points += 50;
				EndTurn();
			}
			//hvis den nuværende user er sidst på listen af users, og altså er sidst i rækken, skiften ruleIndex så den næste bruger har ny regel
			if (currentUser == Users.Last())
			{
				ruleIndex++;
			}
		}

		public void CheckIfValidDiceAmount(int count)
		{
			if(count <= 0 || count > 5)
				throw new Exception("Invalid dice count");
		}

		public void AddDebugValues()
		{

            for (int x = 0; x < Users.Count; x++)
            {
				for (int i = 0; i < 6; i++)
				{
					Users[x].Rules[i].Points = currentUser.Rules[i].MaxPoints;
				}
			}
            
        }
	}
}
