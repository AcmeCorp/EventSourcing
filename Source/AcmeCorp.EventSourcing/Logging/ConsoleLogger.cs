namespace AcmeCorp.EventSourcing.Logging
{
    using System;

    public class ConsoleLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }
    }
}