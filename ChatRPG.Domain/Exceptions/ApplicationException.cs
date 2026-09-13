namespace ChatRPG.Domain.Exceptions;

public abstract class ApplicationException(string? message = null) : Exception(message);
