using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Policies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAutoRenewal",
                table: "Policies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalPolicyId",
                table: "Policies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RenewalReminderSent",
                table: "Policies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Policies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Policies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Policies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PolicyEndorsements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NewValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyEndorsements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyEndorsements_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RenewalReminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReminderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSent = table.Column<bool>(type: "bit", nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAcknowledged = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenewalReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RenewalReminders_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyEndorsements_PolicyId",
                table: "PolicyEndorsements",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_RenewalReminders_PolicyId",
                table: "RenewalReminders",
                column: "PolicyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PolicyEndorsements");

            migrationBuilder.DropTable(
                name: "RenewalReminders");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "IsAutoRenewal",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "OriginalPolicyId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "RenewalReminderSent",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Policies");
        }
    }
}
