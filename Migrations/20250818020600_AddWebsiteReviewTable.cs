using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace project.Migrations
{
    /// <inheritdoc />
    public partial class AddWebsiteReviewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventFeedback_Events_EventId",
                table: "EventFeedback");

            migrationBuilder.DropForeignKey(
                name: "FK_EventFeedback_Registrations_RegistrationId",
                table: "EventFeedback");

            migrationBuilder.DropForeignKey(
                name: "FK_EventFeedback_Users_UserId",
                table: "EventFeedback");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventFeedback",
                table: "EventFeedback");

            migrationBuilder.RenameTable(
                name: "EventFeedback",
                newName: "EventFeedbacks");

            migrationBuilder.RenameIndex(
                name: "IX_EventFeedback_UserId",
                table: "EventFeedbacks",
                newName: "IX_EventFeedbacks_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_EventFeedback_RegistrationId",
                table: "EventFeedbacks",
                newName: "IX_EventFeedbacks_RegistrationId");

            migrationBuilder.RenameIndex(
                name: "IX_EventFeedback_EventId",
                table: "EventFeedbacks",
                newName: "IX_EventFeedbacks_EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventFeedbacks",
                table: "EventFeedbacks",
                column: "FeedbackId");

            migrationBuilder.CreateTable(
                name: "WebsiteReviews",
                columns: table => new
                {
                    ReviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteReviews", x => x.ReviewId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_EventFeedbacks_Events_EventId",
                table: "EventFeedbacks",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventFeedbacks_Registrations_RegistrationId",
                table: "EventFeedbacks",
                column: "RegistrationId",
                principalTable: "Registrations",
                principalColumn: "RegistrationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventFeedbacks_Users_UserId",
                table: "EventFeedbacks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventFeedbacks_Events_EventId",
                table: "EventFeedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_EventFeedbacks_Registrations_RegistrationId",
                table: "EventFeedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_EventFeedbacks_Users_UserId",
                table: "EventFeedbacks");

            migrationBuilder.DropTable(
                name: "WebsiteReviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventFeedbacks",
                table: "EventFeedbacks");

            migrationBuilder.RenameTable(
                name: "EventFeedbacks",
                newName: "EventFeedback");

            migrationBuilder.RenameIndex(
                name: "IX_EventFeedbacks_UserId",
                table: "EventFeedback",
                newName: "IX_EventFeedback_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_EventFeedbacks_RegistrationId",
                table: "EventFeedback",
                newName: "IX_EventFeedback_RegistrationId");

            migrationBuilder.RenameIndex(
                name: "IX_EventFeedbacks_EventId",
                table: "EventFeedback",
                newName: "IX_EventFeedback_EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventFeedback",
                table: "EventFeedback",
                column: "FeedbackId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventFeedback_Events_EventId",
                table: "EventFeedback",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventFeedback_Registrations_RegistrationId",
                table: "EventFeedback",
                column: "RegistrationId",
                principalTable: "Registrations",
                principalColumn: "RegistrationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventFeedback_Users_UserId",
                table: "EventFeedback",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
