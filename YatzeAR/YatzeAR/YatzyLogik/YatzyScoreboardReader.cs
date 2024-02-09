using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YatzeAR.YatzyLogik
{
	public static class YatzyScoreboardReader
	{
				
		public static List<YatzyRule> GetRules()
		{
			List<YatzyRule> rules = new List<YatzyRule>();
			string line = string.Empty;

			try 
			{
				string directory = System.IO.Directory.GetCurrentDirectory();
				StreamReader sr = new StreamReader(directory + "/Yatzy.txt");
				
				while((line = sr.ReadLine()) != null)
				{
				
					string[] content = line.Split(",");
					string[] validDice = content[2].Split(".");
					int[]ints = validDice.Select(x => Convert.ToInt32(x)).ToArray();
				
					rules.Add(new YatzyRule(content[0], Convert.ToInt32(content[1]), new List<int>(ints)));
				}
				sr.Close();
			}
			catch (Exception ex){
				Console.WriteLine(ex);
			}
			return rules;
		}
	}
}
