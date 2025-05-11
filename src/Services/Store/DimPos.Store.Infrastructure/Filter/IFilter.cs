using System.Linq.Expressions;

namespace DimPos.Store.Infrastructure.Filter;

public interface IFilter<T>
{
    Expression<Func<T, bool>> ToExpression();
}