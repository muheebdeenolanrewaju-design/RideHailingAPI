using Microsoft.EntityFrameworkCore;
using RideHailingAPI.Domain.Entities;
using RideHailingAPI.Domain.Enum;

namespace RideHailingAPI.Data;

public class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        // 1. Seed Admin Account
        var admin = new User
        {
            FullName = "System Admin",
            Email = "admin@ridehailing.com",
            PhoneNumber = "07000000000",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminPassword123!"),
            Role = UserRole.Admin,
            IsEmailVerified = true,
            IsPhoneVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 2. Seed Test Passenger Account
        var passenger = new User
        {
            FullName = "John Passenger",
            Email = "passenger@test.com",
            PhoneNumber = "08011112222",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Passenger123!"),
            Role = UserRole.Passenger,
            IsEmailVerified = true,
            IsPhoneVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Seed Test Driver Account
        var driverUser = new User
        {
            FullName = "Jane Driver",
            Email = "driver@test.com",
            PhoneNumber = "09033334444",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Driver123!"),
            Role = UserRole.Driver,
            IsEmailVerified = true,
            IsPhoneVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(admin, passenger, driverUser);
        await context.SaveChangesAsync();

        // 4. Seed Driver Profile and Vehicle for Test Driver
        var driverProfile = new DriverProfile
        {
            UserId = driverUser.Id,
            LicenseNumber = "DL-987654321",
            ApprovalStatus = DriverApprovalStatus.Pending,
            AvailabilityStatus = DriverAvailabilityStatus.Unavailable,
            Vehicle = new Vehicle
            {
                Make = "Toyota",
                Model = "Corolla",
                PlateNumber = "LAG-123-XY",
                Color = "Black",
                Year = 2020
            }
        };

        context.DriverProfiles.Add(driverProfile);
        await context.SaveChangesAsync();
    }
}