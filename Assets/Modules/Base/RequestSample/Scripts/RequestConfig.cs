using System;

namespace Modules.Base.RequestSampleModule.Scripts
{
    /// <summary>
    /// Configuration data for HTTP requests
    /// </summary>
    [Serializable]
    public class RequestConfig
    {
        public string requestUrl = "https://example.com";
        public int timeout = 10;
        public string description = "";
    }
}

