namespace Domain.Exceptions
{
  public abstract class BusinessException(
    string message,
    Exception? innerException = null) : Exception(
    message,
    innerException);
}