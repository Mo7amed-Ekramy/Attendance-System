using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class FixSectionNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SectionNumber",
                table: "CourseSections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SectionNumber",
                table: "CourseSections");
        }
    }
}
