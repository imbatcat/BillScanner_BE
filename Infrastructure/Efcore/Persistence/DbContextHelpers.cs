using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Efcore.Persistence
{
    internal class UtcToLocalDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcToLocalDateTimeConverter() : base(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc).ToLocalTime())
        { }
    }
}