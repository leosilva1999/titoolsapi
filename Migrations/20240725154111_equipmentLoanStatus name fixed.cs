using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiTools_backend.Migrations
{
    /// <inheritdoc />
    public partial class equipmentLoanStatusnamefixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EquipamentLoanStatus",
                table: "Equipments",
                newName: "EquipmentLoanStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EquipmentLoanStatus",
                table: "Equipments",
                newName: "EquipamentLoanStatus");
        }
    }
}
