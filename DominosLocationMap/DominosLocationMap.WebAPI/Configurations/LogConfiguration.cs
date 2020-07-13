using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DominosLocationMap.WebAPI.Configurations
{
    public static class LogConfiguration
    {
        public static void AddNLogConfig(this IServiceCollection services, string path)
        {
            LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), path));
        }
    }
}
