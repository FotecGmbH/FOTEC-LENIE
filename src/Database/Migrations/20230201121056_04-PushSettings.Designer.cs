﻿// <auto-generated />
using System;
using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Database.Migrations
{
    [DbContext(typeof(Db))]
    [Migration("20230201121056_04-PushSettings")]
    partial class _04PushSettings
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Database.Tables.TableAccessToken", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<DateTime>("GuiltyUntilUtc")
                        .HasColumnType("datetime2");

                    b.Property<long?>("TblOrganizationId")
                        .HasColumnType("bigint");

                    b.Property<long?>("TblUserId")
                        .HasColumnType("bigint");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("TblOrganizationId");

                    b.HasIndex("TblUserId");

                    b.ToTable("AccessToken");
                });

            modelBuilder.Entity("Database.Tables.TableChat", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<string>("ChatName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<bool>("PublicChat")
                        .HasColumnType("bit");

                    b.Property<long?>("TblIdeaId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("TblIdeaId");

                    b.ToTable("Chat");
                });

            modelBuilder.Entity("Database.Tables.TableChatEntry", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<DateTime>("EntryDateTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<long?>("TblChatId")
                        .HasColumnType("bigint");

                    b.Property<long?>("TblUserWriterId")
                        .HasColumnType("bigint");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("TblChatId");

                    b.HasIndex("TblUserWriterId");

                    b.ToTable("ChatEntry");
                });

            modelBuilder.Entity("Database.Tables.TableChatUsers", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<long>("TblChatId")
                        .HasColumnType("bigint");

                    b.Property<long>("TblUserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("TblChatId");

                    b.HasIndex("TblUserId");

                    b.ToTable("ChatUsers");
                });

            modelBuilder.Entity("Database.Tables.TableDevice", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<string>("AppVersion")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CurrentAppType")
                        .HasColumnType("int");

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("DeviceHardwareId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DeviceIdiom")
                        .HasColumnType("int");

                    b.Property<string>("DeviceName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DeviceToken")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DeviceType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsAppRunning")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastDateTimeUtcOnline")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LastLogin")
                        .HasColumnType("datetime2");

                    b.Property<string>("Manufacturer")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OperatingSystemVersion")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Plattform")
                        .HasColumnType("int");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScreenResolution")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("TblUserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("TblUserId");

                    b.ToTable("Device");
                });

            modelBuilder.Entity("Database.Tables.TableFile", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<string>("AdditionalData")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BlobName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Bytes")
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PublicLink")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("File");
                });

            modelBuilder.Entity("Database.Tables.TableFutureWish", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Link")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("FutureWish");
                });

            modelBuilder.Entity("Database.Tables.TableFutureWishLike", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<long>("TblFutureWishId")
                        .HasColumnType("bigint");

                    b.Property<long>("TblUserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("TblFutureWishId");

                    b.HasIndex("TblUserId");

                    b.ToTable("FutureWishLike");
                });

            modelBuilder.Entity("Database.Tables.TableIdea", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("From")
                        .HasColumnType("datetime2");

                    b.Property<string>("LocationAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("LocationLat")
                        .HasColumnType("float");

                    b.Property<double?>("LocationLon")
                        .HasColumnType("float");

                    b.Property<long?>("TblIdeaImageId")
                        .HasColumnType("bigint");

                    b.Property<long>("TblUserId")
                        .HasColumnType("bigint");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("To")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("TblIdeaImageId");

                    b.HasIndex("TblUserId");

                    b.ToTable("Idea");
                });

            modelBuilder.Entity("Database.Tables.TableIdeaHelper", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<DateTime?>("From")
                        .HasColumnType("datetime2");

                    b.Property<string>("Info")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("TblIdeaId")
                        .HasColumnType("bigint");

                    b.Property<long>("TblUserId")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("To")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("TblIdeaId");

                    b.HasIndex("TblUserId");

                    b.ToTable("IdeaHelper");
                });

            modelBuilder.Entity("Database.Tables.TableIdeaLike", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<long>("TblIdeaId")
                        .HasColumnType("bigint");

                    b.Property<long>("TblUserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("TblIdeaId");

                    b.HasIndex("TblUserId");

                    b.ToTable("IdeaLike");
                });

            modelBuilder.Entity("Database.Tables.TableIdeaNeed", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<long>("Amount")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Info")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("TblIdeaId")
                        .HasColumnType("bigint");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("TblIdeaId");

                    b.ToTable("IdeaNeed");
                });

            modelBuilder.Entity("Database.Tables.TableIdeaOrganization", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<long>("TblIdeaId")
                        .HasColumnType("bigint");

                    b.Property<long>("TblOrganizationId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("TblIdeaId");

                    b.HasIndex("TblOrganizationId");

                    b.ToTable("IdeaOrganization");
                });

            modelBuilder.Entity("Database.Tables.TableIdeaReport", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("TblIdeaId")
                        .HasColumnType("bigint");

                    b.Property<long>("TblUserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("TblIdeaId");

                    b.HasIndex("TblUserId");

                    b.ToTable("IdeaReport");
                });

            modelBuilder.Entity("Database.Tables.TableIdeaSupply", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<long>("Amount")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<long>("TblIdeaHelperId")
                        .HasColumnType("bigint");

                    b.Property<long>("TblIdeaNeedId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("TblIdeaHelperId");

                    b.HasIndex("TblIdeaNeedId");

                    b.ToTable("IdeaSupply");
                });

            modelBuilder.Entity("Database.Tables.TableOrganization", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("OrganizationType")
                        .HasColumnType("int");

                    b.Property<string>("PostalCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Organization");
                });

            modelBuilder.Entity("Database.Tables.TablePermission", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<bool>("MainOrganization")
                        .HasColumnType("bit");

                    b.Property<long>("TblOrganizationId")
                        .HasColumnType("bigint");

                    b.Property<long>("TblUserId")
                        .HasColumnType("bigint");

                    b.Property<int>("UserRight")
                        .HasColumnType("int");

                    b.Property<int>("UserRole")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TblOrganizationId");

                    b.HasIndex("TblUserId");

                    b.ToTable("Permission");
                });

            modelBuilder.Entity("Database.Tables.TableSetting", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int>("Key")
                        .HasColumnType("int");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Setting");
                });

            modelBuilder.Entity("Database.Tables.TableUser", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<string>("AgbVersion")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ConfirmationToken")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("DataVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("DefaultLanguage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("bit");

                    b.Property<string>("JwtToken")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Locked")
                        .HasColumnType("bit");

                    b.Property<bool>("LoginConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("LoginName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("NotificationMailChat")
                        .HasColumnType("bit");

                    b.Property<bool>("NotificationMailMeeting")
                        .HasColumnType("bit");

                    b.Property<bool>("NotificationPushChat")
                        .HasColumnType("bit");

                    b.Property<bool>("NotificationPushComment")
                        .HasColumnType("bit");

                    b.Property<bool>("NotificationPushIdea")
                        .HasColumnType("bit");

                    b.Property<bool>("NotificationPushLike")
                        .HasColumnType("bit");

                    b.Property<bool>("NotificationPushMeeting")
                        .HasColumnType("bit");

                    b.Property<bool>("NotificationPushReport")
                        .HasColumnType("bit");

                    b.Property<bool>("NotificationPushSupport")
                        .HasColumnType("bit");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PushTags")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Setting10MinPush")
                        .HasColumnType("bit");

                    b.Property<long?>("TblUserImageId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("TblUserImageId");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Database.Tables.TableAccessToken", b =>
                {
                    b.HasOne("Database.Tables.TableOrganization", "TblOrganization")
                        .WithMany("TblAccessToken")
                        .HasForeignKey("TblOrganizationId");

                    b.HasOne("Database.Tables.TableUser", "TblUser")
                        .WithMany("TblAccessToken")
                        .HasForeignKey("TblUserId");

                    b.Navigation("TblOrganization");

                    b.Navigation("TblUser");
                });

            modelBuilder.Entity("Database.Tables.TableChat", b =>
                {
                    b.HasOne("Database.Tables.TableIdea", "TblIdea")
                        .WithMany("TblChat")
                        .HasForeignKey("TblIdeaId");

                    b.Navigation("TblIdea");
                });

            modelBuilder.Entity("Database.Tables.TableChatEntry", b =>
                {
                    b.HasOne("Database.Tables.TableChat", "TblChat")
                        .WithMany("TblChatEntries")
                        .HasForeignKey("TblChatId");

                    b.HasOne("Database.Tables.TableUser", "TblUserWriter")
                        .WithMany()
                        .HasForeignKey("TblUserWriterId");

                    b.Navigation("TblChat");

                    b.Navigation("TblUserWriter");
                });

            modelBuilder.Entity("Database.Tables.TableChatUsers", b =>
                {
                    b.HasOne("Database.Tables.TableChat", "TblChat")
                        .WithMany("TblChatUsers")
                        .HasForeignKey("TblChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Tables.TableUser", "TblUser")
                        .WithMany("TblChatUsers")
                        .HasForeignKey("TblUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TblChat");

                    b.Navigation("TblUser");
                });

            modelBuilder.Entity("Database.Tables.TableDevice", b =>
                {
                    b.HasOne("Database.Tables.TableUser", "TblUser")
                        .WithMany("TblDevices")
                        .HasForeignKey("TblUserId");

                    b.Navigation("TblUser");
                });

            modelBuilder.Entity("Database.Tables.TableFutureWishLike", b =>
                {
                    b.HasOne("Database.Tables.TableFutureWish", "TblFutureWish")
                        .WithMany("TblFutureWishLikes")
                        .HasForeignKey("TblFutureWishId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Tables.TableUser", "TblUser")
                        .WithMany("TblFutureWishLikes")
                        .HasForeignKey("TblUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TblFutureWish");

                    b.Navigation("TblUser");
                });

            modelBuilder.Entity("Database.Tables.TableIdea", b =>
                {
                    b.HasOne("Database.Tables.TableFile", "TblIdeaImage")
                        .WithMany("TblIdeas")
                        .HasForeignKey("TblIdeaImageId");

                    b.HasOne("Database.Tables.TableUser", "TblUser")
                        .WithMany("TblIdeas")
                        .HasForeignKey("TblUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TblIdeaImage");

                    b.Navigation("TblUser");
                });

            modelBuilder.Entity("Database.Tables.TableIdeaHelper", b =>
                {
                    b.HasOne("Database.Tables.TableIdea", "TblIdea")
                        .WithMany("TblIdeaHelpers")
                        .HasForeignKey("TblIdeaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Tables.TableUser", "TblUser")
                        .WithMany("TblIdeaHelpers")
                        .HasForeignKey("TblUserId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("TblIdea");

                    b.Navigation("TblUser");
                });

            modelBuilder.Entity("Database.Tables.TableIdeaLike", b =>
                {
                    b.HasOne("Database.Tables.TableIdea", "TblIdea")
                        .WithMany("TblIdeaLikes")
                        .HasForeignKey("TblIdeaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Tables.TableUser", "TblUser")
                        .WithMany("TblIdeaLikes")
                        .HasForeignKey("TblUserId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("TblIdea");

                    b.Navigation("TblUser");
                });

            modelBuilder.Entity("Database.Tables.TableIdeaNeed", b =>
                {
                    b.HasOne("Database.Tables.TableIdea", "TblIdea")
                        .WithMany("TblIdeaNeeds")
                        .HasForeignKey("TblIdeaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TblIdea");
                });

            modelBuilder.Entity("Database.Tables.TableIdeaOrganization", b =>
                {
                    b.HasOne("Database.Tables.TableIdea", "TblIdea")
                        .WithMany("TblIdeaOrganizations")
                        .HasForeignKey("TblIdeaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Tables.TableOrganization", "TblOrganization")
                        .WithMany("TblIdeaOrganisations")
                        .HasForeignKey("TblOrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TblIdea");

                    b.Navigation("TblOrganization");
                });

            modelBuilder.Entity("Database.Tables.TableIdeaReport", b =>
                {
                    b.HasOne("Database.Tables.TableIdea", "TblIdea")
                        .WithMany("TblIdeaReports")
                        .HasForeignKey("TblIdeaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Tables.TableUser", "TblUser")
                        .WithMany("TblIdeaReports")
                        .HasForeignKey("TblUserId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("TblIdea");

                    b.Navigation("TblUser");
                });

            modelBuilder.Entity("Database.Tables.TableIdeaSupply", b =>
                {
                    b.HasOne("Database.Tables.TableIdeaHelper", "TblIdeaHelper")
                        .WithMany("TblIdeaSupplies")
                        .HasForeignKey("TblIdeaHelperId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Tables.TableIdeaNeed", "TblIdeaNeed")
                        .WithMany("TblIdeaSupplies")
                        .HasForeignKey("TblIdeaNeedId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("TblIdeaHelper");

                    b.Navigation("TblIdeaNeed");
                });

            modelBuilder.Entity("Database.Tables.TablePermission", b =>
                {
                    b.HasOne("Database.Tables.TableOrganization", "TblOrganization")
                        .WithMany("TblPermissions")
                        .HasForeignKey("TblOrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Database.Tables.TableUser", "TblUser")
                        .WithMany("TblPermissions")
                        .HasForeignKey("TblUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TblOrganization");

                    b.Navigation("TblUser");
                });

            modelBuilder.Entity("Database.Tables.TableUser", b =>
                {
                    b.HasOne("Database.Tables.TableFile", "TblUserImage")
                        .WithMany("TblUserImages")
                        .HasForeignKey("TblUserImageId");

                    b.Navigation("TblUserImage");
                });

            modelBuilder.Entity("Database.Tables.TableChat", b =>
                {
                    b.Navigation("TblChatEntries");

                    b.Navigation("TblChatUsers");
                });

            modelBuilder.Entity("Database.Tables.TableFile", b =>
                {
                    b.Navigation("TblIdeas");

                    b.Navigation("TblUserImages");
                });

            modelBuilder.Entity("Database.Tables.TableFutureWish", b =>
                {
                    b.Navigation("TblFutureWishLikes");
                });

            modelBuilder.Entity("Database.Tables.TableIdea", b =>
                {
                    b.Navigation("TblChat");

                    b.Navigation("TblIdeaHelpers");

                    b.Navigation("TblIdeaLikes");

                    b.Navigation("TblIdeaNeeds");

                    b.Navigation("TblIdeaOrganizations");

                    b.Navigation("TblIdeaReports");
                });

            modelBuilder.Entity("Database.Tables.TableIdeaHelper", b =>
                {
                    b.Navigation("TblIdeaSupplies");
                });

            modelBuilder.Entity("Database.Tables.TableIdeaNeed", b =>
                {
                    b.Navigation("TblIdeaSupplies");
                });

            modelBuilder.Entity("Database.Tables.TableOrganization", b =>
                {
                    b.Navigation("TblAccessToken");

                    b.Navigation("TblIdeaOrganisations");

                    b.Navigation("TblPermissions");
                });

            modelBuilder.Entity("Database.Tables.TableUser", b =>
                {
                    b.Navigation("TblAccessToken");

                    b.Navigation("TblChatUsers");

                    b.Navigation("TblDevices");

                    b.Navigation("TblFutureWishLikes");

                    b.Navigation("TblIdeaHelpers");

                    b.Navigation("TblIdeaLikes");

                    b.Navigation("TblIdeaReports");

                    b.Navigation("TblIdeas");

                    b.Navigation("TblPermissions");
                });
#pragma warning restore 612, 618
        }
    }
}
