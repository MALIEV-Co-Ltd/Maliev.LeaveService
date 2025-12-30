using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.LeaveService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "level",
                table: "leave_approvals");

            migrationBuilder.RenameColumn(
                name: "expiration_period_months",
                table: "leave_policies",
                newName: "max_consecutive_days");

            migrationBuilder.RenameColumn(
                name: "decision_timestamp",
                table: "leave_approvals",
                newName: "decided_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "max_consecutive_days",
                table: "leave_policies",
                newName: "expiration_period_months");

            migrationBuilder.RenameColumn(
                name: "decided_at",
                table: "leave_approvals",
                newName: "decision_timestamp");

            migrationBuilder.AddColumn<int>(
                name: "level",
                table: "leave_approvals",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
