using Microsoft.EntityFrameworkCore;
using SEMS.Core.CRM;
using SEMS.Core.Finance;
using SEMS.Core.HR;
using SEMS.Core.Inventory;
using SEMS.Core.Projects;
using SEMS.Core.Reports;
using SEMS.Core.ValueObjects;
using SEMS.Infrastructure.Events;
using SEMS.Core.Common;
using SEMS.Core.Identity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SEMS.Core.Tenancy;
using SEMS.Application.Abstractions;
using System.Reflection;
using SEMS.Core.Auditing;

namespace SEMS.Infrastructure.Persistence;

public class SemsDbContext : DbContext
{
    private readonly IDomainEventDispatcher? _dispatcher;
    private readonly ITenantProvider? _tenantProvider;
    private readonly IUserContext? _userContext;

    public SemsDbContext(DbContextOptions<SemsDbContext> options, IDomainEventDispatcher? dispatcher = null, ITenantProvider? tenantProvider = null, IUserContext? userContext = null) : base(options)
    {
        _dispatcher = dispatcher;
        _tenantProvider = tenantProvider;
        _userContext = userContext;
    }

    public Guid? CurrentTenantId => _tenantProvider?.TenantId;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<Payroll> Payrolls => Set<Payroll>();
    public DbSet<Benefit> Benefits => Set<Benefit>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<CommunicationLog> CommunicationLogs => Set<CommunicationLog>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<TimeLog> TimeLogs => Set<TimeLog>();
    public DbSet<ProjectResource> ProjectResources => Set<ProjectResource>();
    public DbSet<ProjectAssignment> ProjectAssignments => Set<ProjectAssignment>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ReportTemplate> ReportTemplates => Set<ReportTemplate>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Owned<Address>();
        modelBuilder.Owned<Money>();

        // Tenancy
        modelBuilder.Entity<Tenant>().HasIndex(t => t.Identifier).IsUnique();

        // CRM
        modelBuilder.Entity<SEMS.Core.CRM.Customer>().Property(x => x.Email).HasConversion(v => v.Value, v => new Email(v)).HasColumnName("Email");
        modelBuilder.Entity<SEMS.Core.CRM.Customer>().Property(x => x.Phone).HasConversion(v => v.Value, v => new PhoneNumber(v)).HasColumnName("Phone");
        modelBuilder.Entity<SEMS.Core.CRM.Customer>().OwnsOne(x => x.Address);
        modelBuilder.Entity<SEMS.Core.CRM.Customer>()
            .HasMany(c => c.Invoices)
            .WithOne(i => i.Customer)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SEMS.Core.CRM.Contact>().Property(x => x.Email).HasConversion(v => v.Value, v => new Email(v)).HasColumnName("Email");
        modelBuilder.Entity<SEMS.Core.CRM.Contact>().Property(x => x.Phone).HasConversion(v => v.Value, v => new PhoneNumber(v)).HasColumnName("Phone");

        // HR
        modelBuilder.Entity<SEMS.Core.HR.Employee>().Property(x => x.Email).HasConversion(v => v.Value, v => new Email(v)).HasColumnName("Email");
        modelBuilder.Entity<SEMS.Core.HR.Employee>().Property(x => x.Phone).HasConversion(v => v.Value, v => new PhoneNumber(v)).HasColumnName("Phone");
        modelBuilder.Entity<SEMS.Core.HR.Employee>().OwnsOne(x => x.Address);
        
