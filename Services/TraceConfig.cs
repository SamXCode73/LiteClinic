using System.Diagnostics;

namespace LiteClinic.Services
{
    public static class TraceConfig
    {
        public static void Configure()
        {
            if (!EventLog.SourceExists("LiteClinic"))
            {
                EventLog.CreateEventSource("LiteClinic", "Application");
            }

            Trace.Listeners.Add(new EventLogTraceListener("LiteClinic"));
        }
    }
}