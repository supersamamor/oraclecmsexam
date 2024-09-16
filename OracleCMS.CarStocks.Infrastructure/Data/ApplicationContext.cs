using OracleCMS.Common.Data;
using OracleCMS.Common.Identity.Abstractions;
using OracleCMS.CarStocks.Core.CarStocks;
using Microsoft.EntityFrameworkCore;
using LanguageExt;

namespace OracleCMS.CarStocks.Infrastructure.Data;

public class ApplicationContext : AuditableDbContext<ApplicationContext>
{
    private readonly IAuthenticatedUser _authenticatedUser;

    public ApplicationContext(DbContextOptions<ApplicationContext> options,
                              IAuthenticatedUser authenticatedUser) : base(options, authenticatedUser)
    {
        _authenticatedUser = authenticatedUser;
    }
    public DbSet<ReportState> Report { get; set; } = default!;
    public DbSet<ReportQueryFilterState> ReportQueryFilter { get; set; } = default!;
    public DbSet<ReportRoleAssignmentState> ReportRoleAssignment { get; set; } = default!;
    public DbSet<ReportAIIntegrationState> ReportAIIntegration { get; set; } = default!;
    public DbSet<UploadProcessorState> UploadProcessor { get; set; } = default!;
    public DbSet<ApprovalState> Approval { get; set; } = default!;
    public DbSet<ApproverSetupState> ApproverSetup { get; set; } = default!;
    public DbSet<ApproverAssignmentState> ApproverAssignment { get; set; } = default!;
    public DbSet<ApprovalRecordState> ApprovalRecord { get; set; } = default!;

