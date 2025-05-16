using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using DimPos.Catalog.Domain.Entities;

namespace DimPos.Catalog.Infrastructure.Filter.FilterModel;

public class CategoryFilter : IFilter<Categories>, IParsable<CategoryFilter>
{
    public string? Name { get; set; }
    public Expression<Func<Categories, bool>> ToExpression()
    {
        return x =>
            (string.IsNullOrEmpty(Name) || x.Name.Contains(Name));
    }

    public static CategoryFilter Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out var result))
            return result;

        throw new FormatException($"Invalid format for CategoryFilter: '{s}'");
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out CategoryFilter result)
    {
        result = new CategoryFilter();

        if (string.IsNullOrWhiteSpace(s))
            return true; // valid empty filter

        var parameters = s.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var param in parameters)
        {
            var kvp = param.Split('=', 2);
            if (kvp.Length != 2) continue;

            var key = kvp[0].Trim().ToLower();
            var value = kvp[1].Trim();

            if (key == "name")
                result.Name = value;
        }

        return true;
    }
}