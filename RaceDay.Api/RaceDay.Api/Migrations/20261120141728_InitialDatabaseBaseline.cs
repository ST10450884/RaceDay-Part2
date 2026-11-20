
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.Api.Migrations
{
    public partial class InitialDatabaseBaseline : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing RaceDay database tables are already present.
            // This migration only establishes EF Core migration history.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally leave existing database tables untouched.
        }
    }
}
