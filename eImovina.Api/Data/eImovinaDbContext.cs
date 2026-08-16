using eImovina.Shared.Models.Equipments;
using eImovina.Shared.Models.Inventories;
using eImovina.Shared.Models.Locations;
using eImovina.Shared.Models.Users;
using eImovina.Shared.Models.WriteOffRequests;
using Microsoft.EntityFrameworkCore;

namespace eImovina.Api.Data
{
    public class eImovinaDbContext : DbContext
    {
        public eImovinaDbContext(DbContextOptions<eImovinaDbContext> options) : base(options)
        {
        }
        public DbSet<LocationType> LocationTypes => Set<LocationType>();
        public DbSet<EquipmentCategory> EquipmentCategories => Set<EquipmentCategory>();
        public DbSet<EquipmentStatus> EquipmentStatuses => Set<EquipmentStatus>();
        public DbSet<AssignmentStatus> AssignmentStatuses => Set<AssignmentStatus>();
        public DbSet<InventoryStatus> InventoryStatuses => Set<InventoryStatus>();
        public DbSet<RequestStatus> RequestStatuses => Set<RequestStatus>();
        public DbSet<WriteOffRequestStatus> WriteOffRequestStatuses => Set<WriteOffRequestStatus>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<User> Users => Set<User>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Equipment> Equipment => Set<Equipment>();
        public DbSet<EquipmentAssignment> EquipmentAssignments => Set<EquipmentAssignment>();
        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
        public DbSet<EquipmentRequest> EquipmentRequests => Set<EquipmentRequest>();
        public DbSet<WriteOffRequest> WriteOffRequests => Set<WriteOffRequest>();
        public DbSet<EquipmentFile> EquipmentFiles => Set<EquipmentFile>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<LocationType>().HasIndex(e => e.Name).IsUnique();
            modelBuilder.Entity<EquipmentCategory>().HasIndex(e => e.Name).IsUnique();
            modelBuilder.Entity<EquipmentStatus>().HasIndex(e => e.Name).IsUnique();
            modelBuilder.Entity<AssignmentStatus>().HasIndex(e => e.Name).IsUnique();
            modelBuilder.Entity<InventoryStatus>().HasIndex(e => e.Name).IsUnique();
            modelBuilder.Entity<RequestStatus>().HasIndex(e => e.Name).IsUnique();
            modelBuilder.Entity<WriteOffRequestStatus>().HasIndex(e => e.Name).IsUnique();
            modelBuilder.Entity<Role>().HasIndex(e => e.Name).IsUnique();

            modelBuilder.Entity<Location>()
                .HasOne(l => l.LocationType)
                .WithMany(lt => lt.Locations)
                .HasForeignKey(l => l.LocationTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Location)
                .WithMany(l => l.Employees)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Employee)
                .WithOne(e => e.User)
                .HasForeignKey<User>(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Equipment>(e =>
            {
                e.HasIndex(x => x.InventoryNumber).IsUnique();

                e.HasIndex(x => x.SerialNumber)
                    .IsUnique()
                    .HasFilter("[SerialNumber] IS NOT NULL");

                e.HasOne(x => x.Category)
                    .WithMany(c => c.Equipment)
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Status)
                    .WithMany(s => s.Equipment)
                    .HasForeignKey(x => x.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.CurrentLocation)
                    .WithMany(l => l.Equipment)
                    .HasForeignKey(x => x.CurrentLocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.CategoryId, x.StatusId, x.CurrentLocationId });
            });

            modelBuilder.Entity<EquipmentAssignment>(e =>
            {
                e.HasOne(x => x.Equipment)
                    .WithMany(eq => eq.Assignments)
                    .HasForeignKey(x => x.EquipmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Employee)
                    .WithMany(emp => emp.Assignments)
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Status)
                    .WithMany(s => s.Assignments)
                    .HasForeignKey(x => x.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.AssignedByUser)
                    .WithMany(u => u.AssignmentsGiven)
                    .HasForeignKey(x => x.AssignedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.ReturnedByUser)
                    .WithMany(u => u.AssignmentsReturned)
                    .HasForeignKey(x => x.ReturnedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => x.EquipmentId)
                    .IsUnique()
                    .HasFilter("[StatusId] = 1")
                    .HasDatabaseName("IX_EquipmentAssignments_OneActivePerEquipment");
            });

            modelBuilder.Entity<Inventory>(e =>
            {
                e.HasOne(x => x.Location)
                    .WithMany(l => l.Inventories)
                    .HasForeignKey(x => x.LocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.ResponsibleEmployee)
                    .WithMany(emp => emp.ResponsibleForInventories)
                    .HasForeignKey(x => x.ResponsibleEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Status)
                    .WithMany(s => s.Inventories)
                    .HasForeignKey(x => x.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.CreatedByUser)
                    .WithMany(u => u.InventoriesCreated)
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.ClosedByUser)
                    .WithMany(u => u.InventoriesClosed)
                    .HasForeignKey(x => x.ClosedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<InventoryItem>(e =>
            {
                e.HasOne(x => x.Inventory)
                    .WithMany(i => i.Items)
                    .HasForeignKey(x => x.InventoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Equipment)
                    .WithMany(eq => eq.InventoryItems)
                    .HasForeignKey(x => x.EquipmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.ExpectedLocation)
                    .WithMany()
                    .HasForeignKey(x => x.ExpectedLocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.FoundLocation)
                    .WithMany()
                    .HasForeignKey(x => x.FoundLocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.ProcessedByUser)
                    .WithMany(u => u.InventoryItemsProcessed)
                    .HasForeignKey(x => x.ProcessedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.InventoryId, x.EquipmentId }).IsUnique();
            });

            modelBuilder.Entity<EquipmentRequest>(e =>
            {
                e.HasOne(x => x.Employee)
                    .WithMany(emp => emp.EquipmentRequests)
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Category)
                    .WithMany(c => c.EquipmentRequests)
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Status)
                    .WithMany(s => s.EquipmentRequests)
                    .HasForeignKey(x => x.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.ResolvedByUser)
                    .WithMany(u => u.EquipmentRequestsResolved)
                    .HasForeignKey(x => x.ResolvedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<WriteOffRequest>(e =>
            {
                e.HasOne(x => x.Equipment)
                    .WithMany(eq => eq.WriteOffRequests)
                    .HasForeignKey(x => x.EquipmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Status)
                    .WithMany(s => s.WriteOffRequests)
                    .HasForeignKey(x => x.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.RequestedByUser)
                    .WithMany(u => u.WriteOffRequestsSubmitted)
                    .HasForeignKey(x => x.RequestedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.DecisionByUser)
                    .WithMany(u => u.WriteOffRequestsDecided)
                    .HasForeignKey(x => x.DecisionByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EquipmentFile>(e =>
            {
                e.HasOne(x => x.Equipment)
                    .WithMany(eq => eq.Files)
                    .HasForeignKey(x => x.EquipmentId)
                    .OnDelete(DeleteBehavior.Cascade); 

                e.HasOne(x => x.UploadedByUser)
                    .WithMany(u => u.FilesUploaded)
                    .HasForeignKey(x => x.UploadedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => x.StoredFileName).IsUnique();
                e.HasIndex(x => new { x.EquipmentId, x.FileType });

                e.Property(x => x.FileType)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            });
        }
    }
}
