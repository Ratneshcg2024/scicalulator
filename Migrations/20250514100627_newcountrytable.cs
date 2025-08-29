using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCIMetricAPI.Migrations
{
    /// <inheritdoc />
    public partial class newcountrytable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CpuInfo",
                columns: table => new
                {
                    cpu_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    H_id = table.Column<int>(type: "int", nullable: false),
                    system = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nodes = table.Column<int>(type: "int", nullable: false),
                    maxwatts = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    minwatts = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    cores = table.Column<int>(type: "int", nullable: false),
                    chips = table.Column<int>(type: "int", nullable: false),
                    processor = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    memory = table.Column<int>(type: "int", nullable: false),
                    jvm_vendor = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    jvm_instance = table.Column<int>(type: "int", nullable: false),
                    diskdrive = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CpuInfo", x => x.cpu_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hardwarevendor",
                columns: table => new
                {
                    Hid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HardwareName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hardwarevendor", x => x.Hid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CpuInfo");

            migrationBuilder.DropTable(
                name: "hardwarevendor");
        }
    }
}
