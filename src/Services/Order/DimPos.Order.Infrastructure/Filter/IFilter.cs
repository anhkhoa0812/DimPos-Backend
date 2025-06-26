using System.Linq.Expressions;

namespace DimPos.Order.Infrastructure.Filter;

public interface IFilter<T>
{
    Expression<Func<T, bool>> ToExpression();
}