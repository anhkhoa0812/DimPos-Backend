using System.Linq.Expressions;

namespace DimPos.Identity.Infrastructure.Filter;

public interface IFilter<T>
{
    Expression<Func<T, bool>> ToExpression();
}