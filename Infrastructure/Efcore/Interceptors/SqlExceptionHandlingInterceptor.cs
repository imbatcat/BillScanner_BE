using EntityFramework.Exceptions.Common;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.Efcore.Interceptors
{
    /// <summary>
    /// Converts PostgreSQL exceptions to typed exceptions AND logs them
    /// </summary>
    public partial class SqlExceptionHandlingInterceptor(
        ILogger<SqlExceptionHandlingInterceptor> _logger) : ExceptionProcessorInterceptor<NpgsqlException>
    {
        // PostgreSQL error codes
        private const string UNIQUE_VIOLATION = "23505";

        private const string FOREIGN_KEY_VIOLATION = "23503";

        private const string NOT_NULL_VIOLATION = "23502";

        private const string STRING_DATA_RIGHT_TRUNCATION = "22001";

        private const string NUMERIC_VALUE_OUT_OF_RANGE = "22003";

        protected override DatabaseError? GetDatabaseError(NpgsqlException dbException)
        {
            // Map PostgreSQL error codes to DatabaseError enum
            var databaseError = dbException.SqlState switch
            {
                UNIQUE_VIOLATION => DatabaseError.UniqueConstraint,
                FOREIGN_KEY_VIOLATION => DatabaseError.ReferenceConstraint,
                NOT_NULL_VIOLATION => DatabaseError.CannotInsertNull,
                STRING_DATA_RIGHT_TRUNCATION => DatabaseError.MaxLength,
                NUMERIC_VALUE_OUT_OF_RANGE => DatabaseError.NumericOverflow,
                _ => (DatabaseError?)null
            };

            // Log the error DURING conversion (before it becomes a typed exception)
            if (databaseError.HasValue)
            {
                LogDatabaseError(databaseError.Value, dbException);
            }

            return databaseError;
        }

        private void LogDatabaseError(DatabaseError error, NpgsqlException exception)
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