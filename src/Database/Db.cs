// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Biss.Apps.Base;
using Biss.Common;
using Biss.Log.Producer;
using Database.Tables;
using Exchange.Enum;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebExchange;

namespace Database;

/// <summary>
///     <para>Projektweite Datenbank - Entity Framework Core</para>
///     Klasse Db. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class Db : DbContext
{
    #region Erzugen, Neu Erzeugen, Löschen

    /// <summary>
    ///     Datenbank wird bei Aufruf erzugt bzw. gelöscht und neu erzeugt
    /// </summary>
    /// <returns>Erfolg</returns>
    public static bool CreateAndFillUp()
    {
        using var db = new Db();

        if (db.Database.CanConnect())
        {
            Logging.Log.LogWarning($"[{nameof(Db)}]({nameof(CreateAndFillUp)}): DB already exist. Can not create. Please delete DB first.");
            //return false;
        }
        else
        {
            var connstrBldr = new SqlConnectionStringBuilder(db.Database.GetConnectionString())
                              {
                                  InitialCatalog = "master"
                              };

            using (var conn = new SqlConnection(connstrBldr.ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandTimeout = 360;
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                cmd.CommandText = $"CREATE DATABASE [{db.Database.GetDbConnection().Database}] (EDITION = 'basic')";
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                cmd.ExecuteNonQuery();
            }
        }

        if (!db.Database.EnsureCreated())
        {
            Logging.Log.LogError($"[{nameof(Db)}]({nameof(CreateAndFillUp)}): DB create fail!");
            return false;
        }

        #region Initiale Daten für Db

        #region User

        var admin = new TableUser
                    {
                        LoginName = "admin@lenie.at",
                        FirstName = "Lenie",
                        LastName = "Admin",
                        Locked = false,
                        PasswordHash = AppCrypt.CumputeHash("lenie"),
                        DefaultLanguage = "de",
                        LoginConfirmed = true,
                        IsAdmin = true,
                        AgbVersion = "1.0.0",
                        CreatedAtUtc = DateTime.UtcNow,
                        RefreshToken = AppCrypt.GeneratePassword(),
                        JwtToken = AppCrypt.GeneratePassword()
                    };
        db.TblUsers.Add(admin);

        #endregion

        #region Org

        var noCompany = new TableOrganization
                        {
                            Name = "NoCompany",
                            OrganizationType = EnumOrganizationTypes.NoOrganization
                        };

        var wrnCompany = new TableOrganization
                         {
                             Name = "Wiener Neustadt",
                             PostalCode = "2700",
                             OrganizationType = EnumOrganizationTypes.PublicOrganization
                         };

        db.TblOrganizations.Add(noCompany);
        db.TblOrganizations.Add(wrnCompany);

        db.TblPermissions.Add(new TablePermission
                              {
                                  MainOrganization = true,
                                  TblUser = admin,
                                  TblOrganization = wrnCompany,
                                  UserRole = EnumUserRole.Admin,
                                  UserRight = EnumUserRight.ReadWrite,
                              });

        #endregion

        #region Future Wishes

        db.TblFutureWishes.Add(new TableFutureWish {Title = "Kommentieren", Description = "Einträge kommentieren, ohne direkt zu helfen", Link = "https://www.fotec.at"});
        db.TblFutureWishes.Add(new TableFutureWish {Title = "Suche/biete", Description = "Einträge um Sachen die ich Suche/biete anzuzeigen", Link = "https://www.fotec.at"});
        db.TblFutureWishes.Add(new TableFutureWish {Title = "Buchungen", Description = "Buchen von Einrichtungen wie Saal, Grillplatz, etc.", Link = "https://www.fotec.at"});
        db.TblFutureWishes.Add(new TableFutureWish {Title = "Kalender", Description = "Integrierter Kalender zur Anzeige aller Events und Unterstützungen", Link = "https://www.fotec.at"});

        #endregion

        #region Settings

        var settings = EnumUtil.GetValues<EnumDbSettings>();
        foreach (var setting in settings)
        {
            var v = new Version(0, 1);
            switch (setting)
            {
                case EnumDbSettings.Agb:
                    db.TblSettings.Add(new TableSetting {Key = setting, Value = v.ToString()});
                    break;
                case EnumDbSettings.CurrentAppVersion:
                    db.TblSettings.Add(new TableSetting {Key = setting, Value = v.ToString()});
                    break;
                case EnumDbSettings.MinAppVersion:
                    db.TblSettings.Add(new TableSetting {Key = setting, Value = v.ToString()});
                    break;
                case EnumDbSettings.CommonMessage:
                    db.TblSettings.Add(new TableSetting {Key = setting, Value = ""});
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #endregion

        try
        {
            db.SaveChanges();
        }
        catch (Exception e)
        {
            Logging.Log.LogError($"[{nameof(Db)}]({nameof(CreateAndFillUp)}): Error creating database: {e}");
            return false;
        }

        return true;
    }

    #endregion

    /// <summary>
    ///     Db Context initialisieren - für SQL Server
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder != null!)
        {
#if DEBUG
            optionsBuilder.EnableDetailedErrors();
#endif
            optionsBuilder.UseSqlServer(WebConstants.ConnectionString);
        }
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder == null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        modelBuilder.Entity<TableIdeaHelper>()
            .HasOne(x => x.TblUser)
            .WithMany(x => x.TblIdeaHelpers)
            .HasForeignKey(x => x.TblUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<TableIdeaLike>()
            .HasOne(x => x.TblUser)
            .WithMany(x => x.TblIdeaLikes)
            .HasForeignKey(x => x.TblUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<TableIdeaReport>()
            .HasOne(x => x.TblUser)
            .WithMany(x => x.TblIdeaReports)
            .HasForeignKey(x => x.TblUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<TableIdeaSupply>()
            .HasOne(x => x.TblIdeaNeed)
            .WithMany(x => x.TblIdeaSupplies)
            .HasForeignKey(x => x.TblIdeaNeedId)
            .OnDelete(DeleteBehavior.NoAction);

        base.OnModelCreating(modelBuilder);
    }

    #region Tabellen

    /// <summary>
    ///     Tabelle User
    /// </summary>
    public virtual DbSet<TableUser> TblUsers { get; set; } = null!;

    /// <summary>
    ///     Tabelle Settings
    /// </summary>
    public virtual DbSet<TableSetting> TblSettings { get; set; } = null!;

    /// <summary>
    ///     Tabelle Devices
    /// </summary>
    public virtual DbSet<TableDevice> TblDevices { get; set; } = null!;

    /// <summary>
    ///     Tabelle Files z.B. User-Bild
    /// </summary>
    public virtual DbSet<TableFile> TblFiles { get; set; } = null!;

    /// <summary>
    ///     Tabelle Organisationen
    /// </summary>
    public virtual DbSet<TableOrganization> TblOrganizations { get; set; } = null!;

    /// <summary>
    ///     Tabelle Berechtigung
    /// </summary>
    public virtual DbSet<TablePermission> TblPermissions { get; set; } = null!;

    /// <summary>
    ///     Tabelle AccessToken
    /// </summary>
    public virtual DbSet<TableAccessToken> TblAccessToken { get; set; } = null!;

    /// <summary>
    ///     Tabelle neue Features
    /// </summary>
    public virtual DbSet<TableFutureWish> TblFutureWishes { get; set; } = null!;

    /// <summary>
    ///     Tabelle Likes auf neue Features
    /// </summary>
    public virtual DbSet<TableFutureWishLike> TblFutureWishLikes { get; set; } = null!;

    /// <summary>
    ///     Tabelle Ideen
    /// </summary>
    public virtual DbSet<TableIdea> TblIdeas { get; set; } = null!;

    /// <summary>
    ///     Tabelle benötigte Sachen für Idee
    /// </summary>
    public virtual DbSet<TableIdeaNeed> TblIdeaNeeds { get; set; } = null!;

    /// <summary>
    ///     Tabelle zur verfügung gestellte Sachen für Idee
    /// </summary>
    public virtual DbSet<TableIdeaSupply> TblIdeaSupplies { get; set; } = null!;

    /// <summary>
    ///     Tabelle gelikte Ideen
    /// </summary>
    public virtual DbSet<TableIdeaLike> TblIdeaLikes { get; set; } = null!;

    /// <summary>
    ///     Tabelle gemeldete Ideen
    /// </summary>
    public virtual DbSet<TableIdeaReport> TblIdeaReports { get; set; } = null!;

    /// <summary>
    ///     Tabelle Helfer für Ideen
    /// </summary>
    public virtual DbSet<TableIdeaHelper> TblIdeaHelpers { get; set; } = null!;

    /// <summary>
    ///     Tabelle Idee für Regionen
    /// </summary>
    public virtual DbSet<TableIdeaOrganization> TblIdeaOrganizations { get; set; } = null!;

    /// <summary>
    ///     Tabelle TblChat
    /// </summary>
    public virtual DbSet<TableChat> TblChat { get; set; } = null!;

    /// <summary>
    ///     Tabelle TblChatEntry
    /// </summary>
    public virtual DbSet<TableChatEntry> TblChatEntry { get; set; } = null!;

    /// <summary>
    ///     Tabelle TblChatUsers
    /// </summary>
    public virtual DbSet<TableChatUsers> TblChatUser { get; set; } = null!;

    /// <summary>
    ///     Tabelle TblIntro
    /// </summary>
    public virtual DbSet<TableIntro> TblIntro { get; set; } = null!;

    #endregion
}