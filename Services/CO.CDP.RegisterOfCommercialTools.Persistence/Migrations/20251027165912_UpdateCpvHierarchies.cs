using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.RegisterOfCommercialTools.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCpvHierarchies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var topLevelCodes = new[] {
                "31000000", "32000000", "33000000", "34000000", "35000000",
                "37000000", "38000000", "39000000", "51000000", "55000000",
                "63000000", "64000000", "65000000", "66000000", "71000000",
                "72000000", "73000000", "75000000", "76000000", "77000000",
                "79000000", "85000000", "92000000", "98000000"
            };

            foreach (var code in topLevelCodes)
            {
                migrationBuilder.Sql($@"
                    UPDATE commercial_tools_cpv_codes
                    SET parent_code = NULL, level = 1
                    WHERE code = '{code}';
                ");
            }

            foreach (var parentCode in topLevelCodes)
            {
                var prefix = parentCode.Substring(0, 2);
                migrationBuilder.Sql($@"
                    UPDATE commercial_tools_cpv_codes
                    SET parent_code = '{parentCode}', level = 2
                    WHERE code LIKE '{prefix}%'
                    AND code != '{parentCode}'
                    AND LENGTH(code) = 8
                    AND SUBSTRING(code, 3, 6) != '000000';
                ");
            }

            foreach (var parentCode in topLevelCodes)
            {
                var prefix = parentCode.Substring(0, 2);
                migrationBuilder.Sql($@"
                    UPDATE commercial_tools_cpv_codes c
                    SET level = 3
                    WHERE code LIKE '{prefix}%'
                    AND LENGTH(code) = 8
                    AND SUBSTRING(code, 4, 5) != '00000'
                    AND EXISTS (
                        SELECT 1 FROM commercial_tools_cpv_codes p
                        WHERE p.code = SUBSTRING(c.code, 1, 2) || SUBSTRING(c.code, 3, 1) || '00000'
                        AND p.level = 2
                        AND p.parent_code = '{parentCode}'
                    );
                ");

                migrationBuilder.Sql($@"
                    UPDATE commercial_tools_cpv_codes c
                    SET parent_code = SUBSTRING(c.code, 1, 2) || SUBSTRING(c.code, 3, 1) || '00000'
                    WHERE code LIKE '{prefix}%'
                    AND LENGTH(code) = 8
                    AND SUBSTRING(code, 4, 5) != '00000'
                    AND level = 3;
                ");
            }

            foreach (var parentCode in topLevelCodes)
            {
                var prefix = parentCode.Substring(0, 2);
                migrationBuilder.Sql($@"
                    UPDATE commercial_tools_cpv_codes c
                    SET level = 4
                    WHERE code LIKE '{prefix}%'
                    AND LENGTH(code) = 8
                    AND SUBSTRING(code, 5, 4) != '0000'
                    AND EXISTS (
                        SELECT 1 FROM commercial_tools_cpv_codes p
                        WHERE p.code = SUBSTRING(c.code, 1, 2) || SUBSTRING(c.code, 3, 2) || '0000'
                        AND p.level = 3
                    );
                ");

                migrationBuilder.Sql($@"
                    UPDATE commercial_tools_cpv_codes c
                    SET parent_code = SUBSTRING(c.code, 1, 2) || SUBSTRING(c.code, 3, 2) || '0000'
                    WHERE code LIKE '{prefix}%'
                    AND LENGTH(code) = 8
                    AND SUBSTRING(code, 5, 4) != '0000'
                    AND level = 4;
                ");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var topLevelCodes = new[] {
                "31000000", "32000000", "33000000", "34000000", "35000000",
                "37000000", "38000000", "39000000", "51000000", "55000000",
                "63000000", "64000000", "65000000", "66000000", "71000000",
                "72000000", "73000000", "75000000", "76000000", "77000000",
                "79000000", "85000000", "92000000", "98000000"
            };

            foreach (var code in topLevelCodes)
            {
                migrationBuilder.Sql($@"
                    UPDATE commercial_tools_cpv_codes
                    SET parent_code = '30000000', level = 2
                    WHERE code = '{code}';
                ");
            }

            foreach (var parentCode in topLevelCodes)
            {
                var prefix = parentCode.Substring(0, 2);
                migrationBuilder.Sql($@"
                    UPDATE commercial_tools_cpv_codes
                    SET parent_code = '{parentCode}', level = 3
                    WHERE code LIKE '{prefix}%'
                    AND code != '{parentCode}'
                    AND LENGTH(code) = 8
                    AND SUBSTRING(code, 3, 6) != '000000';
                ");
            }

            foreach (var parentCode in topLevelCodes)
            {
                var prefix = parentCode.Substring(0, 2);
                migrationBuilder.Sql($@"
                    UPDATE commercial_tools_cpv_codes c
                    SET level = 4,
                        parent_code = SUBSTRING(c.code, 1, 2) || SUBSTRING(c.code, 3, 1) || '00000'
                    WHERE code LIKE '{prefix}%'
                    AND LENGTH(code) = 8
                    AND SUBSTRING(code, 4, 5) != '00000';
                ");
            }
        }
    }
}
