using System;
using System.Diagnostics;

namespace DebugApi
{
    public static class Logger
    {
        public static void Log(string param)
        {
            Debug.WriteLine("Log : " + DateTime.Now.ToString("hh.mm.ss.ffffff") + " - " + param);
            Console.WriteLine(DateTime.Now.ToString("hh.mm.ss.ffffff") + " - " + param);
        }
    }
}