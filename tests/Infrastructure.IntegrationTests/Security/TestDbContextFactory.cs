using GiveAID.Application.Common.Interfaces;
using GiveAID.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GiveAID.Tests.Unit.Infrastructure.Security;

/// <summary>
/// Factory for creating mock IApplicationDbContext for testing.
/// </summary>
public static class TestDbContextFactory
{
    public static IApplicationDbContext CreateMock(List<User>? users = null)
    {
        var mockContext = new Mock<IApplicationDbContext>();
        var userList = users ?? new List<User>();
        
        // Create a mock DbSet that works with LINQ extension methods
        var mockUserDbSet = CreateMockDbSet(userList.AsQueryable());
        mockContext.Setup(c => c.Users).Returns(mockUserDbSet.Object);
        
        return mockContext.Object;
    }
    
    private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        
        // Setup IQueryable implementation
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(data.Provider);
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Expression)
            .Returns(data.Expression);
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.ElementType)
            .Returns(data.ElementType);
        mockSet.As<IQueryable<T>>()
            .Setup(m => m.GetEnumerator())
            .Returns(data.GetEnumerator());
        
        return mockSet;
    }
}
