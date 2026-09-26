using RideHailingAPI.Domain.Entities;
using RideHailingAPI.Repository.Interfaces;

namespace RideHailingAPI.Repository.Implementations;

public class AuditRepository : IAuditRepository
{
    public Task<List<AuditLog>> GetAuditLogsAsync()
    {
        throw new NotImplementedException();
    }

    public Task AddAuditLogAsync(AuditLog log)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync()
    {
        throw new NotImplementedException();
    }
}