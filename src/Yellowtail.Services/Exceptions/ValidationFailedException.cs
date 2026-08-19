namespace Yellowtail.Services.Exceptions;

/// <summary>
/// Thrown when a business-rule validation check fails (e.g. a referenced entity does not exist).
/// </summary>
public class ValidationFailedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationFailedException"/> class.
    /// </summary>
    /// <param name="message">A message describing the validation failure.</param>
    public ValidationFailedException(string message) : base(message)
    {
    }
}
