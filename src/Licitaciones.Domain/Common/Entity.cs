namespace Licitaciones.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset UpdatedAt { get; protected set; }

    protected void Touch(IClock clock) => UpdatedAt = clock.UtcNow;
}

public abstract class SoftDeletableEntity : Entity
{
    public DateTimeOffset? DeletedAt { get; protected set; }
    public bool EstaEliminado => DeletedAt is not null;

    public void EliminarLogicamente(IClock clock)
    {
        DeletedAt = clock.UtcNow;
        Touch(clock);
    }
}
