using FluentValidation;

namespace Prodify.Application.Reports.Queries.GetSalesChart;

public class GetSalesChartQueryValidator : AbstractValidator<GetSalesChartQuery>
{
    public static readonly int[] DayOptions = { 7, 30, 90 };

    public GetSalesChartQueryValidator()
    {
        RuleFor(x => x.Days)
            .Must(days => DayOptions.Contains(days))
            .WithMessage("Days must be 7, 30 or 90.");
    }
}
