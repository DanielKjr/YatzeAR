using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YatzeAR.YatzyLogik
{
	//burde nok hedde noget andet men kan ikke finde på noget
	public class YatzyRule
	{
		public string Rule { get; set; }
		public int MaxPoints { get; set; }
		public List<int> ValidDice { get; set; } = new List<int>();
		public int Points
		{
			get { return points; }
			set
			{
				points = value > MaxPoints ? MaxPoints : value;
			}
		}
		public bool FilledIn { get { return Points != 0; }}
		private int points = 0;
		public YatzyRule(string rule, int maxpoints, List<int> validDice)
		{
			Rule = rule;
			MaxPoints = maxpoints;
			ValidDice = validDice;
		}
	}
}

