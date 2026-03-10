using FantasyLeagueManager.DataAccess.Interfaces;
using FantasyLeagueManager.EfCore.Context;
using FantasyLeagueManager.Models; 
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyLeagueManager.DataAccess.EfCore
{
    public class EfCoreTeamRepository : IDataAccess<Team>
    {
        private readonly FantasyLeagueDbContext _context;

        public EfCoreTeamRepository(FantasyLeagueDbContext context)
        {
            _context = context;
        }

        // REQUIREMENT: Relational Retrieval using Include()
        public Team GetById(int id)
        {
            // EAGER LOADING: Pulls the Team AND the Players in one SQL query
            var efTeam = _context.Teams
                .Include(t => t.Players)
                .FirstOrDefault(t => t.TeamId == id);

            if (efTeam == null) return null;

            // Map EF Entity -> Domain Model
            return new Team
            {
                TeamId = efTeam.TeamId,
                TeamName = efTeam.TeamName,
                OwnerName = efTeam.OwnerName,

                // Map the included relational data to our Domain's Roster list
                Roster = efTeam.Players.Select(p => new Player
                {
                    PlayerId = p.PlayerId,
                    FullName = p.FullName,
                    Position = p.Position,
                    NFLTeam = p.Nflteam,
                    ProjectedPoints = p.ProjectedPoints,
                    TeamId = p.TeamId
                }).ToList()
            };
        }

        public List<Team> GetAll()
        {
            var efTeams = _context.Teams.Include(t => t.Players).ToList();

            return efTeams.Select(t => new Team
            {
                TeamId = t.TeamId,
                TeamName = t.TeamName,
                OwnerName = t.OwnerName,
                Roster = t.Players.Select(p => new Player
                {
                    PlayerId = p.PlayerId,
                    FullName = p.FullName,
                    Position = p.Position,
                    NFLTeam = p.Nflteam,
                    ProjectedPoints = p.ProjectedPoints,
                    TeamId = p.TeamId
                }).ToList()
            }).ToList();
        }

        // Stubs for interface compliance
        public void Add(Team item) => throw new NotImplementedException();
        public void Update(Team item) => throw new NotImplementedException();
        public void Delete(int id) => throw new NotImplementedException();
        public List<Team> GetByPosition(string position) => throw new NotImplementedException();
    }
}