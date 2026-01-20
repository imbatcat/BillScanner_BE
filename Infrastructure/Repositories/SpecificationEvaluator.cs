using Business.Interfaces.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    internal static class SpecificationEvaluator<T> where T : class
    {
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification)
        {
            var query = inputQuery;

            // Modify the IQueryable using the specification's criteria expression
            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            // Includes all expression-based includes
            query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

            // Include any string-based include statements
            query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

            // Apply ordering if expressions are specified
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            // Apply ThenBy ordering
            if (specification.ThenByExpressions.Any())
            {
                IOrderedQueryable<T>? orderedQuery = query as IOrderedQueryable<T>;
                foreach (var thenBy in specification.ThenByExpressions)
                {
                    if (orderedQuery != null)
                    {
                        orderedQuery = thenBy.IsDescending
                            ? orderedQuery.ThenByDescending(thenBy.Expression)
                            : orderedQuery.ThenBy(thenBy.Expression);
                    }
                }
                query = orderedQuery ?? query;
            }

            // Apply grouping if expression is specified
            if (specification.GroupBy != null)
            {
                var groupedQuery = query.GroupBy(specification.GroupBy);

                // Apply having clause if specified
                if (specification.Having != null)
                {
                    groupedQuery = groupedQuery.Where(specification.Having);
                }

                // Apply select clause if specified
                if (specification.Select != null)
                {
                    query = groupedQuery.Select(specification.Select);
                }
                else
                {
                    // If no select is specified, flatten the groups back to T
                    query = groupedQuery.SelectMany(g => g);
                }
            }

            // Apply top if specified
            if (specification.Top.HasValue)
            {
                query = query.Take(specification.Top.Value);
            }

            // Apply paging if enabled
            if (specification.Skip > 0)
            {
                query = query.Skip(specification.Skip);
            }

            if (specification.Take > 0)
            {
                query = query.Take(specification.Take);
            }

            return query;
        }
    }
}
