using Microsoft.Xna.Framework;
using StardewValley;

namespace PolyamorySweetRooms
{
    public interface IPolyamorySweetRoomsAPI
    {
        public Point GetSpouseTileOffset(NPC spouse);
        public Point GetSpouseTile(NPC spouse);

        public Point GetSpouseRoomCornerTile(NPC spouse);
    }
}