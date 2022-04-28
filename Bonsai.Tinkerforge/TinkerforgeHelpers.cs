using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Tinkerforge
{
    public class TinkerforgeHelpers
    {

        public static class TinkerForgeNameLookup
        {
            public static Dictionary<int, string> Defaults = new Dictionary<int, string>
            {
                { 13,  "Master Brick"},
                { 2131, "Ambient Light Bricklet 3.0"},
                { 2147, "CO2 Bricklet 2.0" }
            };
        }

    }
}
