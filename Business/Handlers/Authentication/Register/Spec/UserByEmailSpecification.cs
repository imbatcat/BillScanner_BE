using Business.Interfaces.Specifications;
using Domain.Entities;
using System.Linq.Expressions;

namespace Business.Handlers.Authentication.Register.Spec
{
    public class UserByEmailSpecification(string email) : ISpecification<User>
    {
        public Expression<Func<User, bool>>? Criteria { get; } = u => u.Email == email;

        public Expression<Func<User, object>>? OrderBy { get; } = null;

        public Expression<Func<User, object>>? OrderByDescending { get; } = null;

        public Expression<Func<User, object>>? GroupBy { get; } = null;

        public Expression<Func<IGrouping<object, User>, bool>>? Having { get; } = null;

        public Expression<Func<IGrouping<object, User>, User>>? Select { get; } = null;

        public List<Expression<Func<User, object>>> Includes { get; } = new();

        public List<(Expression<Func<User, object>> Expression, bool IsDescending)> ThenByExpressions { get; } = new();

        public List<string> IncludeStrings { get; } = new();

        public int? Top { get; } = null;

        public int Skip { get; } = 0;

        public int Take { get; } = 0;

        public bool IsPagingEnabled { get; } = false;

        public bool IsDistinct { get; } = false;
    }
}