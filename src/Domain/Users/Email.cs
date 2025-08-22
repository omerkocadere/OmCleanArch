namespace CleanArch.Domain.Users;

public sealed record Email
{
    private const int MaxLength = 320;

    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Email> Create(string email) =>
        Result
            .Create(email)
            .Ensure(e => !string.IsNullOrWhiteSpace(e), UserErrors.EmailEmpty)
            .Ensure(e => e.Length <= MaxLength, UserErrors.EmailTooLong)
            .Ensure(e => e.Split('@').Length == 2, UserErrors.EmailInvalidFormat)
            .Map(e => new Email(e.ToLower()));

    public static implicit operator string(Email email) => email.Value;
}
