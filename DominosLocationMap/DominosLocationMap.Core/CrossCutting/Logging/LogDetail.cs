using System;
using System.Collections.Generic;

namespace DominosLocationMap.Core.CrossCutting.Logging
{
    public class LogDetail
    {
        public string FullName { get; set; }
        public string MethodName { get; set; }
        public string IpAddress { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public List<LogParameter> Parameters { get; set; }
        public LogReturnValue ReturnValue { get; set; }
        public DateTime LogDate { get; set; } = DateTime.Now;
    }
}