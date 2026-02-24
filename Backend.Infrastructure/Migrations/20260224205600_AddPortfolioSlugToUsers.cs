using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioSlugToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PortfolioSlug",
                table: "users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_users_PortfolioSlug",
                table: "users",
                column: "PortfolioSlug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_PortfolioSlug",
                table: "users");

            migrationBuilder.DropColumn(
                name: "PortfolioSlug",
                table: "users");
        }
    }
}
