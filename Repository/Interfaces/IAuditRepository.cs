using RideHailingAPI.Domain.Entities;

namespace RideHailingAPI.Repository.Interfaces;

public interface IAuditRepository
{
    Task<List<AuditLog>> GetAuditLogsAsync();
    Task AddAuditLogAsync(AuditLog log);
    Task SaveChangesAsync();
}