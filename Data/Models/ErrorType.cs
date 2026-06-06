using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models
{
    public enum ErrorType
    {
        None,
        ModelIsNull,
        IncorrectId,
        OuterLibraryError,
        CommunicationError,
        SandboxError,
        SystemError,
        AgentError,
        AgentPixelBotError,
        TimedOut,
    }
}
