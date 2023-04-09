public abstract class Resource
{
    public string Id { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public virtual object Value { get; set; } = new();
}