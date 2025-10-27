using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.RegisterOfCommercialTools.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUkCountryCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE commercial_tools_nuts_codes DROP CONSTRAINT IF EXISTS fk_commercial_tools_nuts_codes_commercial_tools_nuts_codes_par;");

            migrationBuilder.Sql(@"
                DELETE FROM commercial_tools_nuts_codes WHERE code IN ('SCT', 'WLS', 'NIR');
                UPDATE commercial_tools_nuts_codes SET parent_code = 'ENG', level = level + 1 WHERE code IN ('UKC', 'UKD', 'UKE', 'UKF', 'UKG', 'UKH', 'UKI', 'UKJ', 'UKK');
            ");

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

            migrationBuilder.Sql(@"
                UPDATE commercial_tools_nuts_codes SET parent_code = 'GB', level = level - 1 WHERE code IN ('UKC', 'UKD', 'UKE', 'UKF', 'UKG', 'UKH', 'UKI', 'UKJ', 'UKK');
                INSERT INTO commercial_tools_nuts_codes (code, description_en, description_cy, parent_code, level, is_active, is_selectable, created_on, updated_on)
                VALUES
                    ('SCT', 'Scotland', 'Yr Alban', 'GB', 2, true, true, NOW(), NOW()),
                    ('WLS', 'Wales', 'Cymru', 'GB', 2, true, true, NOW(), NOW()),
                    ('NIR', 'Northern Ireland', 'Gogledd Iwerddon', 'GB', 2, true, true, NOW(), NOW());
            ");

            migrationBuilder.Sql(
                @"ALTER TABLE commercial_tools_nuts_codes
                  ADD CONSTRAINT fk_commercial_tools_nuts_codes_commercial_tools_nuts_codes_par
                  FOREIGN KEY (parent_code)
                  REFERENCES commercial_tools_nuts_codes(code)
                  ON DELETE RESTRICT;");
        }
    }
}
