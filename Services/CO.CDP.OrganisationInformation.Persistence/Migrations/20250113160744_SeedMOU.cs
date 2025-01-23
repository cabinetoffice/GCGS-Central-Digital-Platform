using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedMOU : Migration
    {
        private readonly Guid mouId = Guid.Parse("1170db62-9657-4661-9b30-041b3fe234c2");
        private readonly string path = @"\mou-pdfs\mou-pdf-template.pdf";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                INSERT INTO mou (guid, file_path)
                VALUES ('{mouId}', '{path}');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DELETE FROM mou WHERE guid = '{mouId}';
            ");

        }
    }
}
