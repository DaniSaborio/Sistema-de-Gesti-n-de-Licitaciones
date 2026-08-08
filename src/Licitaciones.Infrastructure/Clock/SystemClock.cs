using Licitaciones.Domain.Common;

namespace Licitaciones.Infrastructure.Clock;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
