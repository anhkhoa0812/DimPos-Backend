using System.Linq.Expressions;

namespace DimPos.Notification.Infrastructure.Filter;

public interface IFilter<T>
{
    Expression<Func<T, bool>> ToExpression();
}