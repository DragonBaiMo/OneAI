using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneAI.AppMigrations
{
    /// <inheritdoc />
    public partial class UpdateIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIRequestLogs_AIAccounts_AccountId",
                table: "AIRequestLogs");

            migrationBuilder.DropTable(
                name: "AIAccountQuotas");

            migrationBuilder.DropIndex(
                name: "IX_AIRequestLogs_AccountId_RequestStartTime",
                table: "AIRequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_AIRequestLogs_ConversationId",
                table: "AIRequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_AIRequestLogs_IsSuccess",
                table: "AIRequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_AIRequestLogs_Model",
                table: "AIRequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_AIRequestLogs_Model_RequestStartTime",
                table: "AIRequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_AIRequestLogs_RequestId",
                table: "AIRequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_AIRequestLogs_RequestStartTime",
                table: "AIRequestLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_AIRequestLogs_AIAccounts_AccountId",
                table: "AIRequestLogs",
                column: "AccountId",
                principalTable: "AIAccounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIRequestLogs_AIAccounts_AccountId",
                table: "AIRequestLogs");

            migrationBuilder.CreateTable(
                name: "AIAccountQuotas",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreditsBalance = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreditsUnlimited = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasCredits = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlanType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PrimaryOverSecondaryLimitPercent = table.Column<int>(type: "INTEGER", nullable: false),
                    PrimaryResetAfterSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    PrimaryResetAt = table.Column<long>(type: "INTEGER", nullable: false),
                    PrimaryUsedPercent = table.Column<int>(type: "INTEGER", nullable: false),
                    PrimaryWindowMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    SecondaryResetAfterSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    SecondaryResetAt = table.Column<long>(type: "INTEGER", nullable: false),
                    SecondaryUsedPercent = table.Column<int>(type: "INTEGER", nullable: false),
                    SecondaryWindowMinutes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIAccountQuotas", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_AIAccountQuotas_AIAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AIAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIRequestLogs_AccountId_RequestStartTime",
                table: "AIRequestLogs",
                columns: new[] { "AccountId", "RequestStartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AIRequestLogs_ConversationId",
                table: "AIRequestLogs",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_AIRequestLogs_IsSuccess",
                table: "AIRequestLogs",
                column: "IsSuccess");

            migrationBuilder.CreateIndex(
                name: "IX_AIRequestLogs_Model",
                table: "AIRequestLogs",
                column: "Model");

            migrationBuilder.CreateIndex(
                name: "IX_AIRequestLogs_Model_RequestStartTime",
                table: "AIRequestLogs",
                columns: new[] { "Model", "RequestStartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AIRequestLogs_RequestId",
                table: "AIRequestLogs",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIRequestLogs_RequestStartTime",
                table: "AIRequestLogs",
                column: "RequestStartTime");

            migrationBuilder.CreateIndex(
                name: "IX_AIAccountQuotas_LastUpdatedAt",
                table: "AIAccountQuotas",
                column: "LastUpdatedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_AIRequestLogs_AIAccounts_AccountId",
                table: "AIRequestLogs",
                column: "AccountId",
                principalTable: "AIAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
