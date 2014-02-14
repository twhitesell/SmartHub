using System;
using Microsoft.SPOT;

namespace BusNetwork.Network
{
    public class BusModuleName
    {
        public static string Get(byte type)
        {
            switch (type)
            {
                case 0: return "AE test full module";
                case 1: return "AE-R8";


                default: return type.ToString() + " [Unknown]";
            }
        }
    }
}
