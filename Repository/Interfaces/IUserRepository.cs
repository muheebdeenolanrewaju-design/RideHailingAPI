using RideHailingAPI.Domain.Entities;

namespace RideHailingAPI.Repository.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByPhoneNumberAsync(string phoneNumber);
    Task<User?> GetUserByEmailOrPhoneAsync(string identifier);
    Task<List<User>> GetAllUsersAsync();
    Task AddUserAsync(User user);
    Task SaveChangesAsync();
}

