using JrsExpressAccounting.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace JrsExpressAccounting.Web.Services;

public interface IReportService
{
    Task<List<GeneralLedgerLine>> GetGeneralLedgerAsync(DateTime from, DateTime to, int? branchId);
    Task<List<IncomeStatementLine>> GetIncomeStatementAsync(DateTime from, DateTime to, int? branchId);
    Task<List<BirCasReportLine>> GetBirCasSummaryAsync(DateTime from, DateTime to, int? branchId);
}

public record GeneralLedgerLine(DateTime Date, string ReferenceNo, string AccountCode, string AccountName, decimal Debit, decimal Credit, decimal Vat, decimal Wtax);
public record IncomeStatementLine(string AccountType, string AccountName, decimal Amount);
public record BirCasReportLine(string ReportName, string Description, int Count);

public class ReportService(ApplicationDbContext db) : IReportService
{
    public async Task<List<GeneralLedgerLine>> GetGeneralLedgerAsync(DateTime from, DateTime to, int? branchId)
    {
        var query = db.JournalEntryLines
            .Include(l => l.JournalEntry)
            .Include(l => l.ChartOfAccount)
            .Where(l => l.JournalEntry != null && l.JournalEntry.TransactionDate >= from && l.JournalEntry.TransactionDate <= to);

        if (branchId.HasValue)
        {
            query = query.Where(l => l.JournalEntry!.BranchId == branchId.Value);
        }

        return await query
            .OrderBy(l => l.JournalEntry!.TransactionDate)
            .Select(l => new GeneralLedgerLine(
                l.JournalEntry!.TransactionDate,
                l.JournalEntry.ReferenceNo,
                l.ChartOfAccount!.AccountCode,
                l.ChartOfAccount.AccountName,
                l.Debit,
                l.Credit,
                l.VatAmount,
                l.WithholdingTaxAmount))
            .ToListAsync();
    }

    public async Task<List<IncomeStatementLine>> GetIncomeStatementAsync(DateTime from, DateTime to, int? branchId)
    {
        var gl = await GetGeneralLedgerAsync(from, to, branchId);

        return gl.GroupBy(x => x.AccountCode)
            .Select(g => new IncomeStatementLine(
                g.First().AccountCode.StartsWith("4") ? "Revenue" : "Expense",
                g.First().AccountName,
                g.Sum(x => x.Credit - x.Debit)))
            .OrderBy(x => x.AccountType)
            .ToList();
    }

    public async Task<List<BirCasReportLine>> GetBirCasSummaryAsync(DateTime from, DateTime to, int? branchId)
    {
        var gl = await GetGeneralLedgerAsync(from, to, branchId);
        var transactions = gl.Select(x => x.ReferenceNo).Distinct().Count();

        return
        [
            new BirCasReportLine("Books of Accounts", "General Ledger transactions", transactions),
            new BirCasReportLine("Tax Summary", "VAT line entries", gl.Count(x => x.Vat != 0)),
            new BirCasReportLine("Tax Summary", "Withholding tax line entries", gl.Count(x => x.Wtax != 0))
        ];
    }
}
