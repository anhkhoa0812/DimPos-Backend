using System.Linq.Expressions;

namespace DimPos.Catalog.Infrastructure.Filter;

public interface IFilter<T>
{
    Expression<Func<T, bool>> ToExpression();
}