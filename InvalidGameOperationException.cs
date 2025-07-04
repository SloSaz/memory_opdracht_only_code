namespace Business.Exceptions;
/// <summary>
/// Exception thrown when an invalid game operation is attempted
/// </summary>
public class InvalidGameOperationException(string message) : Exception(message);