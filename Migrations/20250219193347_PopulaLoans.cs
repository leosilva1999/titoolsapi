using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiTools_backend.Migrations
{
    /// <inheritdoc />
    public partial class PopulaLoans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder mb)
        {
            mb.Sql("insert into loans(ApplicantName, RequestTime, ReturnTime) " +
    "Values('Leonardo','2025-02-11 18:15:17.926000', null)");
            mb.Sql("insert into loans(ApplicantName, RequestTime, ReturnTime) " +
                "Values('Isac','2025-02-11 18:15:17.926000', null)");
            mb.Sql("insert into loans(ApplicantName, RequestTime, ReturnTime) " +
                "Values('Maria','2025-02-11 18:15:17.926000', null)");
            mb.Sql("insert into loans(ApplicantName, RequestTime, ReturnTime) " +
                "Values('João','2025-02-11 18:15:17.926000', null)");
            mb.Sql("insert into loans(ApplicantName, RequestTime, ReturnTime) " +
                "Values('Pedro','2025-02-11 18:15:17.926000', null)");
            mb.Sql("insert into loans(ApplicantName, RequestTime, ReturnTime) " +
                "Values('Josué','2025-02-11 18:15:17.926000', null)");
            mb.Sql("insert into loans(ApplicantName, RequestTime, ReturnTime) " +
                "Values('Marcos','2025-02-11 18:15:17.926000', null)");
            mb.Sql("insert into loans(ApplicantName, RequestTime, ReturnTime) " +
                "Values('Lucas','2025-02-11 18:15:17.926000', null)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
