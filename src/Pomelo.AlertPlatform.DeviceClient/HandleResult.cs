namespace Pomelo.AlertPlatform.DeviceClient
{
    public class HandleResult
    {
        public bool IsSucceeded { get; set; }
        public bool IsUserError { get; set; }
        public string Error { get; set; }
    }
}
