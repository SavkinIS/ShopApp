namespace ShopApp.Models;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public T Data { get; set; }
}