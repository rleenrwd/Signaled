namespace Signaled.Domain.Identity;

public sealed class Role
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    private Role()
    {
    }

    public static Role Create(
        Guid id,
        string code,
        string name)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "A role ID is required.",
                nameof(id));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "A role code is required.",
                nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "A role name is required.",
                nameof(name));
        }

        return new Role
        {
            Id = id,
            Code = code.Trim(),
            Name = name.Trim(),
            IsActive = true
        };
    }
}