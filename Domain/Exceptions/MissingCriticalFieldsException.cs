namespace Domain.Exceptions
{
  public class MissingCriticalFieldsException(string[] fieldNames) :
    BusinessException($"Essential extraction field '{string.Join(", ", fieldNames)}' is missing.");
}