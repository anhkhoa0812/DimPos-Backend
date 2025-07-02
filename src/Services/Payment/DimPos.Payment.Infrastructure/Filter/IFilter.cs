using System.Linq.Expressions;

namespace DimPos.Payment.Infrastructure.Filter;

public interface IFilter<T>
{
    Expression<Func<T, bool>> ToExpression();
}