using System;
using System.Net;

namespace NetworkConstants
{
    public static class Raspberry
    {
        private static IPAddress address = new IPAddress(new byte[]{192,168,68,116});

        public const string USERNAME = "pi";
        public const string PASSWORD = "Hubertus7362";
        public static IPAddress => address
        

        static void foo()
        {
            var p = new IPAddress(address);
        }
    }
}