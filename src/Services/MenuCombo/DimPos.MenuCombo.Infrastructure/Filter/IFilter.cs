using System.Linq.Expressions;

namespace DimPos.MenuCombo.Infrastructure.Filter;

public interface IFilter<T>
{
    Expression<Func<T, bool>> ToExpression();
}