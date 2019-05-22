using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FeatureServices.Storage.Migrations
{
    public partial class SqlServerInitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    Reference = table.Column<Guid>(nullable: false),
                    Tenant = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsReadOnly = table.Column<bool>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeatureValue",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    Reference = table.Column<Guid>(nullable: false),
                    Tenant = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsReadOnly = table.Column<bool>(nullable: false),
                    TenantConfigurationId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureValue_TenantConfiguration_TenantConfigurationId",
                        column: x => x.TenantConfigurationId,
                        principalTable: "TenantConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureValue_Name",
                table: "FeatureValue",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureValue_TenantConfigurationId",
                table: "FeatureValue",
                column: "TenantConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantConfiguration_Name",
                table: "TenantConfiguration",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeatureValue");

            migrationBuilder.DropTable(
                name: "TenantConfiguration");
        }
    }
}
