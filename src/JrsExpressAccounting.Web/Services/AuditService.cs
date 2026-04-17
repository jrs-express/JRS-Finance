using JrsExpressAccounting.Web.Data;
using JrsExpressAccounting.Web.Models;

namespace JrsExpressAccounting.Web.Services;

public interface IAuditService
{
    Task LogAsync(string userId, string action, string entityName, string entityKey, string changes);
}

public class AuditService(ApplicationDbContext db) : IAuditService
{
    public async Task LogAsync(string userId, string action, string entityName, string entityKey, string changes)
    {
        db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityKey = entityKey,
            Changes = changes,
            Timestamp = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }
}
