using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Collections.Generic;
using static CO.CDP.OrganisationInformation.Persistence.Organisation;
using static System.Runtime.InteropServices.JavaScript.JSType;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVatPrimaryIdentifiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                CREATE TEMPORARY TABLE temp_identifiers (
                   identifier_id int,
                   organisation_id int
                );

                insert into temp_identifiers
                SELECT id AS identifier_id, organisation_id
                FROM public.identifiers
                WHERE scheme = 'VAT' AND ""primary"" = true;

                UPDATE public.identifiers
                SET ""primary"" = false
                FROM temp_identifiers t
                WHERE t.organisation_id = public.identifiers.organisation_id
                  AND public.identifiers.id = t.identifier_id;

                CREATE TEMPORARY TABLE identifiers_limited (
                   identifier_id int,
                   rn int,
                   scheme text,
                   organisation_id int
                );

                insert into identifiers_limited
                (identifier_id, rn, scheme, organisation_id)
                SELECT i.id as identifier_id,
	                 ROW_NUMBER() OVER (PARTITION BY i.organisation_id ORDER BY id) AS rn,
	                 scheme,
	                 i.organisation_id
                FROM public.identifiers i
                INNER JOIN temp_identifiers ti on ti.organisation_id = i.organisation_id
                WHERE i.scheme <> 'VAT';

                UPDATE public.identifiers
                SET ""primary"" = true
                FROM identifiers_limited il 
                WHERE il.organisation_id = public.identifiers.organisation_id
  	                AND public.identifiers.id = il.identifier_id
	                AND rn = 1;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not possble as the VAT primary data will have been lost.
        }
    }
}
