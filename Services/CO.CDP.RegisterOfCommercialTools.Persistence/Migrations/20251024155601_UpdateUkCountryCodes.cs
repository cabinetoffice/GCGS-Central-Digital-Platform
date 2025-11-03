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
                UPDATE commercial_tools_nuts_codes
                SET parent_code = 'GB'
                WHERE parent_code IN ('SCT', 'WLS', 'NIR');
            ");

            migrationBuilder.Sql(@"
                UPDATE commercial_tools_nuts_codes
                SET parent_code = 'ENG'
                WHERE code IN ('UKC', 'UKD', 'UKE', 'UKF', 'UKG', 'UKH', 'UKI', 'UKJ', 'UKK')
                AND parent_code = 'GB';
            ");

            migrationBuilder.Sql(@"
                DELETE FROM commercial_tools_nuts_codes WHERE code IN ('SCT', 'WLS', 'NIR');
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
                INSERT INTO commercial_tools_nuts_codes (code, description_en, description_cy, parent_code, level, is_active, is_selectable, created_on, updated_on)
                VALUES
                    ('SCT', 'Scotland', 'Yr Alban', 'GB', 2, true, true, NOW(), NOW()),
                    ('WLS', 'Wales', 'Cymru', 'GB', 2, true, true, NOW(), NOW()),
                    ('NIR', 'Northern Ireland', 'Gogledd Iwerddon', 'GB', 2, true, true, NOW(), NOW())
                ON CONFLICT (code) DO NOTHING;
            ");

            migrationBuilder.Sql(@"
                UPDATE commercial_tools_nuts_codes
                SET parent_code = 'GB'
                WHERE code IN ('UKC', 'UKD', 'UKE', 'UKF', 'UKG', 'UKH', 'UKI', 'UKJ', 'UKK')
                AND parent_code = 'ENG';
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
