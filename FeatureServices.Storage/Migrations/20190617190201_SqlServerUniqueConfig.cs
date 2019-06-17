using Microsoft.EntityFrameworkCore.Migrations;

namespace FeatureServices.Storage.Migrations
{
    public partial class SqlServerUniqueConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TenantConfiguration_Name",
                table: "TenantConfiguration");

            migrationBuilder.CreateIndex(
                name: "IX_TenantConfiguration_Name",
                table: "TenantConfiguration",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TenantConfiguration_Name",
                table: "TenantConfiguration");

            migrationBuilder.CreateIndex(
                name: "IX_TenantConfiguration_Name",
                table: "TenantConfiguration",
                column: "Name");
        }
    }
}
