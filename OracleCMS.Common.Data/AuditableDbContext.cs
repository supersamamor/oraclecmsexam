using OracleCMS.Common.Core.Base.Models;
using OracleCMS.Common.Identity.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace OracleCMS.Common.Data;

/// <summary>
/// A base class for contexts that automatically creates audit logs for transactions.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class AuditableDbContext<T> : AuditableContext where T : DbContext
{
    /// <summary>
    /// Represents the authenticated user doing the transaction.
    /// </summary>
    protected readonly IAuthenticatedUser AuthenticatedUser;

    /// <summary>
    /// Creates an instance of <see cref="AuditableDbContext{T}"/>.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="authenticatedUser"></param>
    public AuditableDbContext(DbContextOptions<T> options,
                            IAuthenticatedUser authenticatedUser) : base(options)
    {
        AuthenticatedUser = authenticatedUser;
    }

    /// <summary>
    /// Override this method to further configure the model that was discovered by convention 
    /// from the entity types exposed in <see cref="DbSet{TEntity}"/> properties on your 
    /// derived context. The resulting model may be cached and re-used for subsequent instances 
    /// of your derived context.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                                                   .SelectMany(t => t.GetProperties())
                                                   .Where(p => p.ClrType == typeof(decimal)
                                                               || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,6)");
        }

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetBaseFields(AuthenticatedUser);
        return base.SaveChangesAsync(AuthenticatedUser.UserId, AuthenticatedUser.TraceId, cancellationToken);
    }
	/// <summary>
    /// Saves changes to the database, disregarding the audit information.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<int> UpdateRecordFromJobsAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
    public Task<int> UpdateBatchRecordAsync(string? userId, BaseEntity entity, CancellationToken cancellationToken = default)
    {
        SetBaseFieldsFromBatchUpload(userId, entity);
        return base.SaveChangesAsync(userId: userId, cancellationToken: cancellationToken);
    }
    public void DetachAllTrackedEntities()
	{
		foreach (var entry in ChangeTracker.Entries().ToList())
		{
			entry.State = EntityState.Detached;
		}
	}
    void SetBaseFields(IAuthenticatedUser authenticatedUser)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Entity = authenticatedUser.Entity;
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                    entry.Entity.CreatedBy = authenticatedUser.UserId;
                    entry.Entity.LastModifiedDate = DateTime.UtcNow;
                    entry.Entity.LastModifiedBy = authenticatedUser.UserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedDate = DateTime.UtcNow;
                    entry.Entity.LastModifiedBy = authenticatedUser.UserId;
                    break;
            }
        }
    }
    void SetBaseFieldsFromBatchUpload(string? userId, BaseEntity entity)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Entity = entity.Entity;
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.LastModifiedDate = DateTime.UtcNow;
                    entry.Entity.LastModifiedBy = userId;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedDate = DateTime.UtcNow;
                    entry.Entity.LastModifiedBy = userId;
                    break;
            }
        }
    }
}
