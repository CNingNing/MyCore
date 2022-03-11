using System.Linq.Expressions;

namespace Winner.Persistence.Linq
{
    public interface IExpression
    {
        void Translate(Expression expression);
    }
}
