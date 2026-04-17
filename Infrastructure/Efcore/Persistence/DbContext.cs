using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Efcore.Persistence
{
    public class BillScannerDbContext(
        DbContextOptions<BillScannerDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();

        public DbSet<Bill> Bills => Set<Bill>();

        public DbSet<BillItem> BillItems => Set<BillItem>();


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            var now = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.UpdatedAt = now;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        break;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ApplyBaseEntityConfigurationToDerivedClass(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BillScannerDbContext).Assembly);
        }

        /// <summary>
        /// Configuring the base entity
        /// </summary>
        /// <param name="modelBuilder"></param>
        private static void ApplyBaseEntityConfigurationToDerivedClass(ModelBuilder modelBuilder)
        {
            var clrTypes = modelBuilder.Model.GetEntityTypes()
                .Where(entityType =>
                    typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)
                    && entityType.ClrType != typeof(BaseEntity))
                .Select(entityType => entityType.ClrType);

            foreach (var clrType in clrTypes)
            {
                // Configure Id
                modelBuilder.Entity(clrType)
                    .HasKey(nameof(BaseEntity.Id));

                // Configure soft delete query filter: e => e.DeletedAt == null
                var parameter = Expression.Parameter(clrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
                var nullConstant = Expression.Constant(null, typeof(DateTime?));
                var equalExpression = Expression.Equal(property, nullConstant);
                var lambda = Expression.Lambda(equalExpression, parameter);

                modelBuilder.Entity(clrType).HasQueryFilter(lambda);
            }
        }
    }
}