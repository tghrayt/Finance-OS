using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceOS.Budget.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialBudgetSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "budget");

            migrationBuilder.CreateTable(
                name: "monthly_budgets",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    total_budget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monthly_budgets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "budget_allocations",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    planned_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    actual_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    budget_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_allocations", x => x.id);
                    table.ForeignKey(
                        name: "FK_budget_allocations_monthly_budgets_budget_id",
                        column: x => x.budget_id,
                        principalSchema: "budget",
                        principalTable: "monthly_budgets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_budget_allocations_budget_id_category_id",
                schema: "budget",
                table: "budget_allocations",
                columns: new[] { "budget_id", "category_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_monthly_budgets_household_id_year_month",
                schema: "budget",
                table: "monthly_budgets",
                columns: new[] { "household_id", "year", "month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "budget_allocations",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "monthly_budgets",
                schema: "budget");
        }
    }
}
