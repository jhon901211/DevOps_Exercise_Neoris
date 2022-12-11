using System;

namespace Entities
{
    /// <summary>
    /// Entity Request
    /// </summary>
    public class RequestMessage
    {
        public string Message { get; set; }
        public string To{ get; set; }
        public string From { get; set; }
        public int TimeToLifeSec { get; set; }
    }
}
