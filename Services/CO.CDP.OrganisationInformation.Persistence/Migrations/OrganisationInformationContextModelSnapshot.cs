﻿// <auto-generated />
using System;
using System.Collections.Generic;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    [DbContext(typeof(OrganisationInformationContext))]
    partial class OrganisationInformationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

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

                    b.Property<int>("TenantId")
                        .HasColumnType("integer");

                    b.Property<List<int>>("Types")
                        .IsRequired()
                        .HasColumnType("integer[]");

                    b.Property<DateTimeOffset>("UpdatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.ComplexProperty<Dictionary<string, object>>("Address", "CO.CDP.OrganisationInformation.Persistence.Organisation.Address#OrganisationAddress", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<string>("AddressLine1")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("AddressLine2")
                                .HasColumnType("text");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("Country")
                                .HasColumnType("text");

                            b1.Property<string>("PostCode")
                                .IsRequired()
                                .HasColumnType("text");
                        });

                    b.ComplexProperty<Dictionary<string, object>>("ContactPoint", "CO.CDP.OrganisationInformation.Persistence.Organisation.ContactPoint#OrganisationContactPoint", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<string>("Email")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("Name")
                                .HasColumnType("text");

                            b1.Property<string>("Telephone")
                                .HasColumnType("text");

                            b1.Property<string>("Url")
                                .HasColumnType("text");
                        });

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

                    b.OwnsMany("CO.CDP.OrganisationInformation.Persistence.Organisation+OrganisationIdentifier", "Identifiers", b1 =>
                        {
                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b1.Property<int>("Id"));

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

                            b1.Property<string>("Uri")
                                .HasColumnType("text");

                            b1.HasKey("Id");

                            b1.HasIndex("OrganisationId");

                            b1.ToTable("OrganisationIdentifier");

                            b1.WithOwner()
                                .HasForeignKey("OrganisationId");
                        });

                    b.Navigation("Identifiers");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("CO.CDP.OrganisationInformation.Persistence.OrganisationPerson", b =>
                {
                    b.HasOne("CO.CDP.OrganisationInformation.Persistence.Organisation", null)
                        .WithMany()
                        .HasForeignKey("OrganisationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CO.CDP.OrganisationInformation.Persistence.Person", null)
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
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

            modelBuilder.Entity("CO.CDP.OrganisationInformation.Persistence.Tenant", b =>
                {
                    b.Navigation("Organisations");
                });
#pragma warning restore 612, 618
        }
    }
}
