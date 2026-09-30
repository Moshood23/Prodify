using System.Globalization;

namespace Prodify.Application.Common.Emails;

public static class Naira
{
    // 32500 -> "N32,500" with the naira sign; kobo only when there are some.
    public static string Format(decimal amount) =>
        "\u20A6" + amount.ToString(amount % 1 == 0 ? "N0" : "N2", CultureInfo.InvariantCulture);
}
