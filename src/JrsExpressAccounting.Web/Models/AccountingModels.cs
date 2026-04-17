using Microsoft.AspNetCore.Identity;

namespace JrsExpressAccounting.Web.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }
}

public class Branch
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Tin { get; set; } = string.Empty;
}

public class ChartOfAccount
{
    public int Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int? ParentAccountId { get; set; }
    public ChartOfAccount? ParentAccount { get; set; }
    public List<ChartOfAccount> SubAccounts { get; set; } = [];
    public string AccountType { get; set; } = "Asset";
}

public class Bank
{
    public int Id { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public int ChartOfAccountId { get; set; }
    public ChartOfAccount? ChartOfAccount { get; set; }
}

public class BusinessParty
{
    public int Id { get; set; }
    public string PartyType { get; set; } = "Customer";
    public string Name { get; set; } = string.Empty;
    public string Tin { get; set; } = string.Empty;
    public string RegisteredAddress { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string VatType { get; set; } = "VAT";
}

public class JournalEntry
{
    public int Id { get; set; }
    public DateTime TransactionDate { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public List<JournalEntryLine> Lines { get; set; } = [];
}

public class JournalEntryLine
{
    public int Id { get; set; }
    public int JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
    public int ChartOfAccountId { get; set; }
    public ChartOfAccount? ChartOfAccount { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal VatAmount { get; set; }
    public decimal WithholdingTaxAmount { get; set; }
}

public class CashTransaction
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = "IncomeDeposit";
    public decimal Amount { get; set; }
    public int BankId { get; set; }
    public Bank? Bank { get; set; }
    public string Remarks { get; set; } = string.Empty;
}

public class CheckPayment
{
    public int Id { get; set; }
    public string CheckNumber { get; set; } = string.Empty;
    public DateTime CheckDate { get; set; }
    public string Payee { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Printed";
}

public class AuditLog
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityKey { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Changes { get; set; } = string.Empty;
}
