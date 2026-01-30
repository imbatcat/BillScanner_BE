namespace Domain.Exceptions
{
  public class ScanRetryRequiredException() :
    BusinessException("Scan retry required.");
}