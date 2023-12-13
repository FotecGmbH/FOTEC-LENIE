using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1062 // Validate Parameter is non-null before using it
namespace Database.Migrations
{
    public partial class _00_Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Erzeugt via Create and fill up

            //migrationBuilder.CreateTable(
            //    name: "File",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        BlobName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PublicLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Bytes = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_File", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "FutureWish",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Link = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_FutureWish", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Organization",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        OrganizationType = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Organization", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Setting",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        Key = table.Column<int>(type: "int", nullable: false),
            //        Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Setting", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AccessToken",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        GuiltyUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        TblUserId = table.Column<long>(type: "bigint", nullable: true),
            //        TblOrganizationId = table.Column<long>(type: "bigint", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AccessToken", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AccessToken_Organization_TblOrganizationId",
            //            column: x => x.TblOrganizationId,
            //            principalTable: "Organization",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Chat",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        ChatName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PublicChat = table.Column<bool>(type: "bit", nullable: false),
            //        TblIdeaId = table.Column<long>(type: "bigint", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Chat", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "User",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        IsAdmin = table.Column<bool>(type: "bit", nullable: false),
            //        AgbVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        LoginName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        JwtToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Locked = table.Column<bool>(type: "bit", nullable: false),
            //        LoginConfirmed = table.Column<bool>(type: "bit", nullable: false),
            //        CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        DefaultLanguage = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ConfirmationToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PushTags = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Setting10MinPush = table.Column<bool>(type: "bit", nullable: false),
            //        NotificationPushMeeting = table.Column<bool>(type: "bit", nullable: false),
            //        NotificationPushChat = table.Column<bool>(type: "bit", nullable: false),
            //        NotificationPushComment = table.Column<bool>(type: "bit", nullable: false),
            //        NotificationPushLike = table.Column<bool>(type: "bit", nullable: false),
            //        NotificationMailMeeting = table.Column<bool>(type: "bit", nullable: false),
            //        NotificationMailChat = table.Column<bool>(type: "bit", nullable: false),
            //        TblUserImageId = table.Column<long>(type: "bigint", nullable: true),
            //        TableChatId = table.Column<long>(type: "bigint", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_User", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_User_Chat_TableChatId",
            //            column: x => x.TableChatId,
            //            principalTable: "Chat",
            //            principalColumn: "Id");
            //        table.ForeignKey(
            //            name: "FK_User_File_TblUserImageId",
            //            column: x => x.TblUserImageId,
            //            principalTable: "File",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ChatEntry",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        EntryDateTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        TblChatId = table.Column<long>(type: "bigint", nullable: true),
            //        TblUserWriterId = table.Column<long>(type: "bigint", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ChatEntry", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_ChatEntry_Chat_TblChatId",
            //            column: x => x.TblChatId,
            //            principalTable: "Chat",
            //            principalColumn: "Id");
            //        table.ForeignKey(
            //            name: "FK_ChatEntry_User_TblUserWriterId",
            //            column: x => x.TblUserWriterId,
            //            principalTable: "User",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Device",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        DeviceHardwareId = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Plattform = table.Column<int>(type: "int", nullable: false),
            //        DeviceIdiom = table.Column<int>(type: "int", nullable: false),
            //        OperatingSystemVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DeviceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Manufacturer = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DeviceToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AppVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        LastDateTimeUtcOnline = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        IsAppRunning = table.Column<bool>(type: "bit", nullable: false),
            //        ScreenResolution = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        CurrentAppType = table.Column<int>(type: "int", nullable: false),
            //        TblUserId = table.Column<long>(type: "bigint", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Device", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Device_User_TblUserId",
            //            column: x => x.TblUserId,
            //            principalTable: "User",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "FutureWishLike",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        TblUserId = table.Column<long>(type: "bigint", nullable: false),
            //        TblFutureWishId = table.Column<long>(type: "bigint", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_FutureWishLike", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_FutureWishLike_FutureWish_TblFutureWishId",
            //            column: x => x.TblFutureWishId,
            //            principalTable: "FutureWish",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_FutureWishLike_User_TblUserId",
            //            column: x => x.TblUserId,
            //            principalTable: "User",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Idea",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        From = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        To = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LocationLat = table.Column<double>(type: "float", nullable: true),
            //        LocationLon = table.Column<double>(type: "float", nullable: true),
            //        TblUserId = table.Column<long>(type: "bigint", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Idea", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Idea_User_TblUserId",
            //            column: x => x.TblUserId,
            //            principalTable: "User",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Permission",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        UserRight = table.Column<int>(type: "int", nullable: false),
            //        UserRole = table.Column<int>(type: "int", nullable: false),
            //        MainOrganization = table.Column<bool>(type: "bit", nullable: false),
            //        TblUserId = table.Column<long>(type: "bigint", nullable: false),
            //        TblOrganizationId = table.Column<long>(type: "bigint", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Permission", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Permission_Organization_TblOrganizationId",
            //            column: x => x.TblOrganizationId,
            //            principalTable: "Organization",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_Permission_User_TblUserId",
            //            column: x => x.TblUserId,
            //            principalTable: "User",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "IdeaHelper",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        From = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        To = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        Info = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        TblUserId = table.Column<long>(type: "bigint", nullable: false),
            //        TblIdeaId = table.Column<long>(type: "bigint", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_IdeaHelper", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_IdeaHelper_Idea_TblIdeaId",
            //            column: x => x.TblIdeaId,
            //            principalTable: "Idea",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_IdeaHelper_User_TblUserId",
            //            column: x => x.TblUserId,
            //            principalTable: "User",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "IdeaLike",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        TblUserId = table.Column<long>(type: "bigint", nullable: false),
            //        TblIdeaId = table.Column<long>(type: "bigint", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_IdeaLike", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_IdeaLike_Idea_TblIdeaId",
            //            column: x => x.TblIdeaId,
            //            principalTable: "Idea",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_IdeaLike_User_TblUserId",
            //            column: x => x.TblUserId,
            //            principalTable: "User",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "IdeaNeed",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Info = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Amount = table.Column<long>(type: "bigint", nullable: false),
            //        TblIdeaId = table.Column<long>(type: "bigint", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_IdeaNeed", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_IdeaNeed_Idea_TblIdeaId",
            //            column: x => x.TblIdeaId,
            //            principalTable: "Idea",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "IdeaOrganization",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        TblIdeaId = table.Column<long>(type: "bigint", nullable: false),
            //        TblOrganizationId = table.Column<long>(type: "bigint", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_IdeaOrganization", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_IdeaOrganization_Idea_TblIdeaId",
            //            column: x => x.TblIdeaId,
            //            principalTable: "Idea",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_IdeaOrganization_Organization_TblOrganizationId",
            //            column: x => x.TblOrganizationId,
            //            principalTable: "Organization",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "IdeaReport",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        TblUserId = table.Column<long>(type: "bigint", nullable: false),
            //        TblIdeaId = table.Column<long>(type: "bigint", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_IdeaReport", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_IdeaReport_Idea_TblIdeaId",
            //            column: x => x.TblIdeaId,
            //            principalTable: "Idea",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_IdeaReport_User_TblUserId",
            //            column: x => x.TblUserId,
            //            principalTable: "User",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "IdeaSupply",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DataVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
            //        Amount = table.Column<long>(type: "bigint", nullable: false),
            //        CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        TblIdeaNeedId = table.Column<long>(type: "bigint", nullable: false),
            //        TblIdeaHelperId = table.Column<long>(type: "bigint", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_IdeaSupply", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_IdeaSupply_IdeaHelper_TblIdeaHelperId",
            //            column: x => x.TblIdeaHelperId,
            //            principalTable: "IdeaHelper",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_IdeaSupply_IdeaNeed_TblIdeaNeedId",
            //            column: x => x.TblIdeaNeedId,
            //            principalTable: "IdeaNeed",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_AccessToken_TblOrganizationId",
            //    table: "AccessToken",
            //    column: "TblOrganizationId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AccessToken_TblUserId",
            //    table: "AccessToken",
            //    column: "TblUserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Chat_TblIdeaId",
            //    table: "Chat",
            //    column: "TblIdeaId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ChatEntry_TblChatId",
            //    table: "ChatEntry",
            //    column: "TblChatId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ChatEntry_TblUserWriterId",
            //    table: "ChatEntry",
            //    column: "TblUserWriterId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Device_TblUserId",
            //    table: "Device",
            //    column: "TblUserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_FutureWishLike_TblFutureWishId",
            //    table: "FutureWishLike",
            //    column: "TblFutureWishId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_FutureWishLike_TblUserId",
            //    table: "FutureWishLike",
            //    column: "TblUserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Idea_TblUserId",
            //    table: "Idea",
            //    column: "TblUserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_IdeaHelper_TblIdeaId",
            //    table: "IdeaHelper",
            //    column: "TblIdeaId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_IdeaHelper_TblUserId",
            //    table: "IdeaHelper",
            //    column: "TblUserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_IdeaLike_TblIdeaId",
            //    table: "IdeaLike",
            //    column: "TblIdeaId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_IdeaLike_TblUserId",
            //    table: "IdeaLike",
            //    column: "TblUserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_IdeaNeed_TblIdeaId",
            //    table: "IdeaNeed",
            //    column: "TblIdeaId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_IdeaOrganization_TblIdeaId",
            //    table: "IdeaOrganization",
            //    column: "TblIdeaId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_IdeaOrganization_TblOrganizationId",
            //    table: "IdeaOrganization",
            //    column: "TblOrganizationId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_IdeaReport_TblIdeaId",
            //    table: "IdeaReport",
            //    column: "TblIdeaId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_IdeaReport_TblUserId",
            //    table: "IdeaReport",
            //    column: "TblUserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_IdeaSupply_TblIdeaHelperId",
            //    table: "IdeaSupply",
            //    column: "TblIdeaHelperId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_IdeaSupply_TblIdeaNeedId",
            //    table: "IdeaSupply",
            //    column: "TblIdeaNeedId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Permission_TblOrganizationId",
            //    table: "Permission",
            //    column: "TblOrganizationId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Permission_TblUserId",
            //    table: "Permission",
            //    column: "TblUserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_User_TableChatId",
            //    table: "User",
            //    column: "TableChatId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_User_TblUserImageId",
            //    table: "User",
            //    column: "TblUserImageId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_AccessToken_User_TblUserId",
            //    table: "AccessToken",
            //    column: "TblUserId",
            //    principalTable: "User",
            //    principalColumn: "Id");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Chat_Idea_TblIdeaId",
            //    table: "Chat",
            //    column: "TblIdeaId",
            //    principalTable: "Idea",
            //    principalColumn: "Id");
        }
        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Idea_User_TblUserId",
                table: "Idea");

            migrationBuilder.DropTable(
                name: "AccessToken");

            migrationBuilder.DropTable(
                name: "ChatEntry");

            migrationBuilder.DropTable(
                name: "Device");

            migrationBuilder.DropTable(
                name: "FutureWishLike");

            migrationBuilder.DropTable(
                name: "IdeaLike");

            migrationBuilder.DropTable(
                name: "IdeaOrganization");

            migrationBuilder.DropTable(
                name: "IdeaReport");

            migrationBuilder.DropTable(
                name: "IdeaSupply");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "Setting");

            migrationBuilder.DropTable(
                name: "FutureWish");

            migrationBuilder.DropTable(
                name: "IdeaHelper");

            migrationBuilder.DropTable(
                name: "IdeaNeed");

            migrationBuilder.DropTable(
                name: "Organization");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Chat");

            migrationBuilder.DropTable(
                name: "File");

            migrationBuilder.DropTable(
                name: "Idea");
        }
    }
}
