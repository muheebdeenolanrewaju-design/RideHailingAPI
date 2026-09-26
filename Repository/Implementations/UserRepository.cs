using Microsoft.EntityFrameworkCore;
using RideHailingAPI.Data;
using RideHailingAPI.Domain.Entities;
using RideHailingAPI.Repository.Interfaces;

namespace RideHailingAPI.Repository.Implementations;

public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context;

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetUserByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
    }

    public async Task<User?> GetUserByEmailOrPhoneAsync(string identifier)
    {
        var cleanIdentifier = identifier.Trim().ToLower();
        return await _context.Users.FirstOrDefaultAsync(u => 
            u.Email.ToLower() == cleanIdentifier || u.PhoneNumber == cleanIdentifier);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}