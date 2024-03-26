using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;

namespace PolyamorySweetRooms

{
    public interface IPolyamorySweetLoveAPI
    {
        public void PlaceSpousesInFarmhouse(FarmHouse farmHouse);
        public Dictionary<string, NPC> GetSpouses(Farmer farmer, bool all = true);

    }
}