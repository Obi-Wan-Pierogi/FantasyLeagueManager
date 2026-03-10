using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyLeagueManager.Models
{
    public class Team
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string OwnerName { get; set; }

        // Navigation Property: A team has many players
        public List<Player> Roster { get; set; } = new List<Player>();
    }
}
