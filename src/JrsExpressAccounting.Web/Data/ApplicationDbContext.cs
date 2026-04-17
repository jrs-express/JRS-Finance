using JrsExpressAccounting.Web.Models;
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

        builder.Entity<ChartOfAccount>()
            .HasOne(x => x.ParentAccount)
            .WithMany(x => x.SubAccounts)
            .HasForeignKey(x => x.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<JournalEntryLine>()
            .Property(x => x.Debit)
            .HasColumnType("decimal(18,2)");

        builder.Entity<JournalEntryLine>()
            .Property(x => x.Credit)
            .HasColumnType("decimal(18,2)");

        builder.Entity<JournalEntryLine>()
            .Property(x => x.VatAmount)
            .HasColumnType("decimal(18,2)");

        builder.Entity<JournalEntryLine>()
            .Property(x => x.WithholdingTaxAmount)
            .HasColumnType("decimal(18,2)");
    }
}
