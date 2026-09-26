namespace RideHailingAPI.DTOs;

public class ApiReponse
{
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public object? Data { get; set; }
}