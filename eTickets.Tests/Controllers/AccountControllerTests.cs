using eTickets.Controllers;
using eTickets.Data;
using eTickets.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections;
using eTickets.Tests.TestHelpers;

namespace eTickets.Tests
{
    [TestFixture]
    public class AccountControllerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private Mock<AppDbContext> _contextMock;
        private AccountController _controller;

        [SetUp]
        public void Setup()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(_userManagerMock.Object, contextAccessorMock.Object, userPrincipalFactoryMock.Object, null, null, null, null);

            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            _contextMock = new Mock<AppDbContext>(options);

            var users = new List<ApplicationUser>
            {
                new ApplicationUser { UserName = "user1" },
                new ApplicationUser { UserName = "user2" }
            }.AsQueryable();

            var dbSetMock = new Mock<DbSet<ApplicationUser>>();
            dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
            dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
            dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
            dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            // Mock asynchronous behavior using IAsyncEnumerable
            dbSetMock.As<IAsyncEnumerable<ApplicationUser>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<ApplicationUser>(users.GetEnumerator()));

            dbSetMock.As<IQueryable<ApplicationUser>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<ApplicationUser>(users.Provider));

            _contextMock.Setup(c => c.Users).Returns(dbSetMock.Object);

            _controller = new AccountController(_userManagerMock.Object, _signInManagerMock.Object, _contextMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Users_ReturnsViewResult_WithListOfUsers()
        {
            // Act
            var result = await _controller.Users();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult.Model, Is.InstanceOf<List<ApplicationUser>>());
            var model = viewResult.Model as List<ApplicationUser>;
            Assert.That(model.Count, Is.EqualTo(2));
            Assert.That(model[0].UserName, Is.EqualTo("user1"));
            Assert.That(model[1].UserName, Is.EqualTo("user2"));
        }
    }

    // Helper classes for mocking async queries

    //public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    //{
    //    private readonly IEnumerator<T> _enumerator;

    //    public TestAsyncEnumerator(IEnumerator<T> enumerator)
    //    {
    //        _enumerator = enumerator;
    //    }

    //    public T Current => _enumerator.Current;

    //    public ValueTask DisposeAsync() => new ValueTask(Task.CompletedTask);

    //    public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_enumerator.MoveNext());
    //}

    //public class TestAsyncQueryProvider<T> : IAsyncQueryProvider
    //{
    //    private readonly IQueryProvider _inner;

    //    public TestAsyncQueryProvider(IQueryProvider inner)
    //    {
    //        _inner = inner;
    //    }

    //    public IQueryable CreateQuery(Expression expression) => _inner.CreateQuery(expression);

    //    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => _inner.CreateQuery<TElement>(expression);

    //    public object Execute(Expression expression)
    //    {
    //        return _inner.Execute(expression);
    //    }

    //    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

    //    public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    //    {
    //        var result = _inner.Execute<TResult>(expression);
    //        return Task.FromResult(result);
    //    }

    //    TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}



    //public class TestAsyncEnumerable<T> : IAsyncEnumerable<T>, IQueryable<T>
    //{
    //    private readonly IQueryable<T> _queryable;

    //    public TestAsyncEnumerable(IQueryable<T> queryable)
    //    {
    //        _queryable = queryable;
    //    }

    //    // Explicit implementation for IAsyncEnumerable
    //    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
    //        new TestAsyncEnumerator<T>(_queryable.GetEnumerator());

    //    // Explicit implementation for IQueryable
    //    public IEnumerator<T> GetEnumerator() => _queryable.GetEnumerator();

    //    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); // This should work as expected now

    //    public Type ElementType => _queryable.ElementType;

    //    public Expression Expression => _queryable.Expression;

    //    public IQueryProvider Provider => new TestAsyncQueryProvider<T>(_queryable.Provider);
    //}
}
