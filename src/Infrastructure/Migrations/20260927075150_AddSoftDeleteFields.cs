using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiveAID.V2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "team_members",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "team_members",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "programmes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "programmes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "programme_registrations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "programme_registrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "organizations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "organizations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "invitations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "invitations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "gallery",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "gallery",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "faqs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "faqs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "email_logs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "email_logs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "donations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "donations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "conversations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "conversations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "conversation_messages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "conversation_messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "contact_messages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "contact_messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "cms_pages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "cms_pages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "causes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "causes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "careers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "careers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "career_applications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "career_applications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "campaigns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "campaigns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "campaign_reports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "campaign_reports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "campaign_registrations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "campaign_registrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "achievements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "achievements",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "users");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "team_members");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "team_members");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "programmes");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "programmes");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "programme_registrations");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "programme_registrations");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "invitations");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "invitations");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "gallery");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "gallery");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "faqs");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "faqs");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "email_logs");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "email_logs");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "donations");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "donations");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "conversation_messages");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "conversation_messages");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "contact_messages");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "contact_messages");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "cms_pages");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "cms_pages");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "causes");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "causes");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "careers");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "careers");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "career_applications");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "career_applications");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "campaign_reports");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "campaign_reports");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "campaign_registrations");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "campaign_registrations");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "achievements");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "achievements");
        }
    }
}
