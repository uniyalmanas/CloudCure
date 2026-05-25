using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore
{
    public static class DbContextExtensions
    {
        public static T UpdateGraph<T>(this DbContext context, T entity, Expression<Func<IUpdateConfiguration<T>, object>> mapping = null) where T : class
        {
            if (entity == null) return null;

            // In EF Core, context.Update(entity) automatically tracks the entity and its reachable
            // graph, marking child entities as Added or Modified depending on their key values.
            context.Update(entity);
            return entity;
        }
    }

    public interface IUpdateConfiguration<T>
    {
        IUpdateConfiguration<T> OwnedCollection<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> memberExpression) where TProperty : class;
        IUpdateConfiguration<T> OwnedEntity<TProperty>(Expression<Func<T, TProperty>> memberExpression) where TProperty : class;
        IUpdateConfiguration<T> AssociatedCollection<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> memberExpression) where TProperty : class;
        IUpdateConfiguration<T> AssociatedEntity<TProperty>(Expression<Func<T, TProperty>> memberExpression) where TProperty : class;
    }
}
