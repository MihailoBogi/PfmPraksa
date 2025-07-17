using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Split> Splits { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var directionConverter = new ValueConverter<TransactionDirection, string>(
                v => v == TransactionDirection.Debit ? "d" : "c",
                v => v == "d" ? TransactionDirection.Debit
                     : TransactionDirection.Credit
            );

            var kindConverter = new ValueConverter<TransactionKind, string>(
                v => v == TransactionKind.Deposit ? "dep"
                   : v == TransactionKind.Withdrawal ? "wdw"
                   : v == TransactionKind.Payment ? "pmt"
                   : v == TransactionKind.Fee ? "fee"
                   : v == TransactionKind.InterestCredit ? "inc"
                   : v == TransactionKind.Reversal ? "rev"
                   : v == TransactionKind.Adjustment ? "adj"
                   : v == TransactionKind.LoanDisbursement ? "lnd"
                   : v == TransactionKind.LoanRepayment ? "lnr"
                   : v == TransactionKind.ForeignCurrencyExchange ? "fcx"
                   : v == TransactionKind.AccountOpening ? "aop"
                   : v == TransactionKind.AccountClosing ? "acl"
                   : v == TransactionKind.SplitPayment ? "spl"
                   : "sal",
                v => v == "dep" ? TransactionKind.Deposit
                   : v == "wdw" ? TransactionKind.Withdrawal
                   : v == "pmt" ? TransactionKind.Payment
                   : v == "fee" ? TransactionKind.Fee
                   : v == "inc" ? TransactionKind.InterestCredit
                   : v == "rev" ? TransactionKind.Reversal
                   : v == "adj" ? TransactionKind.Adjustment
                   : v == "lnd" ? TransactionKind.LoanDisbursement
                   : v == "lnr" ? TransactionKind.LoanRepayment
                   : v == "fcx" ? TransactionKind.ForeignCurrencyExchange
                   : v == "aop" ? TransactionKind.AccountOpening
                   : v == "acl" ? TransactionKind.AccountClosing
                   : v == "spl" ? TransactionKind.SplitPayment
                   : TransactionKind.Salary
            );

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transactions");
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Id)
                      .ValueGeneratedNever();

                entity.Property(t => t.BeneficiaryName)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(t => t.Date)
                      .IsRequired();

                entity.Property(t => t.Direction)
                      .HasConversion(directionConverter)
                      .HasMaxLength(1)
                      .IsRequired();

                entity.Property(t => t.Amount)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.Property(t => t.Description)
                      .HasMaxLength(500);

                entity.Property(t => t.Currency)
                      .HasMaxLength(3)
                      .IsRequired();

                entity.Property(t => t.Mcc)
                     .HasColumnName("Mcc")
                     .HasConversion<int?>()
                     .IsRequired(false);

                entity.Property(t => t.Kind)
                      .HasConversion(kindConverter)
                      .HasMaxLength(3)
                      .IsRequired();
                entity.Property(t => t.CatCode)
                      .HasMaxLength(5)
                      .IsUnicode(false);
                entity.HasOne(t => t.Category)
                      .WithMany(c => c.Transactions)
                      .HasForeignKey(t => t.CatCode)
                      .HasPrincipalKey(c => c.Code)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(c => c.Code);

                entity.Property(c => c.Code)
                      .HasMaxLength(5)
                      .IsUnicode(false)
                      .IsRequired()
                      .ValueGeneratedNever();

                entity.Property(c => c.Name)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(c => c.ParentCode)
                      .HasMaxLength(5)
                      .IsUnicode(false);

                // ParentCode -> Code
                entity.HasOne(c => c.Parent)
                  .WithMany(c => c.Children)
                  .HasForeignKey(c => c.ParentCode)
                  .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Split>(b =>
            {
                b.ToTable("Splits");
                b.HasKey(s => s.Id);
                b.Property(s => s.CatCode).HasMaxLength(5).IsRequired();
                b.Property(s => s.Amount).HasColumnType("decimal(18,2)").IsRequired();

                b.HasOne(s => s.Transaction)
                 .WithMany(t => t.Splits)
                 .HasForeignKey(s => s.TransactionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

        }

    }
}
