using EventMgt.DatabaseContext.DbEntities;
using EventMgt.Helpers;
using Microsoft.EntityFrameworkCore;

namespace EventMgt.DatabaseContext;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureRole(modelBuilder);
        ConfigureUser(modelBuilder);
        ConfigureCategory(modelBuilder);
        ConfigureVenue(modelBuilder);
        ConfigureEvent(modelBuilder);
        ConfigureEventRegistration(modelBuilder);
    }

    private static void ConfigureRole(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Role>();

        entity.ToTable("Roles");

        entity.HasKey(x => x.Id)
            .HasName("PK_Roles");

        entity.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        entity.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(x => x.Description)
            .HasMaxLength(250);

        entity.Property(x => x.IsActive)
            .HasDefaultValue(true);

        entity.Property(x => x.CreatedDate)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("UX_Roles_Name");
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<User>();

        entity.ToTable("Users");

        entity.HasKey(x => x.Id)
            .HasName("PK_Users");

        entity.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        entity.Property(x => x.Email)
            .HasMaxLength(320)
            .IsRequired();

        entity.Property(x => x.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        entity.Property(x => x.IsActive)
            .HasDefaultValue(true);

        entity.Property(x => x.CreatedDate)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.Property(x => x.UpdatedDate)
            .HasColumnType("datetime2(0)");

        entity.HasIndex(x => x.Email)
            .IsUnique()
            .HasDatabaseName("UX_Users_Email");

        entity.HasIndex(x => x.RoleId)
            .HasDatabaseName("IX_Users_RoleId");

        entity.HasOne(x => x.Role)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.RoleId)
            .HasConstraintName("FK_Users_Roles")
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureCategory(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Category>();

        entity.ToTable("Categories");

        entity.HasKey(x => x.Id)
            .HasName("PK_Categories");

        entity.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.Description)
            .HasMaxLength(500);

        entity.Property(x => x.IsActive)
            .HasDefaultValue(true);

        entity.Property(x => x.CreatedDate)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.Property(x => x.UpdatedDate)
            .HasColumnType("datetime2(0)");

        entity.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("UX_Categories_Name");
    }

    private static void ConfigureVenue(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Venue>();

        entity.ToTable("Venues");

        entity.HasKey(x => x.Id)
            .HasName("PK_Venues");

        entity.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(x => x.Address)
            .HasMaxLength(500);

        entity.Property(x => x.City)
            .HasMaxLength(100);

        entity.Property(x => x.State)
            .HasMaxLength(100);

        entity.Property(x => x.Country)
            .HasMaxLength(100);

        entity.Property(x => x.PostalCode)
            .HasMaxLength(20);

        entity.Property(x => x.IsActive)
            .HasDefaultValue(true);

        entity.Property(x => x.CreatedDate)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.Property(x => x.UpdatedDate)
            .HasColumnType("datetime2(0)");

        entity.HasIndex(x => x.City)
            .HasDatabaseName("IX_Venues_City");

        entity.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_Venues_IsActive");

        entity.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Venues_Capacity",
                "[Capacity] IS NULL OR [Capacity] > 0");
        });
    }

    private static void ConfigureEvent(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Event>();

        entity.ToTable("Events");

        entity.HasKey(x => x.Id)
            .HasName("PK_Events");

        entity.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(x => x.Description)
            .HasColumnType("nvarchar(max)");

        entity.Property(x => x.StartDate)
            .HasColumnType("datetime2(0)");

        entity.Property(x => x.EndDate)
            .HasColumnType("datetime2(0)");

        entity.Property(x => x.MaxParticipants)
            .IsRequired();

        // SQL column is NVARCHAR(30), while the entity uses a strongly typed enum.
        entity.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired()
            .HasDefaultValue(EventStatus.Draft);

        entity.Property(x => x.RegistrationStartDate)
            .HasColumnType("datetime2(0)");

        entity.Property(x => x.RegistrationEndDate)
            .HasColumnType("datetime2(0)");

        entity.Property(x => x.EventImageUrl)
            .HasMaxLength(1000);

        // SQL column is TINYINT (1,2,3), matching EventMode enum underlying byte.
        entity.Property(x => x.EventMode)
            .HasConversion<byte>()
            .HasDefaultValue(EventMode.InPerson)
            .IsRequired();

        entity.Property(x => x.OnlineMeetingUrl)
            .HasMaxLength(1000);

        entity.Property(x => x.IsActive)
            .HasDefaultValue(true);

        entity.Property(x => x.IsCancelled)
            .HasDefaultValue(false);

        entity.Property(x => x.CancellationReason)
            .HasMaxLength(500);

        entity.Property(x => x.CreatedDate)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.Property(x => x.UpdatedDate)
            .HasColumnType("datetime2(0)");

        entity.HasIndex(x => x.CategoryId)
            .HasDatabaseName("IX_Events_CategoryId");

        entity.HasIndex(x => x.VenueId)
            .HasDatabaseName("IX_Events_VenueId");

        entity.HasIndex(x => x.OrganizerId)
            .HasDatabaseName("IX_Events_OrganizerId");

        entity.HasIndex(x => x.StartDate)
            .HasDatabaseName("IX_Events_StartDate");

        entity.HasIndex(x => x.Status)
            .HasDatabaseName("IX_Events_Status");

        entity.HasIndex(x => new { x.IsActive, x.Status, x.StartDate })
            .HasDatabaseName("IX_Events_IsActive_Status_StartDate");

        entity.HasOne(x => x.Category)
            .WithMany(x => x.Events)
            .HasForeignKey(x => x.CategoryId)
            .HasConstraintName("FK_Events_Categories")
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Venue)
            .WithMany(x => x.Events)
            .HasForeignKey(x => x.VenueId)
            .HasConstraintName("FK_Events_Venues")
            .OnDelete(DeleteBehavior.Restrict);

        // Explicit mapping is important because User has another relationship
        // with EventRegistration.
        entity.HasOne(x => x.Organizer)
            .WithMany(x => x.OrganizedEvents)
            .HasForeignKey(x => x.OrganizerId)
            .HasConstraintName("FK_Events_Organizer")
            .OnDelete(DeleteBehavior.Restrict);

        entity.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Events_DateRange",
                "[EndDate] > [StartDate]");

            t.HasCheckConstraint(
                "CK_Events_MaxParticipants",
                "[MaxParticipants] > 0");

            t.HasCheckConstraint(
                "CK_Events_Status",
                "[Status] IN (N'Draft', N'Published', N'Completed', N'Cancelled')");

            t.HasCheckConstraint(
                "CK_Events_RegistrationDates",
                "[RegistrationStartDate] IS NULL OR [RegistrationEndDate] IS NULL OR [RegistrationEndDate] >= [RegistrationStartDate]");

            t.HasCheckConstraint(
                "CK_Events_RegistrationWithinEvent",
                "[RegistrationStartDate] IS NULL OR [RegistrationStartDate] <= [StartDate]");

            t.HasCheckConstraint(
                "CK_Events_EventMode",
                "[EventMode] IN (1, 2, 3)");

            t.HasCheckConstraint(
                "CK_Events_ModeRequirements",
                "([EventMode] = 1 AND [VenueId] IS NOT NULL AND [OnlineMeetingUrl] IS NULL) OR " +
                "([EventMode] = 2 AND [VenueId] IS NULL AND [OnlineMeetingUrl] IS NOT NULL) OR " +
                "([EventMode] = 3 AND [VenueId] IS NOT NULL AND [OnlineMeetingUrl] IS NOT NULL)");

            t.HasCheckConstraint(
                "CK_Events_Cancellation",
                "[IsCancelled] = 0 OR [Status] = N'Cancelled'");
        });
    }

    private static void ConfigureEventRegistration(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<EventRegistration>();

        entity.ToTable("EventRegistrations");

        entity.HasKey(x => x.Id)
            .HasName("PK_EventRegistrations");

        entity.Property(x => x.RegistrationDate)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(RegistrationStatus.Registered);

        entity.Property(x => x.CancelledDate)
            .HasColumnType("datetime2(0)");

        entity.Property(x => x.CreatedDate)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        entity.Property(x => x.UpdatedDate)
            .HasColumnType("datetime2(0)");

        entity.HasIndex(x => new { x.EventId, x.UserId })
            .IsUnique()
            .HasDatabaseName("UX_EventRegistrations_EventId_UserId");

        entity.HasIndex(x => x.EventId)
            .HasDatabaseName("IX_EventRegistrations_EventId");

        entity.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_EventRegistrations_UserId");

        entity.HasIndex(x => new { x.UserId, x.Status })
            .HasDatabaseName("IX_EventRegistrations_UserId_Status");

        entity.HasOne(x => x.Event)
            .WithMany(x => x.Registrations)
            .HasForeignKey(x => x.EventId)
            .HasConstraintName("FK_EventRegistrations_Events")
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.User)
            .WithMany(x => x.EventRegistrations)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("FK_EventRegistrations_Users")
            .OnDelete(DeleteBehavior.Restrict);

        entity.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_EventRegistrations_Status",
                "[Status] IN (N'Registered', N'Cancelled')");

            t.HasCheckConstraint(
                "CK_EventRegistrations_Cancellation",
                "([Status] = N'Registered' AND [CancelledDate] IS NULL) OR " +
                "([Status] = N'Cancelled' AND [CancelledDate] IS NOT NULL)");
        });
    }
}
