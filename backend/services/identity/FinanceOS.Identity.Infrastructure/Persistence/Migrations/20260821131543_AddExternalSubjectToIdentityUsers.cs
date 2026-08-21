using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceOS.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalSubjectToIdentityUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_subject",
                schema: "identity",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_users_external_subject",
                schema: "identity",
                table: "users",
                column: "external_subject",
                unique: true,
                filter: "external_subject <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_external_subject",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "external_subject",
                schema: "identity",
                table: "users");
        }
    }
}
