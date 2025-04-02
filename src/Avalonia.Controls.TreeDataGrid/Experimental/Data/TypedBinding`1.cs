using System;
using System.Linq.Expressions;
using System.Reflection;
using Avalonia.Data;
using Avalonia.Data.Core.Parsers;

#nullable enable

namespace Avalonia.Experimental.Data
{
    /// <summary>
    /// Provides factory methods for creating <see cref="TypedBinding{TIn, TOut}"/> objects from
    /// C# lambda expressions.
    /// </summary>
    /// <typeparam name="TIn">The input type of the binding.</typeparam>
    public static class TypedBinding<TIn>
        where TIn : class
    {
        public static TypedBinding<TIn, TOut> Default<TOut>(
            Expression<Func<TIn, TOut>> read,
            Action<TIn, TOut> write)
        {
            return new TypedBinding<TIn, TOut>
            {
                Read = read.Compile(),
                Write = write,
                Links = ExpressionChainVisitor<TIn>.Build(read),
                Mode = BindingMode.Default,
            };
        }

        public static TypedBinding<TIn, TOut> OneWay<TOut>(Expression<Func<TIn, TOut>> read)
        {
            return new TypedBinding<TIn, TOut>
            {
                Read = read.Compile(),
                Links = ExpressionChainVisitor<TIn>.Build(read),
            };
        }

        public static TypedBinding<TIn, TOut> TwoWay<TOut>(Expression<Func<TIn, TOut>> expression)
        {
            var property = (expression.Body as MemberExpression)?.Member as PropertyInfo ??
                throw new ArgumentException(
                    $"Cannot create a two-way binding for '{expression}' because the expression does not target a property.",
                    nameof(expression));

            MethodInfo? getMethodInfo = property.GetGetMethod(true);
            if (getMethodInfo is null || getMethodInfo.IsPrivate)
                throw new ArgumentException(
                    $"Cannot create a two-way binding for '{expression}' because the property has no getter or the getter is private.",
                    nameof(expression));

            MethodInfo? setMethodInfo = property.GetSetMethod(true);
            if (setMethodInfo is null || setMethodInfo.IsPrivate)
                throw new ArgumentException(
                    $"Cannot create a two-way binding for '{expression}' because the property has no setter or the setter is private.",
                    nameof(expression));

            // TODO: This is using reflection and mostly untested. Unit test it properly and
            // benchmark it against creating an expression.
            var links = ExpressionChainVisitor<TIn>.Build(expression);
            Action<TIn, TOut> write = links.Length == 1 ?
                (o, v) => property.SetValue(o, v) :
                (root, v) =>
                {
                    // The last link points the object containing the property to set
                    var o = links[^1](root);
                    property.SetValue(o, v);
                };

            return new TypedBinding<TIn, TOut>
            {
                Read = expression.Compile(),
                Write = write,
                Links = links,
                Mode = BindingMode.TwoWay,
            };
        }

        public static TypedBinding<TIn, TOut> TwoWay<TOut>(
            Expression<Func<TIn, TOut>> read,
            Action<TIn, TOut> write)
        {
            return new TypedBinding<TIn, TOut>
            {
                Read = read.Compile(),
                Write = write,
                Links = ExpressionChainVisitor<TIn>.Build(read),
                Mode = BindingMode.TwoWay,
            };
        }

        public static TypedBinding<TIn, TOut> OneTime<TOut>(Expression<Func<TIn, TOut>> read)
        {
            return new TypedBinding<TIn, TOut>
            {
                Read = read.Compile(),
                Links = ExpressionChainVisitor<TIn>.Build(read),
                Mode = BindingMode.OneTime,
            };
        }
    }
}
