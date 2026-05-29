using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "feed_rule",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    keyword_pattern = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    mentioned_siren = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    notify_email = table.Column<bool>(type: "boolean", nullable: false),
                    notify_push = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_evaluated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_triggered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    times_triggered = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feed_rule", x => x.id);
                    table.ForeignKey(
                        name: "FK_feed_rule_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_feed_rule_is_active",
                table: "feed_rule",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_feed_rule_user_id",
                table: "feed_rule",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "feed_rule");
        }
    }
}
