namespace Organization.Application.Exceptions;

public sealed class OrganizationNotFoundException(string message) : Exception(message)
{
}

public sealed class OrganizationConflictException(string message) : Exception(message)
{
}

public sealed class OrganizationValidationException(string message) : Exception(message)
{
}
