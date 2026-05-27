using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedItemCluster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "cluster_id",
                table: "feed_item",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "feed_item_cluster",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    simhash = table.Column<long>(type: "bigint", nullable: false),
                    item_count = table.Column<int>(type: "integer", nullable: false),
                    first_published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feed_item_cluster", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_feed_item_cluster_id",
                table: "feed_item",
                column: "cluster_id");

            migrationBuilder.CreateIndex(
                name: "IX_feed_item_cluster_last_published_at",
                table: "feed_item_cluster",
                column: "last_published_at");

            migrationBuilder.AddForeignKey(
                name: "FK_feed_item_feed_item_cluster_cluster_id",
                table: "feed_item",
                column: "cluster_id",
                principalTable: "feed_item_cluster",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_feed_item_feed_item_cluster_cluster_id",
                table: "feed_item");

            migrationBuilder.DropTable(
                name: "feed_item_cluster");

            migrationBuilder.DropIndex(
                name: "IX_feed_item_cluster_id",
                table: "feed_item");

            migrationBuilder.DropColumn(
                name: "cluster_id",
                table: "feed_item");
        }
    }
}
