using System;
using System.Net;

namespace NetworkConstants
{
    public static class Raspberry
    {
        private static IPEndPoint address = new IPEndPoint(
            new IPAddress(new byte[]{192,168,68,116}),
            8554);

        public const string USERNAME = "pi";
        public const string PASSWORD = "Orangensaft";
        public static IPEndPoint Address => address;

        public const int VID_HEIGHT = 1000;
        public const int VID_WIDTH = 1000;
        public const int VID_FPS = 15;
    }
}