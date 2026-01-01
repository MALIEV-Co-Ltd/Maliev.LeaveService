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
            // Note: "level" column never existed in leave_approvals table, so we don't need to drop it
            // migrationBuilder.DropColumn(name: "level", table: "leave_approvals");

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

            // Note: "level" column never existed, so we don't add it in rollback
            // migrationBuilder.AddColumn<int>(name: "level", table: "leave_approvals", type: "integer", nullable: false, defaultValue: 0);
        }
    }
}
