using System.Text.RegularExpressions;

namespace API.Domain.ValueObjects;

public sealed partial record Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("L'email ne peut pas être vide.", nameof(value));

        var normalized = value.Trim().ToLowerInvariant();

        if (!EmailFormat().IsMatch(normalized))
            throw new ArgumentException($"'{value}' n'est pas une adresse email valide.", nameof(value));

        return new Email(normalized);
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailFormat();
}
