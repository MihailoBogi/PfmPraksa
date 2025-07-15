using System;
using System.Runtime.Serialization;

namespace PersonalFinance.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; private set; }

        public string BeneficiaryName { get; private set; } = string.Empty;
        public DateTime Date { get; private set; }
        public TransactionDirection Direction { get; set; }
        public decimal Amount { get; private set; }
        public string Description { get; private set; } = default!;
        public string Currency { get; private set; } = default!;
        public int? Mcc { get; private set; }
        public TransactionKind Kind { get; private set; }
        public string? CatCode { get; private set; }
        public Category? Category { get; private set; }


        private Transaction() { }

        // konstruktor koji mapira CSV na entitet
        public Transaction(
            int id,
            string? beneficiaryName,
            DateTime date,
            TransactionDirection direction,
            decimal amount,
            string description,
            string currency,
            int? mcc,
            TransactionKind kind,
            string? catCode = null)
        {
            Id = id;
            BeneficiaryName = beneficiaryName
                ?? string.Empty;
            Date = date;
            Direction = direction;
            Amount = amount;
            Description = description ?? string.Empty;
            Currency = currency ?? throw new ArgumentNullException(nameof(currency));
            Mcc = mcc;
            Kind = kind;
            CatCode = catCode;
        }

        public void Categorize(string? catCode)
        {
            CatCode = string.IsNullOrWhiteSpace(catCode) ? null : catCode;
        }
    }

    public enum TransactionDirection
    {
        [EnumMember(Value = "d")] Debit = 0,
        [EnumMember(Value = "c")] Credit = 1
    }
    public enum TransactionKind
    {
        [EnumMember(Value = "dep")] Deposit = 0,
        [EnumMember(Value = "wdw")] Withdrawal = 1,
        [EnumMember(Value = "pmt")] Payment = 2,
        [EnumMember(Value = "fee")] Fee = 3,
        [EnumMember(Value = "inc")] InterestCredit = 4,
        [EnumMember(Value = "rev")] Reversal = 5,
        [EnumMember(Value = "adj")] Adjustment = 6,
        [EnumMember(Value = "lnd")] LoanDisbursement = 7,
        [EnumMember(Value = "lnr")] LoanRepayment = 8,
        [EnumMember(Value = "fcx")] ForeignCurrencyExchange = 9,
        [EnumMember(Value = "aop")] AccountOpening = 10,
        [EnumMember(Value = "acl")] AccountClosing = 11,
        [EnumMember(Value = "spl")] SplitPayment = 12,
        [EnumMember(Value = "sal")] Salary = 13
    }
}
