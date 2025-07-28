using System.Linq.Expressions;

namespace DimPos.Inventory.Infrastructure.Filter;

public interface IFilter<T>
{
    Expression<Func<T, bool>> ToExpression();
}