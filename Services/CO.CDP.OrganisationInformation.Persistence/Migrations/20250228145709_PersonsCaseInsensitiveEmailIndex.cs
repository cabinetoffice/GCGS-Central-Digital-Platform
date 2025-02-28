using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PersonsCaseInsensitiveEmailIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    DROP INDEX IF EXISTS ix_persons_email;
                    CREATE UNIQUE INDEX ix_persons_email ON persons(LOWER(email));
                END $$;
             ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                DO $$
                BEGIN
                    DROP INDEX IF EXISTS ix_persons_email;
                    CREATE UNIQUE INDEX ix_persons_email ON persons(email);
                END $$;
             ");
        }
    }
}
