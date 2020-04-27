using System;

namespace EndpointChecker
{
    public class EndpointItem
    {
        public string Name;
        public string IPaddress;
        public string Platform;
        public bool Success;
        public string[] Error;
        public DateTime StartTime;
        public DateTime EndTime;
    }
}