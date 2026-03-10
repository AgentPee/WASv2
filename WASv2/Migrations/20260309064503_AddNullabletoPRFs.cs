using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WASv2.Migrations
{
    /// <inheritdoc />
    public partial class AddNullabletoPRFs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PRFFilePath",
                table: "PRs",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PRFFileName",
                table: "PRs",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PRs",
                keyColumn: "PRFFilePath",
                keyValue: null,
                column: "PRFFilePath",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "PRFFilePath",
                table: "PRs",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "PRs",
                keyColumn: "PRFFileName",
                keyValue: null,
                column: "PRFFileName",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "PRFFileName",
                table: "PRs",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
