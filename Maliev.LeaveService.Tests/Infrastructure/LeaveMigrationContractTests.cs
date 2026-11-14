using Maliev.LeaveService.Infrastructure.Data;
using Maliev.LeaveService.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace Maliev.LeaveService.Tests.Infrastructure;

public class LeaveMigrationContractTests
{
    [Fact]
    public void InitialMigration_ProtectsLeaveBalancesAndApprovalRelationships()
    {
        var migration = new InitialCreate();
        var tables = migration.UpOperations.OfType<CreateTableOperation>().ToDictionary(table => table.Name);
        Assert.Equal(new[] { "accrual_runs", "leave_approvals", "leave_balances", "leave_policies", "leave_requests" }, tables.Keys.Order().ToArray());

        var approval = tables["leave_approvals"];
        var relationship = Assert.Single(approval.ForeignKeys);
        Assert.Equal("leave_requests", relationship.PrincipalTable);
        Assert.Equal(new[] { "leave_request_id" }, relationship.Columns);
        Assert.Equal(ReferentialAction.Cascade, relationship.OnDelete);

        var balance = tables["leave_balances"];
        foreach (var name in new[] { "entitled", "used", "pending", "carried_forward" })
        {
            var column = Assert.Single(balance.Columns, item => item.Name == name);
            Assert.Equal("numeric(5,2)", column.ColumnType);
            Assert.False(column.IsNullable);
        }

        var indexes = migration.UpOperations.OfType<CreateIndexOperation>().ToDictionary(index => index.Name);
        Assert.True(indexes["ix_leave_balances_employee_id_leave_type_year"].IsUnique);
        Assert.Equal(new[] { "employee_id", "leave_type", "year" }, indexes["ix_leave_balances_employee_id_leave_type_year"].Columns);
        Assert.True(indexes["ix_accrual_runs_year_month"].IsUnique);
        Assert.Equal(new[] { "year", "month" }, indexes["ix_accrual_runs_year_month"].Columns);
    }

    [Fact]
    public void InitialMigration_RollbackOrdersDependentTablesBeforeTheirPrincipal()
    {
        var dropped = new InitialCreate().DownOperations.OfType<DropTableOperation>().Select(item => item.Name).ToArray();
        Assert.Equal(5, dropped.Length);
        Assert.True(Array.IndexOf(dropped, "leave_approvals") >= 0);
        Assert.True(Array.IndexOf(dropped, "leave_requests") > Array.IndexOf(dropped, "leave_approvals"));
    }

    [Fact]
    public void SnapshotAndMigrationTarget_PreservePostgresConcurrencyTokens()
    {
        var options = new DbContextOptionsBuilder<LeaveDbContext>().UseNpgsql("Host=localhost;Database=ModelCheck").Options;
        using var context = new LeaveDbContext(options);
        var snapshot = context.GetService<IMigrationsAssembly>().ModelSnapshot;
        Assert.NotNull(snapshot);
        var target = new InitialCreate().TargetModel;

        foreach (var entity in new[] { "LeaveRequest", "LeaveBalance", "LeaveApproval" })
        {
            AssertXmin(snapshot.Model, entity);
            AssertXmin(target, entity);
        }
    }

    private static void AssertXmin(IModel model, string name)
    {
        var entity = Assert.Single(model.GetEntityTypes(), item => item.Name.EndsWith("." + name, StringComparison.Ordinal));
        var property = entity.FindProperty("xmin");
        Assert.NotNull(property);
        Assert.Equal(typeof(uint), property.ClrType);
        Assert.Equal("xid", property.GetColumnType());
        Assert.True(property.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
    }
}
