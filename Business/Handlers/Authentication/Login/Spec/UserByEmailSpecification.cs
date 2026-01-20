using Business.Interfaces.Specifications;
using Business.Specifications;
using Domain.Entities;
using System.Linq.Expressions;

namespace Business.Handlers.Authentication.Login.Spec
{
    public class UserByEmailSpecification(string email) : BaseSpecification<User>(x => x.Email.Equals(email))
    {
    }
}