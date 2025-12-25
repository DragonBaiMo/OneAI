using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneAI.LogMigrations
{
    /// <inheritdoc />
    public partial class AddHourlyAggregationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HourlyByAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HourStartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TotalRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessRate = table.Column<double>(type: "REAL", nullable: false),
                    RateLimitedRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalRetries = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalPromptTokens = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalCompletionTokens = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalTokens = table.Column<long>(type: "INTEGER", nullable: false),
                    AvgTokensPerRequest = table.Column<double>(type: "REAL", nullable: false),
                    AvgDurationMs = table.Column<double>(type: "REAL", nullable: false),
                    MinDurationMs = table.Column<long>(type: "INTEGER", nullable: true),
                    MaxDurationMs = table.Column<long>(type: "INTEGER", nullable: true),
                    AvgTimeToFirstByteMs = table.Column<double>(type: "REAL", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourlyByAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HourlyByModels",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HourStartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TotalRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessRate = table.Column<double>(type: "REAL", nullable: false),
                    StreamingRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalRetries = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalPromptTokens = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalCompletionTokens = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalTokens = table.Column<long>(type: "INTEGER", nullable: false),
                    AvgTokensPerRequest = table.Column<double>(type: "REAL", nullable: false),
                    AvgDurationMs = table.Column<double>(type: "REAL", nullable: false),
                    MinDurationMs = table.Column<long>(type: "INTEGER", nullable: true),
                    MaxDurationMs = table.Column<long>(type: "INTEGER", nullable: true),
                    AvgTimeToFirstByteMs = table.Column<double>(type: "REAL", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourlyByModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HourlySummaries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HourStartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessRate = table.Column<double>(type: "REAL", nullable: false),
                    StreamingRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalRetries = table.Column<int>(type: "INTEGER", nullable: false),
                    RateLimitedRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalPromptTokens = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalCompletionTokens = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalTokens = table.Column<long>(type: "INTEGER", nullable: false),
                    AvgTokensPerRequest = table.Column<double>(type: "REAL", nullable: false),
                    AvgDurationMs = table.Column<double>(type: "REAL", nullable: false),
                    MinDurationMs = table.Column<long>(type: "INTEGER", nullable: true),
                    MaxDurationMs = table.Column<long>(type: "INTEGER", nullable: true),
                    P50DurationMs = table.Column<long>(type: "INTEGER", nullable: true),
                    P95DurationMs = table.Column<long>(type: "INTEGER", nullable: true),
                    P99DurationMs = table.Column<long>(type: "INTEGER", nullable: true),
                    AvgTimeToFirstByteMs = table.Column<double>(type: "REAL", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourlySummaries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HourlyByAccounts_AccountId",
                table: "HourlyByAccounts",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_HourlyByAccounts_HourStartTime_AccountId",
                table: "HourlyByAccounts",
                columns: new[] { "HourStartTime", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HourlyByAccounts_Provider",
                table: "HourlyByAccounts",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_HourlyByAccounts_Provider_HourStartTime",
                table: "HourlyByAccounts",
                columns: new[] { "Provider", "HourStartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_HourlyByModels_HourStartTime_Model",
                table: "HourlyByModels",
                columns: new[] { "HourStartTime", "Model" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HourlyByModels_HourStartTime_Provider",
                table: "HourlyByModels",
                columns: new[] { "HourStartTime", "Provider" });

            migrationBuilder.CreateIndex(
                name: "IX_HourlyByModels_Model",
                table: "HourlyByModels",
                column: "Model");

            migrationBuilder.CreateIndex(
                name: "IX_HourlyByModels_Provider",
                table: "HourlyByModels",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_HourlySummaries_HourStartTime",
                table: "HourlySummaries",
                column: "HourStartTime",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HourlySummaries_HourStartTime_TotalRequests",
                table: "HourlySummaries",
                columns: new[] { "HourStartTime", "TotalRequests" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HourlyByAccounts");

            migrationBuilder.DropTable(
                name: "HourlyByModels");

            migrationBuilder.DropTable(
                name: "HourlySummaries");
        }
    }
}
