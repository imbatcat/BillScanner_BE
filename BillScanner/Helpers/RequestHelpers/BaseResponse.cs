namespace BillScanner.Helpers.RequestHelpers
{
    public class BaseResponse<T>(T? data)
    {
        public T? Data { get; set; } = data;
    }
}