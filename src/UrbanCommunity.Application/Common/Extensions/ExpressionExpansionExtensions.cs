using System.Linq.Expressions;
using System.Reflection;

namespace UrbanCommunity.Application.Common.Extensions;

public static class ExpressionExpansionExtensions
{
    private static readonly MethodInfo InvokeMethod = typeof(ExpressionExpansionExtensions)
        .GetMethod(nameof(Invoke))!;

    /// <summary>
    /// Marks a reusable expression inside a query. <see cref="Expand{T}"/> inlines its body
    /// so the provider can translate it; it never runs on its own.
    /// </summary>
    public static TResult Invoke<T, TResult>(this Expression<Func<T, TResult>> expression, T argument)
        => throw new InvalidOperationException(
            $"{nameof(Invoke)} must be expanded. Call {nameof(Expand)}() on the query before executing it.");

    /// <summary>Inlines every <see cref="Invoke{T, TResult}"/> call in the query expression.</summary>
    public static IQueryable<T> Expand<T>(this IQueryable<T> query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return query.Provider.CreateQuery<T>(new InvokeExpander().Visit(query.Expression));
    }

    private sealed class InvokeExpander : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!node.Method.IsGenericMethod || node.Method.GetGenericMethodDefinition() != InvokeMethod)
                return base.VisitMethodCall(node);

            var lambda = Evaluate(node.Arguments[0]);
            var argument = Visit(node.Arguments[1]);
            var body = new ParameterReplacer(lambda.Parameters[0], argument).Visit(lambda.Body);
            return Visit(body);
        }

        private static LambdaExpression Evaluate(Expression expression) => expression switch
        {
            MemberExpression { Member: FieldInfo { IsStatic: true } field } => (LambdaExpression)field.GetValue(null)!,
            _ => (LambdaExpression)Expression.Lambda(expression).Compile().DynamicInvoke()!
        };
    }

    private sealed class ParameterReplacer(ParameterExpression parameter, Expression replacement) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == parameter ? replacement : base.VisitParameter(node);
    }
}
