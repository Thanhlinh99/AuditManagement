using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuditManagement.Models;
using AuditManagement.Audit;
using Microsoft.Extensions.Logging;

namespace AuditManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApplicationDbContext> _logger;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<ApplicationDbContext> logger)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .ToTable("Product")
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Customer>()
                .ToTable("Customer");

            modelBuilder.Entity<Order>()
                .ToTable("Order");

            modelBuilder.Entity<AuditLog>()
                .ToTable("AuditLog");
        }

        public override int SaveChanges()
        {
            return SaveChangesAsync().GetAwaiter().GetResult();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting SaveChangesAsync");
            var auditEntries = OnBeforeSaveChanges();
            _logger.LogInformation($"Found {auditEntries.Count} audit entries");
            
            if (auditEntries.Any())
            {
                _logger.LogInformation("Processing audit entries");
                OnAfterSaveChanges(auditEntries);
            }

            var result = await base.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Changes saved");
            
            return result;
        }

        private List<AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();

            foreach (var entry in ChangeTracker.Entries())
            {
                _logger.LogInformation($"Processing entry: {entry.Entity.GetType().Name} - {entry.State}");

                if (entry.Entity is AuditLog || entry.State == EntityState.Unchanged)
                {
                    _logger.LogInformation($"Skipping entry: {entry.Entity.GetType().Name} - {entry.State}");
                    continue;
                }

                var tableName = entry.Metadata.GetTableName() ?? "Unknown";
                var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
                var action = entry.State.ToString();
                var recordId = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? "Unknown";

                _logger.LogInformation($"Creating audit entry for {tableName} with action {action}");

                var audit = new AuditEntry
                {
                    TableName = tableName,
                    UserId = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Action = action,
                    RecordId = recordId
                };

                foreach (var prop in entry.Properties)
                {
                    string propName = prop.Metadata.Name;
                    if (prop.IsTemporary) continue;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            audit.NewValues[propName] = prop.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            audit.OldValues[propName] = prop.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (prop.IsModified)
                            {
                                audit.OldValues[propName] = prop.OriginalValue;
                                audit.NewValues[propName] = prop.CurrentValue;
                            }
                            break;
                    }
                }

                auditEntries.Add(audit);
            }

            return auditEntries;
        }

        private void OnAfterSaveChanges(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || !auditEntries.Any()) return;

            foreach (var entry in auditEntries)
            {
                _logger.LogInformation($"Creating audit log for {entry.TableName} with action {entry.Action}");
                
                var log = new AuditLog
                {
                    UserId = entry.UserId,
                    Action = entry.Action,
                    TableName = entry.TableName,
                    RecordId = entry.RecordId,
                    Changes = entry.ToJson(),
                    Timestamp = DateTime.UtcNow,
                    IpAddress = entry.IpAddress,
                    UserAgent = entry.UserAgent
                };

                AuditLog.Add(log);
            }
        }
    }
}