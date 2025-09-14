using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientNotifier.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqliteMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NamedayMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    Day = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NamedayMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EGN = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Birthday = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Nameday = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NotificationsEnabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NamedayMappings_Month_Day",
                table: "NamedayMappings",
                columns: new[] { "Month", "Day" });

            migrationBuilder.CreateIndex(
                name: "IX_NamedayMappings_Name",
                table: "NamedayMappings",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_People_Birthday",
                table: "People",
                column: "Birthday");

            migrationBuilder.CreateIndex(
                name: "IX_People_EGN",
                table: "People",
                column: "EGN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_People_FirstName_LastName",
                table: "People",
                columns: new[] { "FirstName", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_People_Nameday",
                table: "People",
                column: "Nameday");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NamedayMappings");

            migrationBuilder.DropTable(
                name: "People");
        }
    }
}
