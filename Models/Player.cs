using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyLeagueManager.Models
{
    public class Player
    {
        // Primary Key
        public int PlayerId { get; set; }

        // Core Data
        public string FullName { get; set; }
        public string Position { get; set; }
        public string NFLTeam { get; set; }
        public decimal ProjectedPoints { get; set; }

        // Foreign Key (Nullable)
        public int? TeamId { get; set; }
    }
}
