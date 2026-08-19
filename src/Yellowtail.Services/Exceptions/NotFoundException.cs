namespace Yellowtail.Services.Exceptions;

/// <summary>
/// Thrown when a requested entity does not exist.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="message">A message describing which entity was not found.</param>
    public NotFoundException(string message) : base(message)
    {
    }
}
