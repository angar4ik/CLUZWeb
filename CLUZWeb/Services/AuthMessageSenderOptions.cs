using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CLUZWeb.Services
{
    public class AuthMessageSenderOptions
    {
        public string SendGridUser { get; set; } = "Do Not Reply";
        public string SendGridKey { get; set; } = "";
    }
}
