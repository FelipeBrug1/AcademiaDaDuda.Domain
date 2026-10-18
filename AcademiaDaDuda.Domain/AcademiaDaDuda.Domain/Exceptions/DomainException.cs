// Felipe Antonio Brüggemann

namespace AcademiaDaDuda.Domain.Exceptions;

public sealed class DomainException(string message) : Exception(message)
{
}