using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models
{
    public class Error
    {
        public ErrorType Type { get; }
        public string Message { get; }
        public DateTime Timestamp { get; }

        public Error(ErrorType type, string message)
        {
            Type = type;
            Message = message;
            Timestamp = DateTime.Now;
        }
    }
}
