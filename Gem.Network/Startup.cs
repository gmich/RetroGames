﻿using Gem.Network.Configuration;
using Gem.Network.Utilities.Loggers;
using System;

namespace Gem.Network
{
    public static class Startup
    {

        public static void Setup()
        {
            Debugger.Append = new DebugListener();
            Debugger.Append.RegisterAppender(new Log4NetWrapper("DebugLogger"));

            var config = new ConfigurationReaderXML();
            config.Load("gem.config");

            Dependencies.Setup(config.Dependencies);
        }

    }
}
