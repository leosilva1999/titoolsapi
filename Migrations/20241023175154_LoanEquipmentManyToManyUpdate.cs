using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiTools_backend.Migrations
{
    /// <inheritdoc />
    public partial class LoanEquipmentManyToManyUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_Loans_LoanId",
                table: "Equipments");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_LoanId",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "LoanId",
                table: "Equipments");

            migrationBuilder.CreateTable(
                name: "EquipmentLoan",
                columns: table => new
                {
                    EquipmentsEquipmentId = table.Column<int>(type: "int", nullable: false),
                    LoansLoanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentLoan", x => new { x.EquipmentsEquipmentId, x.LoansLoanId });
                    table.ForeignKey(
                        name: "FK_EquipmentLoan_Equipments_EquipmentsEquipmentId",
                        column: x => x.EquipmentsEquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "EquipmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentLoan_Loans_LoansLoanId",
                        column: x => x.LoansLoanId,
                        principalTable: "Loans",
                        principalColumn: "LoanId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentLoan_LoansLoanId",
                table: "EquipmentLoan",
                column: "LoansLoanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentLoan");

            migrationBuilder.AddColumn<int>(
                name: "LoanId",
                table: "Equipments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_LoanId",
                table: "Equipments",
                column: "LoanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_Loans_LoanId",
                table: "Equipments",
                column: "LoanId",
                principalTable: "Loans",
                principalColumn: "LoanId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
