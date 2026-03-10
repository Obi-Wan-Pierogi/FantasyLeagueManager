using FantasyLeagueManager.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using Microsoft.Data.SqlClient;
using FantasyLeagueManager.DataAccess.Interfaces;

namespace FantasyLeagueManager.DataAccess.AdoNet
{
    public class PlayerRepository : IDataAccess<Player>
    {
        // Store the connection string provided by the constructor
        private readonly string _connectionString;

        // SQL Query Constants
        private const string SqlSelectAll = "SELECT PlayerId, FullName, Position, NFLTeam, ProjectedPoints, TeamId FROM Players";

        private const string SqlSelectById = "SELECT PlayerId, FullName, Position, NFLTeam, ProjectedPoints, TeamId FROM Players WHERE PlayerId = @PlayerId";

        private const string SqlSelectByPosition = "SELECT PlayerId," +
            "FullName," +
            "Position," +
            "NFLTeam," +
            "ProjectedPoints," +
            "TeamId" +
            "FROM Players" +
            "WHERE Position = @Position";

        private const string SqlUpdate = @"
            UPDATE Players 
            SET FullName = @FullName,
                Position = @Position,
                NFLTeam = @NFLTeam,
                ProjectedPoints = @ProjectedPoints,
                TeamId = @TeamId
            WHERE PlayerId = @PlayerId";

        private const string SqlDelete = "DELETE FROM Players WHERE PlayerId = @PlayerId";

        private const string SqlTradeUpdate = "UPDATE Players SET TeamId = @TeamId WHERE PlayerId = @PlayerId";

        private const string SqlTradeLog = "INSERT INTO RosterLogs (PlayerId, ActionType, NewTeamId, LogDate) VALUES (@PlayerId, 'Trade', @TeamId, @Date)";

        private const string SqlBaselineTeams = "SELECT TeamId, TeamName, OwnerName FROM Teams";

        private const string SqlBaselinePlayers = "SELECT * FROM Players WHERE TeamId = @TeamId";

        private const string SqlOptimizedJoin = @"
            SELECT t.TeamId, t.TeamName, t.OwnerName, 
                   p.PlayerId, p.FullName, p.Position, p.NFLTeam, p.ProjectedPoints, p.TeamId AS PlayerTeamId
            FROM Teams t
            LEFT JOIN Players p ON t.TeamId = p.TeamId";

        public PlayerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Get All Method
        public List<Player> GetAll()
        {
            var players = new List<Player>();

            using (var connection = new SqlConnection(_connectionString))
            {
                // Usage of Constant
                var command = new SqlCommand(SqlSelectAll, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        players.Add(MapReaderToPlayer(reader));
                    }
                }
            }
            return players;
        }

        // Get By ID Method
        public Player GetById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // Usage of Constant
                var command = new SqlCommand(SqlSelectById, connection);
                command.Parameters.AddWithValue("@PlayerId", id);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read()) return MapReaderToPlayer(reader);
                }
            }
            return null;
        }

        // Get By Position Method
        public List<Player> GetByPosition(string position)
        {
            var players = new List<Player>();

            using (var connection = new SqlConnection(_connectionString))
            {
                // Usage of Constant
                var command = new SqlCommand(SqlSelectByPosition, connection);
                command.Parameters.AddWithValue("@Position", position);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        players.Add(MapReaderToPlayer(reader));
                    }
                }
            }
            return players;
        }

        // Add Method (Not Implemented)
        public void Add(Player item) => throw new NotImplementedException();

        // Update Method
        public void Update(Player item)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // Usage of Constant (Much cleaner!)
                var command = new SqlCommand(SqlUpdate, connection);

                command.Parameters.AddWithValue("@PlayerId", item.PlayerId);
                command.Parameters.AddWithValue("@FullName", item.FullName);
                command.Parameters.AddWithValue("@Position", item.Position);
                command.Parameters.AddWithValue("@NFLTeam", item.NFLTeam);
                command.Parameters.AddWithValue("@ProjectedPoints", item.ProjectedPoints);

                if (item.TeamId.HasValue)
                    command.Parameters.AddWithValue("@TeamId", item.TeamId.Value);
                else
                    command.Parameters.AddWithValue("@TeamId", DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Delete Method
        public void Delete(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // Usage of Constant
                var command = new SqlCommand(SqlDelete, connection);
                command.Parameters.AddWithValue("@PlayerId", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // Transaction Method for Trading a Player
        public void ProcessTrade(int playerId, int newTeamId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Usage of Constants for Transaction
                        using (var updateCmd = new SqlCommand(SqlTradeUpdate, connection, transaction))
                        {
                            updateCmd.Parameters.AddWithValue("@TeamId", newTeamId);
                            updateCmd.Parameters.AddWithValue("@PlayerId", playerId);
                            updateCmd.ExecuteNonQuery();
                        }

                        using (var logCmd = new SqlCommand(SqlTradeLog, connection, transaction))
                        {
                            logCmd.Parameters.AddWithValue("@PlayerId", playerId);
                            logCmd.Parameters.AddWithValue("@TeamId", newTeamId);
                            logCmd.Parameters.AddWithValue("@Date", DateTime.Now);
                            logCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Inefficient "N+1" Query Method
        public List<Team> GetTeamsWithRoster_Baseline(out int queryCount)
        {
            var teams = new List<Team>();
            queryCount = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                queryCount++;

                // Usage of Constant
                using (var teamCmd = new SqlCommand(SqlBaselineTeams, connection))
                using (var reader = teamCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        teams.Add(new Team
                        {
                            TeamId = (int)reader["TeamId"],
                            TeamName = (string)reader["TeamName"],
                            OwnerName = (string)reader["OwnerName"]
                        });
                    }
                }

                foreach (var team in teams)
                {
                    queryCount++;
                    // Usage of Constant
                    using (var playerCmd = new SqlCommand(SqlBaselinePlayers, connection))
                    {
                        playerCmd.Parameters.AddWithValue("@TeamId", team.TeamId);
                        using (var reader = playerCmd.ExecuteReader())
                        {
                            while (reader.Read()) team.Roster.Add(MapReaderToPlayer(reader));
                        }
                    }
                }
            }
            return teams;
        }

        // Optimized Single Query Method
        public List<Team> GetTeamsWithRoster_Optimized()
        {
            var teamDictionary = new Dictionary<int, Team>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // Usage of Constant
                using (var command = new SqlCommand(SqlOptimizedJoin, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int teamId = (int)reader["TeamId"];
                        if (!teamDictionary.TryGetValue(teamId, out Team currentTeam))
                        {
                            currentTeam = new Team
                            {
                                TeamId = teamId,
                                TeamName = (string)reader["TeamName"],
                                OwnerName = (string)reader["OwnerName"]
                            };
                            teamDictionary.Add(teamId, currentTeam);
                        }

                        if (reader["PlayerId"] != DBNull.Value)
                        {
                            currentTeam.Roster.Add(MapReaderToPlayer(reader));
                        }
                    }
                }
            }
            return new List<Team>(teamDictionary.Values);
        }

        // Helper method to map SqlDataReader to Player object
        private Player MapReaderToPlayer(SqlDataReader reader)
        {
            return new Player
            {
                PlayerId = (int)reader["PlayerId"],
                FullName = (string)reader["FullName"],
                Position = (string)reader["Position"],
                NFLTeam = (string)reader["NFLTeam"],
                ProjectedPoints = (decimal)reader["ProjectedPoints"],
                TeamId = reader["TeamId"] == DBNull.Value ? null : (int?)reader["TeamId"]
            };
        }
    }
}