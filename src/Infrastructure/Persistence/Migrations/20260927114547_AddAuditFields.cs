using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiveAID.V2.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "team_members",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "programmes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "programme_registrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "programme_registrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "organizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "organizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "invitations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "invitations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "gallery",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "gallery",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "faqs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "email_logs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "email_logs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "donations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "donations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "conversations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "conversations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "conversation_messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "conversation_messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "contact_messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "contact_messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "cms_pages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "causes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "causes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "careers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "career_applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "career_applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "campaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "campaign_reports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "campaign_reports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "campaign_registrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "campaign_registrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                table: "achievements",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by",
                table: "users");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "users");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "team_members");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "programmes");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "programme_registrations");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "programme_registrations");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "invitations");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "invitations");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "gallery");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "gallery");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "faqs");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "email_logs");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "email_logs");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "donations");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "donations");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "conversation_messages");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "conversation_messages");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "contact_messages");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "contact_messages");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "cms_pages");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "causes");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "causes");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "careers");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "career_applications");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "career_applications");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "campaign_reports");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "campaign_reports");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "campaign_registrations");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "campaign_registrations");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "achievements");
        }
    }
}
