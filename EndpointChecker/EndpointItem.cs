using System;

namespace EndpointChecker
{
    public class EndpointItem
    {
        public string Name;
        public string IPaddress;
        public string Platform;
        public bool Success;
        public DateTime StartTime;
        public DateTime EndTime;
        public string ErrorMessage;

        public void Process()
        {

        }
    }
}