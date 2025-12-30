using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.Exceptions;

public class InvalidStatusTransitionException : Exception
{
    public InvalidStatusTransitionException(GrievanceStatus from, GrievanceStatus to) 
        : base($"Invalid status transition from '{from}' to '{to}'.")
    {
    }

    public InvalidStatusTransitionException(string message) : base(message)
    {
    }
}
