using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public enum Response
    {
        Success,
        Failure,
        Ready,
        Connected,
        Error,
        NeedAuthData,
        NeedGuardCode
    }
}
