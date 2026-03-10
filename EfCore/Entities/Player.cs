using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FantasyLeagueManager.EfCore.Entities;

public partial class Player
{
    [Key]
    public int PlayerId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string FullName { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string Position { get; set; } = null!;

    [Column("NFLTeam")]
    [StringLength(10)]
    [Unicode(false)]
    public string Nflteam { get; set; } = null!;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal ProjectedPoints { get; set; }

    public int? TeamId { get; set; }
    // Navigation Property linking back to the Principal entity
    public virtual Team TeamNavigation { get; set; }
}
