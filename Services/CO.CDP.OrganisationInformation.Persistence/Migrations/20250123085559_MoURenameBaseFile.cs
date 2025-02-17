using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoURenameBaseFile : Migration
    {
        /// <inheritdoc />
        private readonly Guid mouId = Guid.Parse("1170db62-9657-4661-9b30-041b3fe234c2");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                UPDATE mou
                SET file_path = 'version-1.pdf'
                WHERE guid = '{mouId}'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                UPDATE mou
                SET file_path = 'mou-pdf-template.pdf'
                WHERE guid = '{mouId}'
            ");
        }
    }
}
