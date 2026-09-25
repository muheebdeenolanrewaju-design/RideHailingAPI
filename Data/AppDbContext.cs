using RideHailingAPI.Domain.Entities;

namespace RideHailingAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<DriverProfile> DriverProfiles { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Ride> Rides { get; set; }
    public DbSet<RideStatusHistory> RideStatusHistories { get; set; }
    public DbSet<EmailOtp> EmailOtps { get; set; }
    public DbSet<PhoneOtp> PhoneOtps { get; set; }
    public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================================
        // User Entity Configuration
        // ==========================================
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        // ==========================================
        // DriverProfile Entity Configuration
        // ==========================================
        modelBuilder.Entity<DriverProfile>()
            .Property(d => d.ApprovalStatus)
            .HasConversion<string>();

        modelBuilder.Entity<DriverProfile>()
            .Property(d => d.AvailabilityStatus)
            .HasConversion<string>();

        // 1-to-1: User <-> DriverProfile
        modelBuilder.Entity<DriverProfile>()
            .HasOne(d => d.User)
            .WithOne(u => u.DriverProfile)
            .HasForeignKey<DriverProfile>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==========================================
        // Vehicle Entity Configuration
        // ==========================================
        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.PlateNumber)
            .IsUnique();

        // 1-to-1: DriverProfile <-> Vehicle
        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.DriverProfile)
            .WithOne(dp => dp.Vehicle)
            .HasForeignKey<Vehicle>(v => v.DriverProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==========================================
        // Ride Entity Configuration
        // ==========================================
        modelBuilder.Entity<Ride>()
            .HasIndex(r => r.RideReference)
            .IsUnique();

        modelBuilder.Entity<Ride>()
            .Property(r => r.CurrentStatus)
            .HasConversion<string>();

        // Many-to-1: Ride -> Passenger (User)
        modelBuilder.Entity<Ride>()
            .HasOne(r => r.Passenger)
            .WithMany(u => u.PassengerRides)
            .HasForeignKey(r => r.PassengerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-1: Ride -> Driver (DriverProfile)
        modelBuilder.Entity<Ride>()
            .HasOne(r => r.Driver)
            .WithMany(dp => dp.AssignedRides)
            .HasForeignKey(r => r.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        // ==========================================
        // RideStatusHistory Entity Configuration
        // ==========================================
        modelBuilder.Entity<RideStatusHistory>()
            .Property(rsh => rsh.PreviousStatus)
            .HasConversion<string>();

        modelBuilder.Entity<RideStatusHistory>()
            .Property(rsh => rsh.NewStatus)
            .HasConversion<string>();

        // Many-to-1: RideStatusHistory -> Ride
        modelBuilder.Entity<RideStatusHistory>()
            .HasOne(rsh => rsh.Ride)
            .WithMany(r => r.StatusHistories)
            .HasForeignKey(rsh => rsh.RideId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-1: RideStatusHistory -> ChangedByUser (User)
        modelBuilder.Entity<RideStatusHistory>()
            .HasOne(rsh => rsh.ChangedByUser)
            .WithMany()
            .HasForeignKey(rsh => rsh.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ==========================================
        // OTP Configurations
        // ==========================================
        modelBuilder.Entity<EmailOtp>()
            .HasOne(o => o.User)
            .WithMany(u => u.EmailOtps)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PhoneOtp>()
            .HasOne(o => o.User)
            .WithMany(u => u.PhoneOtps)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PasswordResetOtp>()
            .HasOne(o => o.User)
            .WithMany(u => u.PasswordResetOtps)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==========================================
        // Notification & AuditLog Configurations
        // ==========================================
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/*okay, now lets work on one particular user first at a time, 

lets take passanger first

lets generate the*/