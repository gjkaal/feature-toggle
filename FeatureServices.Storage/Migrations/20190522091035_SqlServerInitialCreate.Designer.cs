﻿// <auto-generated />
using System;
using FeatureServices.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FeatureServices.Storage.Migrations
{
    [DbContext(typeof(FeatureServicesContext))]
    [Migration("20190522091035_SqlServerInitialCreate")]
    partial class SqlServerInitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("FeatureServices.Storage.DbModel.FeatureValue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Created");

                    b.Property<bool>("IsDeleted");

                    b.Property<bool>("IsReadOnly");

                    b.Property<string>("Name")
                        .HasMaxLength(50);

                    b.Property<Guid>("Reference");

                    b.Property<int>("Tenant");

                    b.Property<int>("TenantConfigurationId");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.HasIndex("TenantConfigurationId");

                    b.ToTable("FeatureValue");
                });

            modelBuilder.Entity("FeatureServices.Storage.DbModel.TenantConfiguration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Created");

                    b.Property<bool>("IsDeleted");

                    b.Property<bool>("IsReadOnly");

                    b.Property<string>("Name")
                        .HasMaxLength(50);

                    b.Property<Guid>("Reference");

                    b.Property<int>("Tenant");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("TenantConfiguration");
                });

            modelBuilder.Entity("FeatureServices.Storage.DbModel.FeatureValue", b =>
                {
                    b.HasOne("FeatureServices.Storage.DbModel.TenantConfiguration", "TenantConfiguration")
                        .WithMany("FeatureValue")
                        .HasForeignKey("TenantConfigurationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
