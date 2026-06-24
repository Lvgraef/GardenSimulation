namespace Model
{
    public class Response
    {
        public (double, double)[] ParcelCoordinates {get; set;}
        public (double, double)[] PandCoordinates {get; set;}
        public string Message {get; set;}
        public bool Success {get; set;}
    }
}