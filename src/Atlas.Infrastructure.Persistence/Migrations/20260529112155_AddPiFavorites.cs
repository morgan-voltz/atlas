using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPiFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "patent_favorites",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    publication_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    title_snapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    added_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patent_favorites", x => x.id);
                    table.ForeignKey(
                        name: "FK_patent_favorites_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trademark_favorites",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deposit_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name_snapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    added_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trademark_favorites", x => x.id);
                    table.ForeignKey(
                        name: "FK_trademark_favorites_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_patent_favorites_user_id_publication_number",
                table: "patent_favorites",
                columns: new[] { "user_id", "publication_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trademark_favorites_user_id_deposit_number",
                table: "trademark_favorites",
                columns: new[] { "user_id", "deposit_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "patent_favorites");

            migrationBuilder.DropTable(
                name: "trademark_favorites");
        }
    }
}
