namespace GiveAID.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing a monetary amount in VND.
    /// </summary>
    public sealed class Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency = "VND")
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative.", nameof(amount));

            Amount = amount;
            Currency = currency;
        }

        public static Money Zero => new Money(0);

        public override bool Equals(object? obj)
        {
            if (obj is Money other)
                return Amount == other.Amount && Currency == other.Currency;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Currency);
        }

        public override string ToString() => $"{Amount:N0} {Currency}";

        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Cannot add money with different currencies.");
            return new Money(a.Amount + b.Amount, a.Currency);
        }

        public static Money operator -(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Cannot subtract money with different currencies.");
            return new Money(a.Amount - b.Amount, a.Currency);
        }
    }
}
