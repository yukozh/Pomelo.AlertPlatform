namespace Pomelo.AlertPlatform.DeviceClient
{
    public class Response<T>
    {
        public int code { get; set; }

        public string msg { get; set; }

        public T data { get; set; }
    }
}
