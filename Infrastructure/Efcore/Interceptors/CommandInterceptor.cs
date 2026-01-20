using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Infrastructure.Efcore.Interceptors
{
    public class CommandInterceptor(
        ILogger<CommandInterceptor> _logger) : IDbCommandInterceptor
    {
        public Task CommandFailedAsync(
            DbCommand command,
            CommandErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            _logger.LogError(
                "SQL: {Sql}, Error: {Error}",
                command.CommandText,
                eventData.Exception.Message);

            return Task.CompletedTask;
        }
    }
}