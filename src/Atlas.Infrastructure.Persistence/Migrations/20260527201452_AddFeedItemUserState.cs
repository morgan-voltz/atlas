using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedItemUserState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "feed_item_user_state",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    feed_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    is_favorite = table.Column<bool>(type: "boolean", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feed_item_user_state", x => x.id);
                    table.ForeignKey(
                        name: "FK_feed_item_user_state_feed_item_feed_item_id",
                        column: x => x.feed_item_id,
                        principalTable: "feed_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_feed_item_user_state_feed_item_id",
                table: "feed_item_user_state",
                column: "feed_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_feed_item_user_state_user_id_feed_item_id",
                table: "feed_item_user_state",
                columns: new[] { "user_id", "feed_item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_feed_item_user_state_user_id_is_archived",
                table: "feed_item_user_state",
                columns: new[] { "user_id", "is_archived" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "feed_item_user_state");
        }
    }
}
