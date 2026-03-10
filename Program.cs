using FantasyLeagueManager.DataAccess.AdoNet;
using FantasyLeagueManager.DataAccess.EfCore;
using FantasyLeagueManager.DataAccess.Interfaces;
using FantasyLeagueManager.EfCore.Context;
using FantasyLeagueManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;

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
                                ?? $"Server=\"DESKTOP-L00PKD4\\MYSERVER\";Database=FantasyLeagueDb;" +
                                $"Integrated Security=True;TrustServerCertificate=True;Connect Timeout=30;";

            //string connString = "Server=db;Database=FantasyLeagueDb;User Id=sa;Password=SuperSecret_Password_123!;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<FantasyLeagueDbContext>();
            optionsBuilder.UseSqlServer(connString);

            Console.WriteLine("==================================================");
            Console.WriteLine(" FANTASY LEAGUE MANAGER - DOCKERIZED FINAL");
            Console.WriteLine(" Developer: Lee Houk");
            Console.WriteLine("==================================================\n");

            try
            {
                using (var context = new FantasyLeagueDbContext(optionsBuilder.Options))
                {
                    // ---------------------------------------------------------
                    // DOCKER STARTUP & EF CORE MIGRATION
                    // ---------------------------------------------------------
                    Console.WriteLine("Checking database connection and applying migrations...");
                    Console.WriteLine("(If running in Docker for the first time, waiting 3 seconds for SQL Server to boot...)");

                    // Pauses execution to allow the SQL Server container time to fully start
                    Thread.Sleep(3000);

                    // Instantiate repositories once, outside the loop
                    IDataAccess<Player> adoRepo = new PlayerRepository(connString);
                    IDataAccess<Player> efRepo = new EfCorePlayerRepository(context);
                    IDataAccess<Team> teamRepo = new EfCoreTeamRepository(context);

                    bool exitApp = false;

                    while (!exitApp)
                    {
                        Console.WriteLine("\n==============================");
                        Console.WriteLine("   Fantasy League Manager");
                        Console.WriteLine("==============================");
                        Console.WriteLine("1. Verify Architecture (Swappable DAL)");
                        Console.WriteLine("2. List All Teams (IDs and Names)"); 
                        Console.WriteLine("3. View Team Roster (EF Core .Include)");
                        Console.WriteLine("4. Run CRUD Operations Demonstration");
                        Console.WriteLine("5. Find Players by Position (Interactive Search)");
                        Console.WriteLine("6. View Transaction Logs");
                        Console.WriteLine("7. Exit");
                        Console.Write("\nSelect an option (1-7): ");

                        string? input = Console.ReadLine();

                        switch (input)
                        {
                            case "1":
                                Console.Clear();
                                Console.WriteLine("--- 1. ARCHITECTURE: ABSTRACTION & INTERFACES ---\n");
                                Console.WriteLine("Repositories initialized using the IDataAccess<T> interface:");
                                Console.WriteLine($"- ADO.NET Player Repo: {adoRepo.GetType().Name}");
                                Console.WriteLine($"- EF Core Player Repo: {efRepo.GetType().Name}");
                                Console.WriteLine($"- EF Core Team Repo: {teamRepo.GetType().Name}");
                                Console.WriteLine("\nSUCCESS: Repositories are decoupled from the UI.");
                                break;

                            case "2":
                                Console.Clear();
                                Console.WriteLine("--- 2. DATABASE OVERVIEW (All Teams) ---\n");
                                // Using EF Core to quickly list all teams and their IDs
                                var allTeams = context.Teams.OrderBy(t => t.TeamId).ToList();

                                Console.WriteLine($"{"ID",-5} | {"Team Name",-25} | {"Owner",-15}");
                                Console.WriteLine(new string('-', 50));
                                foreach (var t in allTeams)
                                {
                                    Console.WriteLine($"{t.TeamId,-5} | {t.TeamName,-25} | {t.OwnerName,-15}");
                                }
                                break;

                            case "3":
                                Console.Clear();
                                Console.WriteLine("--- 3. RELATIONAL DATA (View Team Roster) ---\n");
                                Console.Write("Enter a Team ID to view: ");
                                if (int.TryParse(Console.ReadLine(), out int selectedId))
                                {
                                    var team = teamRepo.GetById(selectedId);
                                    if (team != null)
                                    {
                                        Console.WriteLine($"\nTEAM: {team.TeamName} | Owner: {team.OwnerName}");
                                        Console.WriteLine(new string('-', 40));
                                        foreach (var p in team.Roster)
                                        {
                                            Console.WriteLine($"  -> [{p.Position}] {p.FullName} ({p.NFLTeam})");
                                        }
                                    }
                                    else Console.WriteLine("Team not found.");
                                }
                                break;

                            case "4":
                                Console.Clear();
                                Console.WriteLine("--- 4. EF CORE WRITE OPERATIONS & CHANGE TRACKING ---\n");

                                // CREATE
                                Console.WriteLine("[CREATE] Adding a new player to the database...");
                                var newPlayer = new Player
                                {
                                    FullName = "Demo Rookie",
                                    Position = "WR",
                                    NFLTeam = "SEA",
                                    ProjectedPoints = 12.0m,
                                    TeamId = 1
                                };
                                efRepo.Add(newPlayer);
                                Console.WriteLine($"  -> Success: '{newPlayer.FullName}' added with ID: {newPlayer.PlayerId}");

                                // UPDATE
                                Console.WriteLine("\n[UPDATE] Modifying the new player's projected points via Tracked Update...");
                                newPlayer.ProjectedPoints = 25.5m;
                                newPlayer.FullName = "Demo Rookie (Updated)";
                                efRepo.Update(newPlayer);

                                var verifiedPlayer = efRepo.GetById(newPlayer.PlayerId);
                                if (verifiedPlayer != null)
                                    Console.WriteLine($"  -> Success: Name is now '{verifiedPlayer.FullName}' with Points: {verifiedPlayer.ProjectedPoints}");

                                // DELETE
                                Console.WriteLine($"\n[DELETE] Removing player ID {newPlayer.PlayerId} from the database...");
                                efRepo.Delete(newPlayer.PlayerId);

                                var deletedCheck = efRepo.GetById(newPlayer.PlayerId);
                                if (deletedCheck == null)
                                    Console.WriteLine("  -> Success: Player successfully removed.");
                                break;

                            case "5":
                                Console.Clear();
                                Console.WriteLine("--- 5. HYBRID ACCESS (Interactive Position Search) ---\n");
                                Console.Write("Enter position to search (e.g., QB, WR, RB, BENCH): ");
                                string positionToSearch = Console.ReadLine()?.ToUpper() ?? "QB";

                                // Using your EF Repo which calls FromSqlRaw
                                var players = efRepo.GetByPosition(positionToSearch);

                                Console.WriteLine($"\nFOUND {players.Count} players at position '{positionToSearch}':");
                                foreach (var p in players)
                                {
                                    Console.WriteLine($"  -> {p.FullName} ({p.NFLTeam})");
                                }
                                break;

                            case "6":
                                Console.Clear();
                                Console.WriteLine("--- 6. TRANSACTION LOGS (Detailed Roster History) ---\n");

                                var detailedLogs = context.RosterLogs
                                    .Include(l => l.Player) // This now works!
                                    .OrderByDescending(l => l.LogDate)
                                    .Take(15)
                                    .ToList();

                                Console.WriteLine($"{"Date/Time",-20} | {"Player Name",-20} | {"Action",-10} | {"New Team",-15}");
                                Console.WriteLine(new string('-', 75));

                                foreach (var log in detailedLogs)
                                {
                                    // Access the Player object directly
                                    string playerName = log.Player?.FullName ?? $"ID: {log.PlayerId}";
                                    string targetTeam = context.Teams.Find(log.NewTeamId)?.TeamName ?? "Free Agent";

                                    Console.WriteLine($"{log.LogDate,-20:g} | {playerName,-20} | {log.ActionType,-10} | {targetTeam,-15}");
                                }
                                break;

                            case "7":
                                exitApp = true;
                                Console.Clear();
                                Console.WriteLine("Exiting Fantasy League Manager...");
                                break;

                            default:
                                Console.WriteLine("Invalid selection. Please enter 1-6.");
                                break;
                        }
                    }
                }

                Console.WriteLine("\n==================================================");
                Console.WriteLine(" DEMONSTRATION COMPLETE.");
                Console.WriteLine("==================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nCRITICAL ERROR: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");
                }
            }
        }
    }
}