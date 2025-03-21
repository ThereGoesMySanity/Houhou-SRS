using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Kanji.Common.Helpers
{
    public static class LogHelper
    {
        public static readonly ILoggerFactory Factory = LoggerFactory.Create(static builder =>
        {
            builder.AddConsole();
        });
    }
}
