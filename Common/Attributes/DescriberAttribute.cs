namespace GroupManager.Common.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
internal sealed class DescriberAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }
    public string? Parameters { get; }

    public DescriberAttribute(string name, string description, string? parameters)
    {
        Name = name;
        Description = description;
        Parameters = parameters;
    }
}