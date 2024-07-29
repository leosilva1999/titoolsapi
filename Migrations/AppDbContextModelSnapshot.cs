﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TiTools_backend.Context;

#nullable disable

namespace TiTools_backend.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("TiTools_backend.Models.Equipment", b =>
                {
                    b.Property<int>("EquipmentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("EquipmentId"));

                    b.Property<bool>("EquipmentLoanStatus")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("EquipmentName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("IpAddress")
                        .HasColumnType("longtext");

                    b.Property<string>("MacAddress")
                        .HasColumnType("longtext");

                    b.Property<string>("QrCode")
                        .HasColumnType("longtext");

                    b.HasKey("EquipmentId");

                    b.ToTable("Equipments");
                });
#pragma warning restore 612, 618
        }
    }
}
