using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.RegisterOfCommercialTools.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCountryCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE commercial_tools_nuts_codes DROP CONSTRAINT IF EXISTS fk_commercial_tools_nuts_codes_commercial_tools_nuts_codes_par;");

            migrationBuilder.Sql(
                "UPDATE commercial_tools_nuts_codes SET code = 'GB' WHERE code = 'UK';");

            migrationBuilder.Sql(
                "UPDATE commercial_tools_nuts_codes SET parent_code = 'GB' WHERE parent_code = 'UK';");

            migrationBuilder.Sql(
                @"ALTER TABLE commercial_tools_nuts_codes
                  ADD CONSTRAINT fk_commercial_tools_nuts_codes_commercial_tools_nuts_codes_par
                  FOREIGN KEY (parent_code)
                  REFERENCES commercial_tools_nuts_codes(code)
                  ON DELETE RESTRICT;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE commercial_tools_nuts_codes DROP CONSTRAINT IF EXISTS fk_commercial_tools_nuts_codes_commercial_tools_nuts_codes_par;");

            migrationBuilder.Sql(
                "UPDATE commercial_tools_nuts_codes SET code = 'UK' WHERE code = 'GB';");

            migrationBuilder.Sql(
                "UPDATE commercial_tools_nuts_codes SET parent_code = 'UK' WHERE parent_code = 'GB';");

            migrationBuilder.Sql(
                @"ALTER TABLE commercial_tools_nuts_codes
                  ADD CONSTRAINT fk_commercial_tools_nuts_codes_commercial_tools_nuts_codes_par
                  FOREIGN KEY (parent_code)
                  REFERENCES commercial_tools_nuts_codes(code)
                  ON DELETE RESTRICT;");
        }
    }
}
