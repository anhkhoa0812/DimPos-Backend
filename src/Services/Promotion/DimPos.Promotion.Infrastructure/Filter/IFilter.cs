using System.Linq.Expressions;

namespace DimPos.Promotion.Infrastructure.Filter;

public interface IFilter<T>
{
    Expression<Func<T, bool>> ToExpression();
}