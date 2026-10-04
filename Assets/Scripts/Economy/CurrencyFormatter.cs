namespace KingSmash.Economy
{
    public static class CurrencyFormatter
    {
        public static string Format(long amount)
        {
            if (amount < 1000)
                return amount.ToString();

            if (amount < 10000)
            {
                double k = amount / 1000.0;
                return k.ToString("0.0") + "K";
            }

            if (amount < 1000000)
            {
                double k = amount / 1000.0;
                return k.ToString("0.#") + "K";
            }

            double m = amount / 1000000.0;
            return m.ToString("0.0") + "M";
        }

        public static string FormatExact(long amount)
        {
            return amount.ToString("N0");
        }

        public static string FormatWithIcon(long amount)
        {
            return "C" + Format(amount);
        }

        public static string FormatGems(int amount)
        {
            return "G" + Format(amount);
        }
    }
}
