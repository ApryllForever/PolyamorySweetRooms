using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Netcode;
using StardewValley;
using StardewValley.Network;

namespace PolyamorySweetRooms
{
    public static class Farmer_listWives
    {
        internal class Holder { public HashSet<string> Value = new(); }

        internal static ConditionalWeakTable<Farmer, Holder> values = new();

        public static void set_haslistWives(this Farmer farmer, HashSet<string> newVal)
        {

        }

        public static HashSet<string> haslistWives(this Farmer farmer)
        {
            var holder = values.GetOrCreateValue(farmer);
            return holder.Value;
        }
    }
}