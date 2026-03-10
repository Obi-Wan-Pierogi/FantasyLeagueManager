using FantasyLeagueManager.DataAccess.Interfaces;
using FantasyLeagueManager.EfCore.Context;
using FantasyLeagueManager.EfCore.Entities; // EF Entities
using FantasyLeagueManager.Models; // Domain Models
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using DomainPlayer = FantasyLeagueManager.Models.Player;
// Alias to avoid ambiguity between Domain Player and EF Player
using EfPlayer = FantasyLeagueManager.EfCore.Entities.Player;

namespace FantasyLeagueManager.DataAccess.EfCore
{
    public class EfCorePlayerRepository : IDataAccess<DomainPlayer>
    {
        private readonly FantasyLeagueDbContext _context;

        public EfCorePlayerRepository(FantasyLeagueDbContext context)
        {
            _context = context;
        }

        // --- READ OPERATIONS (Week 6) ---
        public List<DomainPlayer> GetAll()
        {
            var efEntities = _context.Players.ToList();
            return efEntities.Select(MapToDomain).ToList();
        }

        public DomainPlayer GetById(int id)
        {
            var e = _context.Players.FirstOrDefault(p => p.PlayerId == id);
            return e == null ? null : MapToDomain(e);
        }

        // --- WRITE OPERATIONS (Week 7) ---

        public void Add(DomainPlayer item)
        {
            // 1. Map Domain -> EF Entity
            var newEntity = new EfPlayer
            {
                FullName = item.FullName,
                Position = item.Position,
                Nflteam = item.NFLTeam, // Note casing from scaffolding
                ProjectedPoints = item.ProjectedPoints,
                TeamId = item.TeamId
            };

            // 2. Add to Context
            _context.Players.Add(newEntity);

            // 3. Persist
            _context.SaveChanges();

            // Optional: Update ID back to domain object if needed
            item.PlayerId = newEntity.PlayerId;
        }

        public void Update(DomainPlayer item)
        {
            // 1. Load the entity (Tracked Query)
            var existingEntity = _context.Players.Find(item.PlayerId);

            // Behavior Equivalence: If found, update. If not, do nothing (safe).
            if (existingEntity != null)
            {
                // 2. Modify properties
                existingEntity.FullName = item.FullName;
                existingEntity.Position = item.Position;
                existingEntity.Nflteam = item.NFLTeam;
                existingEntity.ProjectedPoints = item.ProjectedPoints;
                existingEntity.TeamId = item.TeamId;

                // 3. Persist
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            // 1. Load by ID
            var entity = _context.Players.Find(id);

            // 2. Remove if found
            if (entity != null)
            {
                _context.Players.Remove(entity);

                // 3. Persist
                _context.SaveChanges();
            }
        }

        public List<DomainPlayer> GetByPosition(string position)
        {
            // Create a parameter to prevent SQL injection
            var positionParam = new SqlParameter("@Position", position);

            // Execute raw SQL but let EF Core map the results to entities
            var efEntities = _context.Players
                .FromSqlRaw("SELECT * FROM Players WHERE Position = @Position", positionParam)
                .ToList();

            // Map to domain models
            return efEntities.Select(MapToDomain).ToList();
        }

        // Helper to map EF -> Domain
        private DomainPlayer MapToDomain(EfPlayer e)
        {
            return new DomainPlayer
            {
                PlayerId = e.PlayerId,
                FullName = e.FullName,
                Position = e.Position,
                NFLTeam = e.Nflteam,
                ProjectedPoints = e.ProjectedPoints,
                TeamId = e.TeamId
            };
        }
    }
}