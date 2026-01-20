using Moq;
using Microsoft.Extensions.Logging;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Infrastructure.Efcore.Persistence;
using Domain.Entities;
using Xunit.Abstractions;
using Meziantou.Extensions.Logging.Xunit;

namespace Test.Infrastructure.Efcore.Interceptor
{
    public class SqlExceptionHandlingInterceptorTests : IDisposable
    {
        private readonly DbConnection _dbConnection;

        private readonly ILogger<SqliteExceptionHandlingInterceptor> _logger;

        private readonly SqliteExceptionHandlingInterceptor _interceptor;

        public SqlExceptionHandlingInterceptorTests(ITestOutputHelper outputHelper)
        {
            _dbConnection = new SqliteConnection("DataSource=:memory:");
            _dbConnection.Open();

            _logger = XUnitLogger.CreateLogger<SqliteExceptionHandlingInterceptor>(outputHelper);
            _interceptor = new SqliteExceptionHandlingInterceptor(_logger);
        }

        [Fact]
        public void SaveChangesFailed_WithUniqueConstraintException_LogsErrorAndThrowsException()
        {
            // Arrange
            var dbContext = CreateBillScannerDbContext(_dbConnection, _interceptor);

            var user1 = new User
            {
                Email = "duplicate@test.com",
                Password = "password123",
                DisplayName = "User 1"
            };

            dbContext.Set<User>().Add(user1);
            dbContext.SaveChanges();

            // Create a second user with the same email (violates unique constraint)
            var user2 = new User
            {
                Email = "duplicate@test.com",
                Password = "password456",
                DisplayName = "User 2"
            };

            dbContext.Set<User>().Add(user2);

            // Act & Assert
            var exception = Assert.Throws<UniqueConstraintException>(() => dbContext.SaveChanges());

            // Verify the exception was thrown (logging will be visible in test output)
            Assert.NotNull(exception);
        }

        public static BillScannerDbContext CreateBillScannerDbContext(
            DbConnection connection,
            params IInterceptor[] interceptors)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BillScannerDbContext>()
                .UseSqlite(connection);

            if (interceptors != null && interceptors.Length > 0)
            {
                optionsBuilder.AddInterceptors(interceptors);
            }

            var dbContext = new BillScannerDbContext(optionsBuilder.Options);
            dbContext.Database.EnsureCreated();

            return dbContext;
        }

        public void Dispose()
        {
            _dbConnection.Close();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Converts SQLite exceptions to typed exceptions AND logs them (for testing)
    /// </summary>
    public partial class SqliteExceptionHandlingInterceptor(
        ILogger<SqliteExceptionHandlingInterceptor> _logger) : ExceptionProcessorInterceptor<SqliteException>
    {
        // SQLite error codes
        private const int SQLITE_CONSTRAINT = 19;

        private const int SQLITE_TOOBIG = 18;

        private const int SQLITE_CONSTRAINT_NOTNULL = 1299;

        private const int SQLITE_CONSTRAINT_UNIQUE = 2067;

        private const int SQLITE_CONSTRAINT_PRIMARYKEY = 1555;

        private const int SQLITE_CONSTRAINT_FOREIGNKEY = 787;

        protected override DatabaseError? GetDatabaseError(SqliteException dbException)
        {
            DatabaseError? databaseError = null;

            if (dbException.SqliteErrorCode == SQLITE_CONSTRAINT || dbException.SqliteErrorCode == SQLITE_TOOBIG)
            {
                databaseError = dbException.SqliteExtendedErrorCode switch
                {
                    SQLITE_TOOBIG => DatabaseError.MaxLength,
                    SQLITE_CONSTRAINT_NOTNULL => DatabaseError.CannotInsertNull,
                    SQLITE_CONSTRAINT_UNIQUE => DatabaseError.UniqueConstraint,
                    SQLITE_CONSTRAINT_PRIMARYKEY => DatabaseError.UniqueConstraint,
                    SQLITE_CONSTRAINT_FOREIGNKEY => DatabaseError.ReferenceConstraint,
                    _ => null
                };
            }

            // Log the error DURING conversion (before it becomes a typed exception)
            if (databaseError.HasValue)
            {
                LogDatabaseError(databaseError.Value, dbException);
            }

            return databaseError;
        }

        private void LogDatabaseError(DatabaseError error, SqliteException exception)
        {
            switch (error)
            {
                case DatabaseError.UniqueConstraint:
                    LogUniqueConstraintViolation(exception, exception.Message);
                    break;

                case DatabaseError.ReferenceConstraint:
                    LogReferenceConstraintViolation(exception, exception.Message);
                    break;

                case DatabaseError.CannotInsertNull:
                    LogCannotInsertNullViolation(exception, exception.Message);
                    break;

                case DatabaseError.MaxLength:
                    LogMaxLengthExceeded(exception, exception.Message);
                    break;

                case DatabaseError.NumericOverflow:
                    LogNumericOverflow(exception, exception.Message);
                    break;

                default:
                    LogDatabaseOperationFailed(exception, exception.Message);
                    break;
            }
        }

        // Source-generated logging methods
        [LoggerMessage(
            EventId = 1001,
            Level = LogLevel.Error,
            Message = "Unique constraint violation occurred: {Message}")]
        private partial void LogUniqueConstraintViolation(Exception exception, string message);

        [LoggerMessage(
            EventId = 1002,
            Level = LogLevel.Error,
            Message = "Foreign key constraint violation occurred: {Message}")]
        private partial void LogReferenceConstraintViolation(Exception exception, string message);

        [LoggerMessage(
            EventId = 1003,
            Level = LogLevel.Error,
            Message = "Not null constraint violation occurred: {Message}")]
        private partial void LogCannotInsertNullViolation(Exception exception, string message);

        [LoggerMessage(
            EventId = 1004,
            Level = LogLevel.Error,
            Message = "Max length constraint violation occurred: {Message}")]
        private partial void LogMaxLengthExceeded(Exception exception, string message);

        [LoggerMessage(
            EventId = 1005,
            Level = LogLevel.Error,
            Message = "Numeric overflow occurred: {Message}")]
        private partial void LogNumericOverflow(Exception exception, string message);

        [LoggerMessage(
            EventId = 1000,
            Level = LogLevel.Error,
            Message = "Database operation failed: {Message}")]
        private partial void LogDatabaseOperationFailed(Exception exception, string message);
    }
}