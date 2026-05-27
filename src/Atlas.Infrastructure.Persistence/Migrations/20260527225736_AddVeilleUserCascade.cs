using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVeilleUserCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_feed_item_user_state_users_user_id",
                table: "feed_item_user_state",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_veille_pack_enrollment_users_user_id",
                table: "veille_pack_enrollment",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_veille_subscription_users_user_id",
                table: "veille_subscription",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_feed_item_user_state_users_user_id",
                table: "feed_item_user_state");

            migrationBuilder.DropForeignKey(
                name: "FK_veille_pack_enrollment_users_user_id",
                table: "veille_pack_enrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_veille_subscription_users_user_id",
                table: "veille_subscription");
        }
    }
}
