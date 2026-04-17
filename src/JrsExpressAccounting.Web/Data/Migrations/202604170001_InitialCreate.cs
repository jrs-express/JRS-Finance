using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JrsExpressAccounting.Web.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "dbo");
            // Full scaffold omitted intentionally in this starter template.
            // Run: dotnet ef migrations add InitialCreate after restoring SDK to generate full SQL.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
