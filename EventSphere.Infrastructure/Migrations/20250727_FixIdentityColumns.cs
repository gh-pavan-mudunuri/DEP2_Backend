using Microsoft.EntityFrameworkCore.Migrations;

namespace backend.Migrations
{
    public partial class FixIdentityColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID('EventFaqs', 'U') IS NOT NULL
                BEGIN
                    ALTER TABLE EventFaqs DROP CONSTRAINT IF EXISTS PK_EventFaqs;
                    ALTER TABLE EventFaqs DROP COLUMN FaqId;
                    ALTER TABLE EventFaqs ADD FaqId INT IDENTITY(1,1) NOT NULL PRIMARY KEY;
                END

                IF OBJECT_ID('EventSpeakers', 'U') IS NOT NULL
                BEGIN
                    ALTER TABLE EventSpeakers DROP CONSTRAINT IF EXISTS PK_EventSpeakers;
                    ALTER TABLE EventSpeakers DROP COLUMN SpeakerId;
                    ALTER TABLE EventSpeakers ADD SpeakerId INT IDENTITY(1,1) NOT NULL PRIMARY KEY;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op for down migration
        }
    }
}
