using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ComplianceDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    account_status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operations",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    recipient_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    recipient_account = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    block_reason_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operations", x => x.id);
                    table.ForeignKey(
                        name: "FK_operations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "appeal_cases",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    operation_id = table.Column<string>(type: "text", nullable: true),
                    case_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    support_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    client_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    missing_info_json = table.Column<string>(type: "jsonb", nullable: true),
                    route_to = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appeal_cases", x => x.id);
                    table.ForeignKey(
                        name: "FK_appeal_cases_operations_operation_id",
                        column: x => x.operation_id,
                        principalTable: "operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_appeal_cases_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "appeal_answers",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    case_id = table.Column<string>(type: "text", nullable: false),
                    question_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    question_text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    answer = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appeal_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_appeal_answers_appeal_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "appeal_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "appeal_documents",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    case_id = table.Column<string>(type: "text", nullable: false),
                    document_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    mock_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appeal_documents", x => x.id);
                    table.ForeignKey(
                        name: "FK_appeal_documents_appeal_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "appeal_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "support_decisions",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    case_id = table.Column<string>(type: "text", nullable: false),
                    decision = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_support_decisions", x => x.id);
                    table.ForeignKey(
                        name: "FK_support_decisions_appeal_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "appeal_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "account_status", "created_at", "full_name", "phone", "updated_at" },
                values: new object[,]
                {
                    { "user_1", "LIMITED", new DateTimeOffset(new DateTime(2026, 5, 30, 10, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Andrey K.", "+7 777 000 00 00", new DateTimeOffset(new DateTime(2026, 5, 30, 10, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user_2", "LIMITED", new DateTimeOffset(new DateTime(2026, 5, 30, 10, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Client Account Appeal", "+7 777 000 00 02", new DateTimeOffset(new DateTime(2026, 5, 30, 10, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "user_3", "ACTIVE", new DateTimeOffset(new DateTime(2026, 5, 30, 10, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Client Operation Confirmation", "+7 777 000 00 03", new DateTimeOffset(new DateTime(2026, 5, 30, 10, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "appeal_cases",
                columns: new[] { "id", "case_type", "client_message", "created_at", "missing_info_json", "operation_id", "route_to", "status", "support_summary", "updated_at", "user_id" },
                values: new object[] { "case_2", "ACCOUNT_BLOCK_APPEAL", null, new DateTimeOffset(new DateTime(2026, 5, 30, 10, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "[\"Source of funds confirmation is required\", \"Purpose of incoming transfers is required\"]", null, "COMPLIANCE", "NEED_MORE_INFO", "Client reported account restriction after several incoming transfers. Supporting documents are not attached yet.", new DateTimeOffset(new DateTime(2026, 5, 30, 10, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user_2" });

            migrationBuilder.InsertData(
                table: "operations",
                columns: new[] { "id", "amount", "block_reason_code", "created_at", "currency", "recipient_account", "recipient_name", "status", "updated_at", "user_id" },
                values: new object[,]
                {
                    { "op_1", 250000m, "CLIENT_CONFIRMATION_REQUIRED", new DateTimeOffset(new DateTime(2026, 5, 30, 10, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "KZT", "KZ00 **** **** 1234", "Alisher M.", "PENDING_CONFIRMATION", new DateTimeOffset(new DateTime(2026, 5, 30, 10, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user_1" },
                    { "op_3", 45000m, "CLIENT_CONFIRMATION_REQUIRED", new DateTimeOffset(new DateTime(2026, 5, 30, 10, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "KZT", "KZ00 **** **** 9876", "Service Company", "PENDING_CONFIRMATION", new DateTimeOffset(new DateTime(2026, 5, 30, 10, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user_3" }
                });

            migrationBuilder.InsertData(
                table: "appeal_cases",
                columns: new[] { "id", "case_type", "client_message", "created_at", "missing_info_json", "operation_id", "route_to", "status", "support_summary", "updated_at", "user_id" },
                values: new object[] { "case_3", "OPERATION_CONFIRMATION", null, new DateTimeOffset(new DateTime(2026, 5, 30, 10, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "[]", "op_3", "SUPPORT", "SUBMITTED", "Client confirmed service payment. Recipient is a company/service. Payment check is attached.", new DateTimeOffset(new DateTime(2026, 5, 30, 10, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user_3" });

            migrationBuilder.CreateIndex(
                name: "IX_appeal_answers_case_id",
                table: "appeal_answers",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_appeal_answers_case_id_question_key",
                table: "appeal_answers",
                columns: new[] { "case_id", "question_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_appeal_cases_operation_id",
                table: "appeal_cases",
                column: "operation_id");

            migrationBuilder.CreateIndex(
                name: "IX_appeal_cases_route_to",
                table: "appeal_cases",
                column: "route_to");

            migrationBuilder.CreateIndex(
                name: "IX_appeal_cases_status",
                table: "appeal_cases",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_appeal_cases_user_id",
                table: "appeal_cases",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_appeal_documents_case_id",
                table: "appeal_documents",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_operations_status",
                table: "operations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_operations_user_id",
                table: "operations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_support_decisions_case_id",
                table: "support_decisions",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_phone",
                table: "users",
                column: "phone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appeal_answers");

            migrationBuilder.DropTable(
                name: "appeal_documents");

            migrationBuilder.DropTable(
                name: "support_decisions");

            migrationBuilder.DropTable(
                name: "appeal_cases");

            migrationBuilder.DropTable(
                name: "operations");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
