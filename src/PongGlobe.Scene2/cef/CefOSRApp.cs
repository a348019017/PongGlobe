using System;
using System.Collections.Generic;
using System.Text;
using Xilium.CefGlue;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace PongGlobe.Scene.cef
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class CefOSRApp:CefApp
    {
        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
            Console.WriteLine("OnBeforeCommandLineProcessing: {0} {1}", processType, commandLine);

            // TODO: currently on linux platform location of locales and pack files are determined
            // incorrectly (relative to main module instead of libcef.so module).
            // Once issue http://code.google.com/p/chromiumembedded/issues/detail?id=668 will be resolved this code can be removed.
            if (CefRuntime.Platform == CefRuntimePlatform.Linux)
            {
                var path = new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath;
                path = Path.GetDirectoryName(path);

                commandLine.AppendSwitch("resources-dir-path", path);
                commandLine.AppendSwitch("locales-dir-path", Path.Combine(path, "locales"));
            }
        }
    }
}
