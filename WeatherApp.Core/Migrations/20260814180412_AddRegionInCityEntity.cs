using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddRegionInCityEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Cities",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Region",
                table: "Cities");
        }
    }
}
