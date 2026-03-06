namespace api;
public class ResponseDto<T> where T : class
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}