        modelBuilder.Entity<SEMS.Core.HR.Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SEMS.Core.HR.Employee>()
            .HasOne(e => e.Role)
            .WithMany(r => r.Employees)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SEMS.Core.HR.Attendance>()
            .HasOne(a => a.Employee)
            .WithMany(e => e.Attendances)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SEMS.Core.HR.LeaveRequest>()
            .HasOne(l => l.Employee)
            .WithMany(e => e.LeaveRequests)
            .HasForeignKey(l => l.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SEMS.Core.Finance.Expense>().OwnsOne(x => x.Amount);
        modelBuilder.Entity<SEMS.Core.Finance.Payment>().OwnsOne(x => x.Amount);
        modelBuilder.Entity<SEMS.Core.Finance.Transaction>().OwnsOne(x => x.Amount);
        modelBuilder.Entity<SEMS.Core.Finance.Invoice>().OwnsOne(x => x.Total);
        modelBuilder.Entity<SEMS.Core.Finance.BankAccount>().OwnsOne(x => x.Balance);
        
        modelBuilder.Entity<SEMS.Core.Finance.Payment>()
            .HasOne(p => p.Invoice)
            .WithMany() // Assuming Invoice doesn't have Collection<Payment> yet, or use WithMany() if 1:N
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Inventory
        modelBuilder.Entity<SEMS.Core.Inventory.Stock>()
            .HasOne(s => s.Product)
            .WithMany(p => p.Stocks)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SEMS.Core.Inventory.Stock>()
            .HasOne(s => s.Warehouse)
            .WithMany(w => w.Stocks)
            .HasForeignKey(s => s.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Global Query Filter for Soft Delete and Multi-Tenancy
        var configureFiltersMethod = typeof(SemsDbContext).GetMethod(nameof(ConfigureGlobalFilters), BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                configureFiltersMethod?.MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, new object[] { modelBuilder });
            }
        }

        var rolesConverter = new ValueConverter<List<SEMS.Core.Enums.UserRole>, string>(
            v => string.Join(',', v.Select(r => r.ToString())),
            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => Enum.Parse<SEMS.Core.Enums.UserRole>(s, true)).ToList()
        );
        modelBuilder.Entity<User>().Property(u => u.Roles).HasConversion(rolesConverter);
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasIndex(r => r.Token).IsUnique();
    }

    protected void ConfigureGlobalFilters<TEntity>(ModelBuilder builder) where TEntity : BaseEntity
    {
        // Filter logic:
        // 1. IsDeleted must be false
        // 2. TenantId must match CurrentTenantId (if CurrentTenantId is set)
        // Note: For Tenant entity itself, we might want to allow it if it matches the current tenant ID.
        // Since Tenant inherits BaseEntity, it has TenantId.
        // If Tenant A has TenantId=A, then it matches.
        
        builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted && (e.TenantId == CurrentTenantId));
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId;
        var userId = _userContext?.UserId;
        var ipAddress = (_userContext as dynamic)?.IpAddress as string; // will be null if not available

        var auditEntries = new List<AuditLog>();
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedAt == default) entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.IsDeleted = false;
                    entry.Entity.CreatedBy = userId;
                    if (tenantId.HasValue && entry.Entity.TenantId == null)
                    {
                        entry.Entity.TenantId = tenantId;
                    }
                    auditEntries.Add(new AuditLog
                    {
                        Action = "Create",
                        EntityName = entry.Entity.GetType().Name,
                        EntityId = entry.Entity.Id,
                        UserId = userId,
                        IpAddress = ipAddress,
                        TenantId = tenantId,
                        Details = null
                    });
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = userId;
                    var modifiedProps = entry.Properties.Where(p => p.IsModified).Select(p => p.Metadata.Name).ToList();
                    auditEntries.Add(new AuditLog
                    {
                        Action = "Update",
                        EntityName = entry.Entity.GetType().Name,
                        EntityId = entry.Entity.Id,
                        UserId = userId,
                        IpAddress = ipAddress,
                        TenantId = tenantId,
                        Details = modifiedProps.Count > 0 ? string.Join(",", modifiedProps) : null
                    });
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.DeletedBy = userId;
                    auditEntries.Add(new AuditLog
                    {
                        Action = "SoftDelete",
                        EntityName = entry.Entity.GetType().Name,
                        EntityId = entry.Entity.Id,
                        UserId = userId,
                        IpAddress = ipAddress,
                        TenantId = tenantId,
                        Details = null
                    });
                    break;
            }
        }

        var domainEvents = ChangeTracker.Entries<BaseEntity>()
            .Select(e => e.Entity)
            .SelectMany(e => e.DomainEvents)
            .ToList();
        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
        }
        var result = await base.SaveChangesAsync(cancellationToken);
        if (_dispatcher is not null && domainEvents.Count > 0)
        {
            await _dispatcher.DispatchAsync(domainEvents, cancellationToken);
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                entry.Entity.ClearDomainEvents();
            }
        }
        return result;
    }
}
