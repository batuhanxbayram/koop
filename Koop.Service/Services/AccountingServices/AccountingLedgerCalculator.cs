using Koop.Entity.DTOs.Accounting;
using Koop.Entity.Entities;

namespace Koop.Service.Services.AccountingServices
{
    public static class AccountingLedgerCalculator
    {
        public static AccountingLedgerDto Calculate(
            AppUser user,
            IEnumerable<AccountingTransaction> sourceTransactions,
            int periodMonth,
            int periodYear,
            string? sort)
        {
            var periodStart = new DateTime(periodYear, periodMonth, 1);
            var periodEnd = periodStart.AddMonths(1);
            var normalizedSort = string.Equals(sort, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

            var orderedTransactions = sourceTransactions
                .OrderBy(t => t.TransactionDate)
                .ThenBy(t => t.CreatedAt)
                .ThenBy(t => t.Id)
                .ToList();

            var openingBalance = orderedTransactions
                .Where(t => t.TransactionDate < periodStart)
                .Sum(GetEffect);

            var runningBalance = openingBalance;
            var periodTransactions = orderedTransactions
                .Where(t => t.TransactionDate >= periodStart && t.TransactionDate < periodEnd)
                .Select(transaction =>
                {
                    runningBalance += GetEffect(transaction);
                    return ToDto(transaction, runningBalance);
                })
                .ToList();

            var totalCredit = periodTransactions.Sum(t => t.CreditAmount);
            var totalDebit = periodTransactions.Sum(t => t.DebitAmount);

            if (normalizedSort == "desc")
            {
                periodTransactions = periodTransactions
                    .OrderByDescending(t => t.Date)
                    .ThenByDescending(t => t.CreatedAt)
                    .ThenByDescending(t => t.Id)
                    .ToList();
            }

            return new AccountingLedgerDto
            {
                UserId = user.Id,
                UserFullName = user.FullName,
                UserName = user.UserName,
                PeriodMonth = periodMonth,
                PeriodYear = periodYear,
                OpeningBalance = Math.Round(openingBalance, 2),
                TotalCredit = Math.Round(totalCredit, 2),
                TotalDebit = Math.Round(totalDebit, 2),
                ClosingBalance = Math.Round(openingBalance + totalCredit - totalDebit, 2),
                Transactions = periodTransactions
            };
        }

        public static string? ValidateInput(DateTime transactionDate, string? description, AccountingTransactionType type, decimal amount)
        {
            if (transactionDate == default)
            {
                return "Tarih zorunludur.";
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                return "Aciklama zorunludur.";
            }

            if (!Enum.IsDefined(typeof(AccountingTransactionType), type))
            {
                return "Islem turu gecersiz.";
            }

            if (amount <= 0)
            {
                return "Tutar sifirdan buyuk olmalidir.";
            }

            return null;
        }

        private static decimal GetEffect(AccountingTransaction transaction)
        {
            return transaction.TransactionType == AccountingTransactionType.Credit
                ? transaction.Amount
                : -transaction.Amount;
        }

        private static AccountingTransactionDto ToDto(AccountingTransaction transaction, decimal runningBalance)
        {
            return new AccountingTransactionDto
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                Date = transaction.TransactionDate,
                Description = transaction.Description,
                Type = transaction.TransactionType,
                TypeName = transaction.TransactionType.ToString(),
                Amount = transaction.Amount,
                CreditAmount = transaction.TransactionType == AccountingTransactionType.Credit ? transaction.Amount : 0,
                DebitAmount = transaction.TransactionType == AccountingTransactionType.Debit ? transaction.Amount : 0,
                RunningBalance = Math.Round(runningBalance, 2),
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt
            };
        }
    }
}
