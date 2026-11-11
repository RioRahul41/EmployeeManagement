using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodexaManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLeavemanagmenttable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_EmployeeId_StartDate_EndDate",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ProofFileName",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "LeaveRequests");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "LeaveRequests",
                newName: "ToDate");

            migrationBuilder.RenameColumn(
                name: "ReviewedAt",
                table: "LeaveRequests",
                newName: "ApprovedDate");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "LeaveRequests",
                newName: "FromDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "LeaveRequests",
                newName: "AppliedDate");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "LeaveRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "LeaveType",
                table: "LeaveRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "LeaveRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentPath",
                table: "LeaveRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HRRemarks",
                table: "LeaveRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "LeaveRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDays",
                table: "LeaveRequests",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_EmployeeId_FromDate_ToDate",
                table: "LeaveRequests",
                columns: new[] { "EmployeeId", "FromDate", "ToDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_EmployeeId_FromDate_ToDate",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "DocumentPath",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "HRRemarks",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "TotalDays",
                table: "LeaveRequests");

            migrationBuilder.RenameColumn(
                name: "ToDate",
                table: "LeaveRequests",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "FromDate",
                table: "LeaveRequests",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "ApprovedDate",
                table: "LeaveRequests",
                newName: "ReviewedAt");

            migrationBuilder.RenameColumn(
                name: "AppliedDate",
                table: "LeaveRequests",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "LeaveRequests",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "LeaveType",
                table: "LeaveRequests",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "ProofFileName",
                table: "LeaveRequests",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "LeaveRequests",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedByUserId",
                table: "LeaveRequests",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_EmployeeId_StartDate_EndDate",
                table: "LeaveRequests",
                columns: new[] { "EmployeeId", "StartDate", "EndDate" });
        }
    }
}
