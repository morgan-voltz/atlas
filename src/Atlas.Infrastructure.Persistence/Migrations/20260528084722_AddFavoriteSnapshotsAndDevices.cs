using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoriteSnapshotsAndDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "company_favorite_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    siren = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    denomination = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    forme_juridique = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    naf_code = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    adresse_line = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    dirigeants_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    captured_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company_favorite_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_company_favorite_snapshots_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "device_registrations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    token = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    registered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_registrations", x => x.id);
                    table.ForeignKey(
                        name: "FK_device_registrations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_company_favorite_snapshots_user_id_siren",
                table: "company_favorite_snapshots",
                columns: new[] { "user_id", "siren" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_device_registrations_token",
                table: "device_registrations",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_device_registrations_user_id",
                table: "device_registrations",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_favorite_snapshots");

            migrationBuilder.DropTable(
                name: "device_registrations");
        }
    }
}
