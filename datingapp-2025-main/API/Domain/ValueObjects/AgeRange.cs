namespace API.Domain.ValueObjects;

public sealed record AgeRange
{
    public const int MinimumAllowedAge = 18;
    public const int MaximumAllowedAge = 100;

    public int Min { get; }
    public int Max { get; }

    private AgeRange(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public static AgeRange Create(int min, int max)
    {
        if (min < MinimumAllowedAge)
            throw new ArgumentOutOfRangeException(nameof(min),
                $"L'âge minimum ne peut pas être inférieur à {MinimumAllowedAge} ans.");

        if (max > MaximumAllowedAge)
            throw new ArgumentOutOfRangeException(nameof(max),
                $"L'âge maximum ne peut pas dépasser {MaximumAllowedAge} ans.");

        if (min > max)
            throw new ArgumentException("L'âge minimum ne peut pas dépasser l'âge maximum.");

        return new AgeRange(min, max);
    }

    public static AgeRange Default => new(MinimumAllowedAge, MaximumAllowedAge);

    /// <summary>
    /// Traduit la tranche d'âge en bornes de date de naissance, du plus ancien (Max ans)
    /// au plus récent (Min ans), relativement à une date de référence.
    /// </summary>
    public (DateOnly MinDateOfBirth, DateOnly MaxDateOfBirth) ToDateOfBirthBounds(DateOnly referenceDate)
    {
        return (referenceDate.AddYears(-Max - 1), referenceDate.AddYears(-Min));
    }
}
