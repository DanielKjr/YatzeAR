using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YatzeAR.YatzyLogik
{
	public class User
	{
		public string Name { get; set; }

		public int TurnOrder { get; set; }

		public int Score { get; set; }

		public List<YatzyRule> Rules { get; set; } = new List<YatzyRule>();


        public User(string userName)
        {
            Name = userName;
        }
    }
}
