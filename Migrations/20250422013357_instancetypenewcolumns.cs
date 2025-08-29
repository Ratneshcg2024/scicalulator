using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCIMetricAPI.Migrations
{
    /// <inheritdoc />
    public partial class instancetypenewcolumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Mid",
                table: "InstanceTypes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "gpuCount",
                table: "InstanceTypes",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "gpuModelName",
                table: "InstanceTypes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "storage",
                table: "InstanceTypes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "storagetype",
                table: "InstanceTypes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ProviderId",
                table: "GridEmissions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mid",
                table: "InstanceTypes");

            migrationBuilder.DropColumn(
                name: "gpuCount",
                table: "InstanceTypes");

            migrationBuilder.DropColumn(
                name: "gpuModelName",
                table: "InstanceTypes");

            migrationBuilder.DropColumn(
                name: "storage",
                table: "InstanceTypes");

            migrationBuilder.DropColumn(
                name: "storagetype",
                table: "InstanceTypes");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                table: "GridEmissions");
        }
    }
}
