namespace ChatRPG.Domain.Exceptions;

public abstract class DomainException(string? message = null) : Exception(message);
