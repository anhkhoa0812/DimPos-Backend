namespace DimPos.Catalog.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException() : base()
    {
        
    }

    public NotFoundException(string message) : base(message)
    {
    }
}