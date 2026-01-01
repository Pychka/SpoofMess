namespace DataHelpers;

public abstract class IdentifiedEntity<TKey> : IIdentifiedEntity, ISoftDeletable
{
    public TKey Id { get; set; } = default!;

    public bool IsDeleted { get; set; }

    public string GetId() => $"{Id}";
}