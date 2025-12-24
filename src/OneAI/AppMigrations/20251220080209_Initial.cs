using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneAI.AppMigrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ApiKey = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    BaseUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsRateLimited = table.Column<bool>(type: "INTEGER", nullable: false),
                    RateLimitResetTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OAuthToken = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsageCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsEditable = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIRequestLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ConversationId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Model = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Instructions = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsStreaming = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequestParams = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    MessageSummary = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RequestBody = table.Column<string>(type: "TEXT", nullable: true),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: true),
                    IsSuccess = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponseSummary = table.Column<string>(type: "TEXT", nullable: true),
                    PromptTokens = table.Column<int>(type: "INTEGER", nullable: true),
                    CompletionTokens = table.Column<int>(type: "INTEGER", nullable: true),
                    TotalTokens = table.Column<int>(type: "INTEGER", nullable: true),
                    RequestStartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RequestEndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DurationMs = table.Column<long>(type: "INTEGER", nullable: true),
                    TimeToFirstByteMs = table.Column<long>(type: "INTEGER", nullable: true),
                    IsRateLimited = table.Column<bool>(type: "INTEGER", nullable: false),
                    RateLimitResetSeconds = table.Column<int>(type: "INTEGER", nullable: true),
                    QuotaInfo = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    SessionStickinessUsed = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClientIp = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Originator = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExtensionData = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRequestLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIRequestLogs_AIAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AIAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIAccounts_IsEnabled",
                table: "AIAccounts",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_AIAccounts_IsRateLimited",
                table: "AIAccounts",
                column: "IsRateLimited");

            migrationBuilder.CreateIndex(
                name: "IX_AIAccounts_Provider",
                table: "AIAccounts",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_AIRequestLogs_AccountId",
                table: "AIRequestLogs",
                column: "AccountId");

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
                name: "IX_SystemSettings_Key",
                table: "SystemSettings",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIRequestLogs");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "AIAccounts");
        }
    }
}
