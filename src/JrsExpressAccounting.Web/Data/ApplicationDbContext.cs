using JrsExpressAccounting.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JrsExpressAccounting.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<ChartOfAccount> ChartOfAccounts => Set<ChartOfAccount>();
    public DbSet<Bank> Banks => Set<Bank>();
    public DbSet<BusinessParty> BusinessParties => Set<BusinessParty>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<CashTransaction> CashTransactions => Set<CashTransaction>();
    public DbSet<CheckPayment> CheckPayments => Set<CheckPayment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.FullName).HasMaxLength(256);
            entity.HasOne(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<IdentityRole>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(256);
            entity.Property(x => x.NormalizedName).HasMaxLength(256);
        });

        builder.Entity<Branch>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(32);
            entity.Property(x => x.Name).HasMaxLength(150);
            entity.Property(x => x.Tin).HasMaxLength(32);
        });

        builder.Entity<ChartOfAccount>(entity =>
        {
            entity.HasIndex(x => x.AccountCode).IsUnique();
            entity.Property(x => x.AccountCode).HasMaxLength(32);
            entity.Property(x => x.AccountName).HasMaxLength(150);
            entity.Property(x => x.AccountType).HasMaxLength(50);

            entity.HasOne(x => x.ParentAccount)
                .WithMany(x => x.SubAccounts)
                .HasForeignKey(x => x.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Bank>(entity =>
        {
            entity.Property(x => x.BankName).HasMaxLength(150);
            entity.Property(x => x.AccountNumber).HasMaxLength(64);
            entity.HasOne(x => x.ChartOfAccount)
                .WithMany()
                .HasForeignKey(x => x.ChartOfAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<BusinessParty>(entity =>
        {
            entity.Property(x => x.PartyType).HasMaxLength(30);
            entity.Property(x => x.Name).HasMaxLength(150);
            entity.Property(x => x.Tin).HasMaxLength(32);
            entity.Property(x => x.VatType).HasMaxLength(20);
        });

        builder.Entity<JournalEntry>(entity =>
        {
            entity.Property(x => x.ReferenceNo).HasMaxLength(64);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.HasOne(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<JournalEntryLine>(entity =>
        {
            entity.Property(x => x.Debit).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Credit).HasColumnType("decimal(18,2)");
            entity.Property(x => x.VatAmount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.WithholdingTaxAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(x => x.JournalEntry)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ChartOfAccount)
                .WithMany()
                .HasForeignKey(x => x.ChartOfAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CashTransaction>(entity =>
        {
            entity.Property(x => x.Type).HasMaxLength(30);
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Remarks).HasMaxLength(500);

            entity.HasOne(x => x.Bank)
                .WithMany()
                .HasForeignKey(x => x.BankId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CheckPayment>(entity =>
        {
            entity.Property(x => x.CheckNumber).HasMaxLength(64);
            entity.Property(x => x.Payee).HasMaxLength(150);
            entity.Property(x => x.Status).HasMaxLength(30);
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.Property(x => x.UserId).HasMaxLength(450);
            entity.Property(x => x.Action).HasMaxLength(100);
            entity.Property(x => x.EntityName).HasMaxLength(150);
            entity.Property(x => x.EntityKey).HasMaxLength(150);
            entity.HasIndex(x => x.Timestamp);
        });
    }
}
