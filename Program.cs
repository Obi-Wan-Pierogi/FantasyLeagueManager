using FantasyLeagueManager.DataAccess.AdoNet;
using FantasyLeagueManager.DataAccess.EfCore;
using FantasyLeagueManager.DataAccess.Interfaces;
using FantasyLeagueManager.EfCore.Context;
using FantasyLeagueManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace FantasyLeagueManager
{
    class Program
    {
        static void Main(string[] args)
        {
            // ---------------------------------------------------------
            // SETUP
            // ---------------------------------------------------------
            string connString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")           
                                ?? $"Server=\"DESKTOP-L00PKD4\\MYSERVER\";Database=FantasyLeagueDb;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=30;";
            var optionsBuilder = new DbContextOptionsBuilder<FantasyLeagueDbContext>();
            optionsBuilder.UseSqlServer(connString);

            Console.WriteLine("==================================================");
            Console.WriteLine(" FANTASY LEAGUE MANAGER - FINAL DEMONSTRATION");
            Console.WriteLine(" Developer: Lee Houk");
            Console.WriteLine("==================================================\n");

            Console.WriteLine("Press any key to initialize the Data Access Layer...");
            Console.ReadKey(true);

            try
            {
                using (var context = new FantasyLeagueDbContext(optionsBuilder.Options))
                {
                    // ---------------------------------------------------------
                    // 1. ARCHITECTURE DEMONSTRATION (Swappable DAL)
                    // ---------------------------------------------------------
                    Console.Clear();
                    Console.WriteLine("--- 1. ARCHITECTURE: ABSTRACTION & INTERFACES ---\n");
                    Console.WriteLine("Instantiating repositories using the IDataAccess<T> interface...");

                    // Notice how both implementations satisfy the exact same interface!
                    IDataAccess<Player> adoRepo = new PlayerRepository(connString);
                    IDataAccess<Player> efRepo = new EfCorePlayerRepository(context);
                    IDataAccess<Team> teamRepo = new EfCoreTeamRepository(context);

                    Console.WriteLine("SUCCESS: Repositories initialized without tightly coupling to the UI.");

                    Console.WriteLine("\nPress any key to demonstrate Relational Data (Eager Loading)...");
                    Console.ReadKey(true);

                    // ---------------------------------------------------------
                    // 2. RELATIONAL DATA (Week 8)
                    // ---------------------------------------------------------
                    Console.Clear();
                    Console.WriteLine("--- 2. RELATIONAL DATA (Eager Loading via EF Core) ---\n");
                    int targetTeamId = 1; // Gridiron Gladiators
                    Console.WriteLine($"Fetching Team #{targetTeamId} and its Roster using .Include()...");

                    var team = teamRepo.GetById(targetTeamId);

                    if (team != null)
                    {
                        Console.WriteLine($"\nTEAM FOUND: {team.TeamName} (Owner: {team.OwnerName})");
                        Console.WriteLine($"ROSTER ({team.Roster.Count} players):");
                        foreach (var p in team.Roster)
                        {
                            Console.WriteLine($"  -> [{p.Position}] {p.FullName} (Proj: {p.ProjectedPoints})");
                        }
                    }

                    Console.WriteLine("\nPress any key to demonstrate full CRUD operations...");
                    Console.ReadKey(true);

                    // ---------------------------------------------------------
                    // 3. CRUD & CHANGE TRACKING (Week 7)
                    // ---------------------------------------------------------
                    Console.Clear();
                    Console.WriteLine("--- 3. EF CORE WRITE OPERATIONS & CHANGE TRACKING ---\n");

                    // CREATE
                    Console.WriteLine("[CREATE] Adding a new player to the database...");
                    var newPlayer = new Player
                    {
                        FullName = "Demo Rookie",
                        Position = "WR",
                        NFLTeam = "SEA",
                        ProjectedPoints = 12.0m,
                        TeamId = targetTeamId
                    };
                    efRepo.Add(newPlayer);
                    Console.WriteLine($"  -> Success: '{newPlayer.FullName}' added with ID: {newPlayer.PlayerId}");

                    // UPDATE
                    Console.WriteLine("\n[UPDATE] Modifying the new player's projected points via Tracked Update...");
                    newPlayer.ProjectedPoints = 25.5m;
                    newPlayer.FullName = "Demo Rookie (Updated)";
                    efRepo.Update(newPlayer);

                    var verifiedPlayer = efRepo.GetById(newPlayer.PlayerId);
                    Console.WriteLine($"  -> Success: Name is now '{verifiedPlayer.FullName}' with Points: {verifiedPlayer.ProjectedPoints}");

                    // DELETE
                    Console.WriteLine($"\n[DELETE] Removing player ID {newPlayer.PlayerId} from the database...");
                    efRepo.Delete(newPlayer.PlayerId);

                    var deletedCheck = efRepo.GetById(newPlayer.PlayerId);
                    if (deletedCheck == null)
                        Console.WriteLine("  -> Success: Player successfully removed.");

                    Console.WriteLine("\nPress any key to demonstrate Hybrid Access (Raw SQL)...");
                    Console.ReadKey(true);

                    // ---------------------------------------------------------
                    // 4. HYBRID ACCESS PATTERN (Week 9)
                    // ---------------------------------------------------------
                    Console.Clear();
                    Console.WriteLine("--- 4. HYBRID ACCESS (Raw SQL in EF Core) ---\n");
                    string positionToSearch = "QB";
                    Console.WriteLine($"Executing parameterized Raw SQL (FromSqlRaw) to find all '{positionToSearch}'s...");

                    var qbList = efRepo.GetByPosition(positionToSearch);

                    Console.WriteLine($"\nFOUND {qbList.Count} QUARTERBACKS:");
                    foreach (var qb in qbList)
                    {
                        Console.WriteLine($"  -> {qb.FullName} ({qb.NFLTeam})");
                    }
                }

                Console.WriteLine("\n==================================================");
                Console.WriteLine(" DEMONSTRATION COMPLETE.");
                Console.WriteLine("==================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nCRITICAL ERROR: {ex.Message}");
            }

            Console.ReadKey();
        }
    }
}