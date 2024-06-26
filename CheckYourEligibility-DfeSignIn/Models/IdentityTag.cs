namespace CheckYourEligibility_DfeSignIn.Models;

public sealed class IdentityTag<T> where T : struct
{
    public T Id { get; set; }

    public string Name { get; set; } = null!;
}
