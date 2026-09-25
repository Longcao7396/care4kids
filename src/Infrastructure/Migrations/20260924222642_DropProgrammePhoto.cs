using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiveAID.V2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropProgrammePhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "programme_photos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "programme_photos",
                columns: table => new
                {
                    photo_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    programme_id = table.Column<int>(type: "int", nullable: false),
                    caption = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    photo_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    uploaded_by = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_programme_photos", x => x.photo_id);
                    table.ForeignKey(
                        name: "fk_programme_photos_programmes_programme_id",
                        column: x => x.programme_id,
                        principalTable: "programmes",
                        principalColumn: "programme_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_programme_photos_programme_id",
                table: "programme_photos",
                column: "programme_id");
        }
    }
}
