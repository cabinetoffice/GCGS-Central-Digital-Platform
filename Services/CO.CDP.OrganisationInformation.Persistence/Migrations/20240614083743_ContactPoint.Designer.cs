﻿// <auto-generated />
using System;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    [DbContext(typeof(OrganisationInformationContext))]
    [Migration("20240614083743_ContactPoint")]
    partial class ContactPoint
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CO.CDP.OrganisationInformation.Persistence.Address", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("CountryName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Locality")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PostalCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Region")
                        .HasColumnType("text");

                    b.Property<string>("StreetAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("StreetAddress2")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("UpdatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("Id");

                    b.ToTable("Address");
                });

            modelBuilder.Entity("CO.CDP.OrganisationInformation.Persistence.Organisation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<Guid>("Guid")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int[]>("Roles")
                        .IsRequired()
                        .HasColumnType("integer[]");

                    b.Property<int>("TenantId")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("UpdatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("Id");

                    b.HasIndex("Guid")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("TenantId");

                    b.ToTable("Organisations");
                });

            modelBuilder.Entity("CO.CDP.OrganisationInformation.Persistence.OrganisationPerson", b =>
                {
                    b.Property<int>("OrganisationId")
                        .HasColumnType("integer");

                    b.Property<int>("PersonId")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Scopes")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<DateTimeOffset>("UpdatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("OrganisationId", "PersonId");

                    b.HasIndex("PersonId");

                    b.ToTable("OrganisationPerson");
                });

            modelBuilder.Entity("CO.CDP.OrganisationInformation.Persistence.Person", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("Guid")
                        .HasColumnType("uuid");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("UpdatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("UserUrn")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Guid")
                        .IsUnique();

                    b.ToTable("Persons");
                });

            modelBuilder.Entity("CO.CDP.OrganisationInformation.Persistence.Tenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<Guid>("Guid")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("UpdatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("Id");

                    b.HasIndex("Guid")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("CO.CDP.OrganisationInformation.Persistence.TenantPerson", b =>
                {
                    b.Property<int>("PersonId")
                        .HasColumnType("integer");

                    b.Property<int>("TenantId")
                        .HasColumnType("integer");

                    b.HasKey("PersonId", "TenantId");

                    b.HasIndex("TenantId");

                    b.ToTable("TenantPerson");
                });

            modelBuilder.Entity("CO.CDP.OrganisationInformation.Persistence.Organisation", b =>
                {
                    b.HasOne("CO.CDP.OrganisationInformation.Persistence.Tenant", "Tenant")
                        .WithMany("Organisations")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("CO.CDP.OrganisationInformation.Persistence.Organisation+BuyerInformation", "BuyerInfo", b1 =>
                        {
                            b1.Property<int>("OrganisationId")
                                .HasColumnType("integer");

                            b1.Property<string>("BuyerType")
                                .HasColumnType("text");

                            b1.Property<DateTimeOffset>("CreatedOn")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("timestamp with time zone")
                                .HasDefaultValueSql("CURRENT_TIMESTAMP");

                            b1.Property<int[]>("DevolvedRegulations")
                                .IsRequired()
                                .HasColumnType("integer[]");

                            b1.Property<DateTimeOffset>("UpdatedOn")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("timestamp with time zone")
                                .HasDefaultValueSql("CURRENT_TIMESTAMP");

                            b1.HasKey("OrganisationId");

                            b1.ToTable("BuyerInformation", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("OrganisationId");
                        });

                    b.OwnsMany("CO.CDP.OrganisationInformation.Persistence.Organisation+ContactPoint", "ContactPoints", b1 =>
                        {
                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b1.Property<int>("Id"));

                            b1.Property<DateTimeOffset>("CreatedOn")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("timestamp with time zone")
                                .HasDefaultValueSql("CURRENT_TIMESTAMP");

                            b1.Property<string>("Email")
                                .HasColumnType("text");

                            b1.Property<string>("Name")
                                .HasColumnType("text");

                            b1.Property<int>("OrganisationId")
                                .HasColumnType("integer");

                            b1.Property<string>("Telephone")
                                .HasColumnType("text");

                            b1.Property<DateTimeOffset>("UpdatedOn")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("timestamp with time zone")
                                .HasDefaultValueSql("CURRENT_TIMESTAMP");

                            b1.Property<string>("Url")
                                .HasColumnType("text");

                            b1.HasKey("Id");

                            b1.HasIndex("OrganisationId");

                            b1.ToTable("ContactPoint");

                            b1.WithOwner()
                                .HasForeignKey("OrganisationId");
                        });

                    b.OwnsMany("CO.CDP.OrganisationInformation.Persistence.Organisation+Identifier", "Identifiers", b1 =>
                        {
                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b1.Property<int>("Id"));

                            b1.Property<DateTimeOffset>("CreatedOn")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("timestamp with time zone")
                                .HasDefaultValueSql("CURRENT_TIMESTAMP");

                            b1.Property<string>("IdentifierId")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("LegalName")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<int>("OrganisationId")
                                .HasColumnType("integer");

                            b1.Property<bool>("Primary")
                                .HasColumnType("boolean");

                            b1.Property<string>("Scheme")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<DateTimeOffset>("UpdatedOn")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("timestamp with time zone")
                                .HasDefaultValueSql("CURRENT_TIMESTAMP");

                            b1.Property<string>("Uri")
                                .HasColumnType("text");

                            b1.HasKey("Id");

                            b1.HasIndex("OrganisationId");

                            b1.ToTable("Identifier");

                            b1.WithOwner()
                                .HasForeignKey("OrganisationId");
                        });

                    b.OwnsMany("CO.CDP.OrganisationInformation.Persistence.Organisation+OrganisationAddress", "Addresses", b1 =>
                        {
                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b1.Property<int>("Id"));

                            b1.Property<int>("AddressId")
                                .HasColumnType("integer");

                            b1.Property<int>("OrganisationId")
                                .HasColumnType("integer");

                            b1.Property<int>("Type")
                                .HasColumnType("integer");

                            b1.HasKey("Id");

                            b1.HasIndex("AddressId");

                            b1.HasIndex("OrganisationId");

                            b1.ToTable("OrganisationAddress");

                            b1.HasOne("CO.CDP.OrganisationInformation.Persistence.Address", "Address")
                                .WithMany()
                                .HasForeignKey("AddressId")
                                .OnDelete(DeleteBehavior.Cascade)
                                .IsRequired();

                            b1.WithOwner()
                                .HasForeignKey("OrganisationId");

                            b1.Navigation("Address");
                        });

                    b.OwnsOne("CO.CDP.OrganisationInformation.Persistence.Organisation+SupplierInformation", "SupplierInfo", b1 =>
                        {
                            b1.Property<int>("OrganisationId")
                                .HasColumnType("integer");

                            b1.Property<bool>("CompletedEmailAddress")
                                .HasColumnType("boolean");

                            b1.Property<bool>("CompletedLegalForm")
                                .HasColumnType("boolean");

                            b1.Property<bool>("CompletedOperationType")
                                .HasColumnType("boolean");

                            b1.Property<bool>("CompletedPostalAddress")
                                .HasColumnType("boolean");

                            b1.Property<bool>("CompletedQualification")
                                .HasColumnType("boolean");

                            b1.Property<bool>("CompletedRegAddress")
                                .HasColumnType("boolean");

                            b1.Property<bool>("CompletedTradeAssurance")
                                .HasColumnType("boolean");

                            b1.Property<bool>("CompletedVat")
                                .HasColumnType("boolean");

                            b1.Property<bool>("CompletedWebsiteAddress")
                                .HasColumnType("boolean");

                            b1.Property<DateTimeOffset>("CreatedOn")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("timestamp with time zone")
                                .HasDefaultValueSql("CURRENT_TIMESTAMP");

                            b1.Property<int[]>("OperationTypes")
                                .IsRequired()
                                .HasColumnType("integer[]");

                            b1.Property<int?>("SupplierType")
                                .HasColumnType("integer");

                            b1.Property<DateTimeOffset>("UpdatedOn")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("timestamp with time zone")
                                .HasDefaultValueSql("CURRENT_TIMESTAMP");

                            b1.HasKey("OrganisationId");

                            b1.ToTable("SupplierInformation", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("OrganisationId");

                            b1.OwnsOne("CO.CDP.OrganisationInformation.Persistence.Organisation+LegalForm", "LegalForm", b2 =>
                                {
                                    b2.Property<int>("SupplierInformationOrganisationId")
                                        .HasColumnType("integer");

                                    b2.Property<DateTimeOffset>("CreatedOn")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("timestamp with time zone")
                                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                                    b2.Property<string>("LawRegistered")
                                        .IsRequired()
                                        .HasColumnType("text");

                                    b2.Property<string>("RegisteredLegalForm")
                                        .IsRequired()
                                        .HasColumnType("text");

                                    b2.Property<string>("RegisteredUnderAct2006")
                                        .IsRequired()
                                        .HasColumnType("text");

                                    b2.Property<DateTimeOffset>("RegistrationDate")
                                        .HasColumnType("timestamp with time zone");

                                    b2.Property<DateTimeOffset>("UpdatedOn")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("timestamp with time zone")
                                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                                    b2.HasKey("SupplierInformationOrganisationId");

                                    b2.ToTable("LegalForm", (string)null);

                                    b2.WithOwner()
                                        .HasForeignKey("SupplierInformationOrganisationId");
                                });

                            b1.OwnsMany("CO.CDP.OrganisationInformation.Persistence.Organisation+Qualification", "Qualifications", b2 =>
                                {
                                    b2.Property<int>("Id")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("integer");

                                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b2.Property<int>("Id"));

                                    b2.Property<string>("AwardedByPersonOrBodyName")
                                        .HasColumnType("text");

                                    b2.Property<DateTimeOffset>("CreatedOn")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("timestamp with time zone")
                                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                                    b2.Property<DateTimeOffset>("DateAwarded")
                                        .HasColumnType("timestamp with time zone");

                                    b2.Property<string>("Name")
                                        .HasColumnType("text");

                                    b2.Property<int>("SupplierInformationOrganisationId")
                                        .HasColumnType("integer");

                                    b2.Property<DateTimeOffset>("UpdatedOn")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("timestamp with time zone")
                                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                                    b2.HasKey("Id");

                                    b2.HasIndex("SupplierInformationOrganisationId");

                                    b2.ToTable("Qualification", (string)null);

                                    b2.WithOwner()
                                        .HasForeignKey("SupplierInformationOrganisationId");
                                });

                            b1.OwnsMany("CO.CDP.OrganisationInformation.Persistence.Organisation+TradeAssurance", "TradeAssurances", b2 =>
                                {
                                    b2.Property<int>("Id")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("integer");

                                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b2.Property<int>("Id"));

                                    b2.Property<string>("AwardedByPersonOrBodyName")
                                        .HasColumnType("text");

                                    b2.Property<DateTimeOffset>("CreatedOn")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("timestamp with time zone")
                                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                                    b2.Property<DateTimeOffset>("DateAwarded")
                                        .HasColumnType("timestamp with time zone");

                                    b2.Property<string>("ReferenceNumber")
                                        .HasColumnType("text");

                                    b2.Property<int>("SupplierInformationOrganisationId")
                                        .HasColumnType("integer");

                                    b2.Property<DateTimeOffset>("UpdatedOn")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("timestamp with time zone")
                                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                                    b2.HasKey("Id");

                                    b2.HasIndex("SupplierInformationOrganisationId");

                                    b2.ToTable("TradeAssurance", (string)null);

                                    b2.WithOwner()
                                        .HasForeignKey("SupplierInformationOrganisationId");
                                });

                            b1.Navigation("LegalForm");

                            b1.Navigation("Qualifications");

                            b1.Navigation("TradeAssurances");
                        });

                    b.Navigation("Addresses");

                    b.Navigation("BuyerInfo");

                    b.Navigation("ContactPoints");

                    b.Navigation("Identifiers");

                    b.Navigation("SupplierInfo");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("CO.CDP.OrganisationInformation.Persistence.OrganisationPerson", b =>
                {
                    b.HasOne("CO.CDP.OrganisationInformation.Persistence.Organisation", "Organisation")
                        .WithMany("OrganisationPersons")
                        .HasForeignKey("OrganisationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CO.CDP.OrganisationInformation.Persistence.Person", "Person")
                        .WithMany("PersonOrganisations")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organisation");

                    b.Navigation("Person");
                });

            modelBuilder.Entity("CO.CDP.OrganisationInformation.Persistence.TenantPerson", b =>
                {
                    b.HasOne("CO.CDP.OrganisationInformation.Persistence.Person", null)
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CO.CDP.OrganisationInformation.Persistence.Tenant", null)
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CO.CDP.OrganisationInformation.Persistence.Organisation", b =>
                {
                    b.Navigation("OrganisationPersons");
                });

            modelBuilder.Entity("CO.CDP.OrganisationInformation.Persistence.Person", b =>
                {
                    b.Navigation("PersonOrganisations");
                });

            modelBuilder.Entity("CO.CDP.OrganisationInformation.Persistence.Tenant", b =>
                {
                    b.Navigation("Organisations");
                });
#pragma warning restore 612, 618
        }
    }
}
