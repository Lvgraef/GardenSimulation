public class CoordinateResponse
{
    public bool Success {get; set;}
    public string Message {get; set;}
    public (double, double)[] Coordinates {get; set;}
}