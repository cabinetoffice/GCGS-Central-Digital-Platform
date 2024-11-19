﻿// <auto-generated />
using System;
using CO.CDP.EntityVerification.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.EntityVerification.Migrations
{
    [DbContext(typeof(EntityVerificationContext))]
    [Migration("20241119163516_IdentifierRegistries")]
    partial class IdentifierRegistries
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("entity_verification")
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CO.CDP.EntityVerification.Persistence.Identifier", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_on")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("IdentifierId")
                        .HasColumnType("text")
                        .HasColumnName("identifier_id");

                    b.Property<string>("LegalName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("legal_name");

                    b.Property<int>("PponId")
                        .HasColumnType("integer")
                        .HasColumnName("ppon_id");

                    b.Property<string>("Scheme")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("scheme");

                    b.Property<DateTimeOffset>("UpdatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_on")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Uri")
                        .HasColumnType("text")
                        .HasColumnName("uri");

                    b.Property<DateTimeOffset?>("endsOn")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("ends_on");

                    b.Property<DateTimeOffset>("startsOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("starts_on")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("Id")
                        .HasName("pk_identifiers");

                    b.HasIndex("IdentifierId")
                        .HasDatabaseName("ix_identifiers_identifier_id");

                    b.HasIndex("PponId")
                        .HasDatabaseName("ix_identifiers_ppon_id");

                    b.HasIndex("Scheme")
                        .HasDatabaseName("ix_identifiers_scheme");

                    b.ToTable("identifiers", "entity_verification");
                });

            modelBuilder.Entity("CO.CDP.EntityVerification.Persistence.IdentifierRegistries", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("CountryCode")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("country_code");

                    b.Property<DateTimeOffset>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_on")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("RegisterName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("register_name");

                    b.Property<string>("Scheme")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("scheme");

                    b.Property<DateTimeOffset>("UpdatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_on")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("Id")
                        .HasName("pk_identifier_registries");

                    b.HasIndex("CountryCode")
                        .HasDatabaseName("ix_identifier_registries_country_code");

                    b.HasIndex("Scheme")
                        .HasDatabaseName("ix_identifier_registries_scheme");

                    b.ToTable("identifier_registries", "entity_verification");
                });

            modelBuilder.Entity("CO.CDP.EntityVerification.Persistence.Ppon", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_on")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("IdentifierId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("identifier_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<Guid>("OrganisationId")
                        .HasColumnType("uuid")
                        .HasColumnName("organisation_id");

                    b.Property<DateTimeOffset>("UpdatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_on")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<DateTimeOffset?>("endsOn")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("ends_on");

                    b.Property<DateTimeOffset>("startsOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("starts_on")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("Id")
                        .HasName("pk_ppons");

                    b.HasIndex("IdentifierId")
                        .IsUnique()
                        .HasDatabaseName("ix_ppons_identifier_id");

                    b.HasIndex("Name")
                        .HasDatabaseName("ix_ppons_name");

                    b.HasIndex("OrganisationId")
                        .HasDatabaseName("ix_ppons_organisation_id");

                    b.ToTable("ppons", "entity_verification");
                });

            modelBuilder.Entity("CO.CDP.MQ.Outbox.OutboxMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_on")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("message");

                    b.Property<bool>("Published")
                        .HasColumnType("boolean")
                        .HasColumnName("published");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("type");

                    b.Property<DateTimeOffset>("UpdatedOn")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_on");

                    b.HasKey("Id")
                        .HasName("pk_outbox_messages");

                    b.HasIndex("CreatedOn")
                        .HasDatabaseName("ix_outbox_messages_created_on");

                    b.HasIndex("Published")
                        .HasDatabaseName("ix_outbox_messages_published");

                    b.ToTable("outbox_messages", "entity_verification");
                });

            modelBuilder.Entity("CO.CDP.EntityVerification.Persistence.Identifier", b =>
                {
                    b.HasOne("CO.CDP.EntityVerification.Persistence.Ppon", null)
                        .WithMany("Identifiers")
                        .HasForeignKey("PponId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_identifiers_ppons_ppon_id");
                });

            modelBuilder.Entity("CO.CDP.EntityVerification.Persistence.Ppon", b =>
                {
                    b.Navigation("Identifiers");
                });
#pragma warning restore 612, 618
        }
    }
}