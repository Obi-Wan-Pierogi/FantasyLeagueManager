using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FantasyLeagueManager.EfCore.Entities;

public partial class RosterLog
{
    [Key]
    public int LogId { get; set; }

    public int PlayerId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string ActionType { get; set; } = null!;

    public int? NewTeamId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime LogDate { get; set; }
}
