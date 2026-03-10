using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FantasyLeagueManager.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Baseline_Existing_Database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Empty because the tables already exist in the database. This migration serves as a baseline for future migrations.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "RosterLogs");

            migrationBuilder.DropTable(
                name: "Teams");
        }
    }
}
