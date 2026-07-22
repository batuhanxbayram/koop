using Koop.Entity.Entities;
using Koop.Entity.DTOs.Accounting;
using Koop.Service.Services.AccountingServices;
using Microsoft.AspNetCore.Authorization;
using WebApi.Controllers.Accounting;
using Xunit;

namespace Koop.Tests
{
    public class AccountingLedgerCalculatorTests
    {
        private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        [Fact]
        public void Credit_transaction_increases_balance()
        {
            var ledger = Calculate(Transaction(1, "2026-07-01", AccountingTransactionType.Credit, 250000m));

            Assert.Equal(250000m, ledger.Transactions.Single().RunningBalance);
            Assert.Equal(250000m, ledger.ClosingBalance);
        }

        [Fact]
        public void Debit_transaction_decreases_balance()
        {
            var ledger = Calculate(
                Transaction(1, "2026-06-30", AccountingTransactionType.Credit, 150000m),
                Transaction(2, "2026-07-30", AccountingTransactionType.Debit, 175500m));

            Assert.Equal(150000m, ledger.OpeningBalance);
            Assert.Equal(-25500m, ledger.ClosingBalance);
        }

        [Fact]
        public void Transactions_are_ordered_by_date_by_default()
        {
            var ledger = Calculate(
                Transaction(2, "2026-07-20", AccountingTransactionType.Credit, 10m),
                Transaction(1, "2026-07-01", AccountingTransactionType.Credit, 5m));

            Assert.Equal(new[] { 1L, 2L }, ledger.Transactions.Select(t => t.Id).ToArray());
        }

        [Fact]
        public void Same_date_transactions_are_ordered_stably()
        {
            var firstCreated = new DateTime(2026, 7, 1, 8, 0, 0, DateTimeKind.Utc);
            var secondCreated = firstCreated.AddMinutes(1);

            var ledger = Calculate(
                Transaction(2, "2026-07-30", AccountingTransactionType.Credit, 200m, secondCreated),
                Transaction(1, "2026-07-30", AccountingTransactionType.Credit, 100m, firstCreated));

            Assert.Equal(new[] { 1L, 2L }, ledger.Transactions.Select(t => t.Id).ToArray());
            Assert.Equal(new[] { 100m, 300m }, ledger.Transactions.Select(t => t.RunningBalance).ToArray());
        }

        [Fact]
        public void Previous_month_closing_balance_becomes_opening_balance()
        {
            var ledger = Calculate(
                Transaction(1, "2026-06-30", AccountingTransactionType.Credit, 150000m),
                Transaction(2, "2026-07-30", AccountingTransactionType.Credit, 250000m));

            Assert.Equal(150000m, ledger.OpeningBalance);
            Assert.Equal(400000m, ledger.ClosingBalance);
        }

        [Fact]
        public void Updating_transaction_recalculates_later_running_balances()
        {
            var transactions = new List<AccountingTransaction>
            {
                Transaction(1, "2026-07-01", AccountingTransactionType.Credit, 100m),
                Transaction(2, "2026-07-02", AccountingTransactionType.Debit, 25m),
            };

            transactions[0].Amount = 200m;
            var ledger = Calculate(transactions.ToArray());

            Assert.Equal(new[] { 200m, 175m }, ledger.Transactions.Select(t => t.RunningBalance).ToArray());
        }

        [Fact]
        public void Deleting_transaction_recalculates_later_running_balances()
        {
            var transactions = new List<AccountingTransaction>
            {
                Transaction(1, "2026-07-01", AccountingTransactionType.Credit, 100m),
                Transaction(2, "2026-07-02", AccountingTransactionType.Debit, 25m),
                Transaction(3, "2026-07-03", AccountingTransactionType.Credit, 10m),
            };

            transactions.RemoveAt(1);
            var ledger = Calculate(transactions.ToArray());

            Assert.Equal(new[] { 100m, 110m }, ledger.Transactions.Select(t => t.RunningBalance).ToArray());
        }

        [Fact]
        public void Regular_user_cannot_call_other_users_ledger_endpoint()
        {
            var attribute = GetAuthorizeAttribute(nameof(AccountingController.GetUserLedger));

            Assert.Equal("Admin,Muhasebeci", attribute.Roles);
        }

        [Fact]
        public void Regular_user_cannot_create_update_or_delete_transactions()
        {
            Assert.Equal("Admin,Muhasebeci", GetAuthorizeAttribute(nameof(AccountingController.CreateTransaction)).Roles);
            Assert.Equal("Admin,Muhasebeci", GetAuthorizeAttribute(nameof(AccountingController.UpdateTransaction)).Roles);
            Assert.Equal("Admin,Muhasebeci", GetAuthorizeAttribute(nameof(AccountingController.DeleteTransaction)).Roles);
        }

        [Fact]
        public void Zero_or_negative_amount_is_rejected()
        {
            Assert.NotNull(AccountingLedgerCalculator.ValidateInput(DateTime.Today, "Odeme", AccountingTransactionType.Credit, 0m));
            Assert.NotNull(AccountingLedgerCalculator.ValidateInput(DateTime.Today, "Odeme", AccountingTransactionType.Credit, -1m));
        }

        [Fact]
        public void Invalid_transaction_type_is_rejected()
        {
            Assert.NotNull(AccountingLedgerCalculator.ValidateInput(DateTime.Today, "Odeme", (AccountingTransactionType)99, 1m));
        }

        [Fact]
        public void Closing_balance_uses_opening_credit_and_debit_totals()
        {
            var ledger = Calculate(
                Transaction(1, "2026-06-30", AccountingTransactionType.Credit, 150000m),
                Transaction(2, "2026-07-30", AccountingTransactionType.Credit, 250000m),
                Transaction(3, "2026-07-30", AccountingTransactionType.Credit, 2000m),
                Transaction(4, "2026-07-30", AccountingTransactionType.Debit, 175500m),
                Transaction(5, "2026-07-30", AccountingTransactionType.Debit, 1500m),
                Transaction(6, "2026-07-30", AccountingTransactionType.Debit, 215000m));

            Assert.Equal(150000m, ledger.OpeningBalance);
            Assert.Equal(252000m, ledger.TotalCredit);
            Assert.Equal(392000m, ledger.TotalDebit);
            Assert.Equal(10000m, ledger.ClosingBalance);
        }

        private static AccountingLedgerDto Calculate(params AccountingTransaction[] transactions)
        {
            return AccountingLedgerCalculator.Calculate(
                new AppUser { Id = UserId, FullName = "Test User", UserName = "test" },
                transactions,
                7,
                2026,
                "asc");
        }

        private static AccountingTransaction Transaction(
            long id,
            string date,
            AccountingTransactionType type,
            decimal amount,
            DateTime? createdAt = null)
        {
            return new AccountingTransaction
            {
                Id = id,
                UserId = UserId,
                TransactionDate = DateTime.Parse(date),
                TransactionType = type,
                Amount = amount,
                Description = "Test",
                CreatedAt = createdAt ?? new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(id)
            };
        }

        private static AuthorizeAttribute GetAuthorizeAttribute(string methodName)
        {
            var method = typeof(AccountingController).GetMethod(methodName)
                ?? throw new InvalidOperationException($"Method {methodName} not found.");
            return method.GetCustomAttributes(typeof(AuthorizeAttribute), false)
                .Cast<AuthorizeAttribute>()
                .Single();
        }
    }
}