    public DbSet<DealersState> Dealers { get; set; } = default!;
    public DbSet<CarsState> Cars { get; set; } = default!;
    public DbSet<StocksState> Stocks { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                                                   .SelectMany(t => t.GetProperties())
                                                   .Where(p => p.ClrType == typeof(decimal)
                                                               || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,6)");
        }
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties()
                                               .Where(p => p.Name.Equals("CreatedBy", StringComparison.OrdinalIgnoreCase)
                                               || p.Name.Equals("LastModifiedBy", StringComparison.OrdinalIgnoreCase)
                                               || p.Name.Equals("Entity", StringComparison.OrdinalIgnoreCase)
                                               || p.Name.Equals("LastModifiedDate", StringComparison.OrdinalIgnoreCase)
                                               || p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
            {
                if (!property.Name.Equals("LastModifiedDate", StringComparison.OrdinalIgnoreCase))
                {
                    property.SetMaxLength(36);
                }
                entityType.AddIndex(property);
            }
        }
        #region Disable Cascade Delete
        var cascadeFKs = modelBuilder.Model.GetEntityTypes()
        .SelectMany(t => t.GetForeignKeys())
        .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);
        foreach (var fk in cascadeFKs)
        {
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }
        #endregion
        modelBuilder.Entity<Audit>().Property(e => e.PrimaryKey).HasMaxLength(120);
        modelBuilder.Entity<Audit>().HasIndex(p => p.PrimaryKey);
        modelBuilder.Entity<Audit>().HasIndex(p => p.TraceId);
        modelBuilder.Entity<Audit>().HasIndex(p => p.DateTime);    


        modelBuilder.Entity<DealersState>().Property(e => e.DealerName).HasMaxLength(100);
        modelBuilder.Entity<DealersState>().Property(e => e.DealerWebsite).HasMaxLength(255);
        modelBuilder.Entity<CarsState>().Property(e => e.Make).HasMaxLength(50);
        modelBuilder.Entity<CarsState>().Property(e => e.Model).HasMaxLength(50);

        modelBuilder.Entity<CarsState>().HasMany(t => t.StocksList).WithOne(l => l.Cars).HasForeignKey(t => t.CarID);
        modelBuilder.Entity<DealersState>().HasMany(t => t.StocksList).WithOne(l => l.Dealers).HasForeignKey(t => t.DealerID);

        modelBuilder.Entity<ApprovalRecordState>().HasQueryFilter(e => _authenticatedUser.Entity == Core.Constants.Entities.Default.ToUpper() || e.Entity == _authenticatedUser.Entity);
        modelBuilder.Entity<ApprovalState>().HasQueryFilter(e => _authenticatedUser.Entity == Core.Constants.Entities.Default.ToUpper() || e.Entity == _authenticatedUser.Entity);
        modelBuilder.Entity<ApproverSetupState>().HasQueryFilter(e => _authenticatedUser.Entity == Core.Constants.Entities.Default.ToUpper() || e.Entity == _authenticatedUser.Entity);
        modelBuilder.Entity<ApproverAssignmentState>().HasQueryFilter(e => _authenticatedUser.Entity == Core.Constants.Entities.Default.ToUpper() || e.Entity == _authenticatedUser.Entity);
        modelBuilder.Entity<ApprovalRecordState>().HasIndex(l => l.DataId);
        modelBuilder.Entity<ApprovalRecordState>().Property(e => e.DataId).HasMaxLength(36);
        modelBuilder.Entity<ApprovalRecordState>().Property(e => e.ApproverSetupId).HasMaxLength(36);
        modelBuilder.Entity<ApprovalRecordState>().HasIndex(l => l.ApproverSetupId);
        modelBuilder.Entity<ApprovalRecordState>().HasIndex(l => l.Status);
        modelBuilder.Entity<ApprovalRecordState>().Property(e => e.Status).HasMaxLength(450);
        modelBuilder.Entity<ApprovalState>().HasIndex(l => l.ApproverUserId);
        modelBuilder.Entity<ApprovalState>().HasIndex(l => l.Status);
        modelBuilder.Entity<ApprovalState>().HasIndex(l => l.EmailSendingStatus);
        modelBuilder.Entity<ApprovalState>().Property(e => e.ApproverUserId).HasMaxLength(36);
        modelBuilder.Entity<ApprovalState>().Property(e => e.Status).HasMaxLength(450);
        modelBuilder.Entity<ApprovalState>().Property(e => e.EmailSendingStatus).HasMaxLength(450);
        modelBuilder.Entity<ApproverSetupState>().Property(e => e.TableName).HasMaxLength(450);
        modelBuilder.Entity<ApproverSetupState>().Property(e => e.ApprovalType).HasMaxLength(450);
        modelBuilder.Entity<ApproverSetupState>().Property(e => e.EmailSubject).HasMaxLength(450);
        modelBuilder.Entity<ApproverSetupState>().Property(e => e.WorkflowName).HasMaxLength(450);
        modelBuilder.Entity<ApproverSetupState>().HasIndex(e => new { e.WorkflowName, e.ApprovalSetupType, e.TableName, e.Entity }).IsUnique();
        modelBuilder.Entity<ApproverAssignmentState>().Property(e => e.ApproverUserId).HasMaxLength(36);
        modelBuilder.Entity<ApproverAssignmentState>().Property(e => e.ApproverRoleId).HasMaxLength(36);
        modelBuilder.Entity<ApproverAssignmentState>().HasIndex(e => new { e.ApproverSetupId, e.ApproverUserId, e.ApproverRoleId }).IsUnique();
        modelBuilder.Entity<UploadProcessorState>().Property(e => e.FileType).HasMaxLength(20);
        modelBuilder.Entity<UploadProcessorState>().Property(e => e.Path).HasMaxLength(450);
        modelBuilder.Entity<UploadProcessorState>().Property(e => e.Status).HasMaxLength(20);
        modelBuilder.Entity<UploadProcessorState>().Property(e => e.Module).HasMaxLength(50);
        modelBuilder.Entity<UploadProcessorState>().Property(e => e.UploadType).HasMaxLength(50);

       

        base.OnModelCreating(modelBuilder);      

    }
  
}
