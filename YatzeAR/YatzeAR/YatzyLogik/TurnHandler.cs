using System;
using System.Collections.Generic;
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
		public User currentUser { get { return CurrentTurn.First(); } }

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
		}


		/// <summary>
		/// Sets the specified rule on the current user to the value
		/// </summary>
		/// <param name="rule"></param>
		/// <param name="temporaryParameter"></param>
		public void RegisterResult(YatzyRule rule, int temporaryParameter)
		{

			//TODO mangler hvordan end terning resulaterne blive repræsenteret
			if(rule != null)
			{
				YatzyRule currentRule = currentUser.Rules.Find(x => x.Rule == rule.Rule && !x.FilledIn)!;
				currentRule.Points = temporaryParameter;
				User temp = currentUser;
				CurrentTurn.Dequeue();
				CurrentTurn.Enqueue(temp);
			}	
		}

		/// <summary>
		/// Returns the rule at said index, if it is not filled in
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public YatzyRule SelectRule(int index)
		{
			YatzyRule rule = currentUser.Rules[index];
			if (!rule.FilledIn)
				return rule;
			else
				return null;
		}

		//if sum of first half of rows is greater than 63, add 50 points
		public void CalculateBonus()
		{
			int sum = 0;
			
			foreach (var rule in currentUser.Rules)
			{
				if (rule.Rule == "Sum")
				{
					if(sum >= 63)
						rule.Points = 50;	
					break;
				}
				else
					sum += rule.Points;
			}
		}
		//TODO mangler tjek til om alle terninger er ens
		public void AddYatzyBonus()
		{
			currentUser.Score += 50;
		}

		public void GetSumOfAlll()
		{
			YatzyRule rule = currentUser.Rules.Last();

			int sum = 0;
			currentUser.Rules.ForEach(i =>
			{
				sum += i.Points;
			});
			rule.Points = sum;

		}
	}
}
