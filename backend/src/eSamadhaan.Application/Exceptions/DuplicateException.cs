namespace eSamadhaan.Application.Exceptions;

public class DuplicateException : Exception
{
    public DuplicateException(string message) : base(message)
    {
    }

    public DuplicateException(string entityName, string propertyName, object value) 
        : base($"{entityName} with {propertyName} '{value}' already exists.")
    {
    }
}
