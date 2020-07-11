using System;
using System.Collections.Generic;
using System.Text;

namespace DominosLocationMap.Core.Entities.Options
{
   public class RabbitMqOptions
    {
        string HostName { get; }
        string UserName { get; }
        string Password { get; }
        string Port { get; }
        string VirtualHost { get; }
    }
}
