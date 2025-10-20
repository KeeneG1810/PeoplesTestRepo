using Microsoft.EntityFrameworkCore;

namespace PeopleAccountsManager.Models
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(p => p.Code).HasName("PK_Persons");

                entity.HasIndex(p => p.IdNumber)
                    .IsUnique()
                    .HasDatabaseName("IX_Person_id");

                entity.HasMany(p => p.Accounts)
                    .WithOne(a => a.Person)
                    .HasForeignKey(a => a.PersonCode)
                    .HasConstraintName("FK_Account_Person")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(a => a.Code).HasName("PK_Accounts");

                entity.HasIndex(a => a.AccountNumber)
                    .IsUnique()
                    .HasDatabaseName("IX_Account_num");

                entity.HasMany(a => a.Transactions)
                    .WithOne(t => t.Account)
                    .HasForeignKey(t => t.AccountCode)
                    .HasConstraintName("FK_Transaction_Account")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Code).HasName("PK_Transactions");

                entity.Property(t => t.TransactionDate).HasColumnType("datetime");
                entity.Property(t => t.CaptureDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Code).HasName("PK_Users");

                entity.Property(u => u.Name).HasColumnType("varchar(50)");
                entity.Property(u => u.Username).HasColumnType("varchar(50)");
                entity.Property(u => u.PasswordHash).HasColumnType("varbinary(32)");
                entity.Property(u => u.PasswordSalt).HasColumnType("varbinary(32)");

                entity.HasIndex(u => u.Username)
                      .IsUnique()
                      .HasDatabaseName("IX_User_username");
            });
        }
    }
}