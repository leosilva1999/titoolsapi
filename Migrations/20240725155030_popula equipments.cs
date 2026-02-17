using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiTools_backend.Migrations
{
    /// <inheritdoc />
    public partial class populaequipments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder mb)
        {
            mb.Sql("insert into Equipments(EquipmentName, IpAddress, MacAddress, EquipmentLoanStatus) " +
    "Values('CAP-NTB-ALUN011','10.1.16.26','1e:0b:1f:82:a3:15', 1)");
            mb.Sql("insert into Equipments(EquipmentName, IpAddress, MacAddress, EquipmentLoanStatus) " +
                "Values('CAP-NTB-ALUN016','10.1.16.43','bf:6d:15:21:db:60', 0)");
            mb.Sql("insert into Equipments(EquipmentName, IpAddress, MacAddress, EquipmentLoanStatus) " +
                "Values('CAP-NTB-ALUN023','10.1.16.18','bb:29:17:65:45:ea', 0)");
            mb.Sql("insert into Equipments(EquipmentName, IpAddress, MacAddress, EquipmentLoanStatus) " +
                "Values('CAP-NTB-ALUN047','10.1.16.37','39:0a:1a:ab:6a:38', 0)");
            mb.Sql("insert into Equipments(EquipmentName, IpAddress, MacAddress, EquipmentLoanStatus) " +
                "Values('CAP-NTB-ALUN048','10.1.16.13','1b:9c:76:f8:f8:e5', 0)");
            mb.Sql("insert into Equipments(EquipmentName, IpAddress, MacAddress, EquipmentLoanStatus) " +
                "Values('CAP-NTB-ALUN03','10.1.16.42','4e:df:28:34:7c:1e', 0)");
            mb.Sql("insert into Equipments(EquipmentName, IpAddress, MacAddress, EquipmentLoanStatus) " +
                "Values('CAP-NTB-ALUN050','10.1.16.31','1a:e2:2d:67:ca:ee', 0)");
            mb.Sql("insert into Equipments(EquipmentName, IpAddress, MacAddress, EquipmentLoanStatus) " +
                "Values('CAP-NTB-ALUN07','10.1.16.27','fc:c9:c9:bb:a7:57', 0)");
            mb.Sql("insert into Equipments(EquipmentName, IpAddress, MacAddress, EquipmentLoanStatus) " +
                "Values('CAP-NTB-ALUN01','10.1.16.28','fc:c9:c9:bb:a7:56', 0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder mb)
        {

        }
    }
}
