using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FantasyLeagueManager.EfCore.Entities;

public partial class Team
{
    [Key]
    public int TeamId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string TeamName { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string OwnerName { get; set; } = null!;

    // Navigation Property holding the dependent entities
    public virtual ICollection<Player> Players { get; set; } = new List<Player>();
}
