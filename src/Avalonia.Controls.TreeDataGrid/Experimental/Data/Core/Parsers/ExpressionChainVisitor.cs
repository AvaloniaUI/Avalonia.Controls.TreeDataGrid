using System;
using System.Collections.Generic;
using System.Linq.Expressions;

#nullable enable

namespace Avalonia.Data.Core.Parsers
{
    public class ExpressionChainVisitor<TIn> : ExpressionVisitor
    {
        private readonly LambdaExpression _rootExpression;
        private readonly List<Func<TIn, object>> _links = new();
        private Expression? _head;

        public ExpressionChainVisitor(LambdaExpression expression)
        {
            _rootExpression = expression;
        }

        /// <summary>
        /// Builds an array of delegates which return the intermediate objects in the expression chain.
        /// <example>
        /// For example, if the expression is <c>x => x.Foo.Bar.Baz</c> then the links will be:
        /// <code>
        ///  - x => x
        ///  - x => x.Foo
        ///  - x => x.Foo.Bar
        /// </code>
        /// There is no delegate for the final property of the expression <c>x => x.Foo.Bar.Baz</c>.
        /// </example>
        /// </summary>
    
        public static Func<TIn, object>[] Build<TOut>(Expression<Func<TIn, TOut>> expression)
        {
            var visitor = new ExpressionChainVisitor<TIn>(expression);
            visitor.Visit(expression);
            return visitor._links.ToArray();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var result = base.VisitBinary(node);
            if (node.Left == _head)
                _head = node;
            return result;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var result = base.VisitMember(node);

            if (node.Expression is not null &&
                node.Expression == _head &&
                node.Expression.Type.IsValueType == false)
            {
                var link = Expression.Lambda<Func<TIn, object>>(node.Expression, _rootExpression.Parameters);
                _links.Add(link.Compile());
                _head = node;
            }

            return result;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var result = base.VisitMethodCall(node);

            if (node.Object is not null &&
                node.Object == _head &&
                node.Type.IsValueType == false)
            {
                var link = Expression.Lambda<Func<TIn, object>>(node.Object, _rootExpression.Parameters);
                _links.Add(link.Compile());
                _head = node;
            }

            return result;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == _rootExpression.Parameters[0])
                _head = node;
            return base.VisitParameter(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var result = base.VisitUnary(node);
            if (node.Operand == _head)
                _head = node;
            return result;
        }
    }

}
