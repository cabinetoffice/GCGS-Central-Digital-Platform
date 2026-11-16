using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <summary>
    /// Stops storing API keys in plain text. Existing keys are converted in place to a SHA-256 hash
    /// (lowercase hex) plus a short hint, then the plaintext column is dropped.
    /// The hash must match <see cref="ApiKeyHasher.Hash"/>.
    /// </summary>
    [DbContext(typeof(OrganisationInformationContext))]
    [Migration("20261116184133_HashAuthenticationKeys")]
    public class HashAuthenticationKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "key_hash",
                table: "authentication_keys",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "key_hint",
                table: "authentication_keys",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE authentication_keys
                SET key_hash = encode(sha256(convert_to(key, 'UTF8')), 'hex'),
                    key_hint = right(key, 4);
            ");

            migrationBuilder.AlterColumn<string>(
                name: "key_hash",
                table: "authentication_keys",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_authentication_keys_key_hash",
                table: "authentication_keys",
                column: "key_hash",
                unique: true);

            migrationBuilder.DropColumn(
                name: "key",
                table: "authentication_keys");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // no-op Down: plaintext keys cannot be recovered from their hashes
        }
    }
}
