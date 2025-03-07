using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using StardewModdingAPI.Events;
using SpaceShared.APIs;
using System.Reflection.Emit;
using System.Reflection;
using StardewValley.GameData.Characters;
using StardewValley.Objects;
using Netcode;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewModdingAPI.Enums;


namespace PolyamorySweetRooms
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        int Goat = 0;
        
        public static string dictPath = "ApryllForever.PolyamorySweetRooms/dict";

        public static List<string> JustEngagedList = new();

        public static Dictionary<string, int> roomIndexes = new Dictionary<string, int>{
            { "Abigail", 0 },
            { "Penny", 1 },
            { "Leah", 2 },
            { "Haley", 3 },
            { "Maru", 4 },
            { "Sebastian", 5 },
            { "Alex", 6 },
            { "Harvey", 7 },
            { "Elliott", 8 },
            { "Sam", 9 },
            { "Shane", 10 },
            { "Emily", 11 },
            { "Krobus", 12 },
        };

        public static Dictionary<string, SpouseRoomData> customRoomData = new Dictionary<string, SpouseRoomData>();
        public static Dictionary<string, SpouseRoomData> currentRoomData = new Dictionary<string, SpouseRoomData>();
        public static Dictionary<string, SpouseRoomData> currentIslandRoomData = new Dictionary<string, SpouseRoomData>();

        
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();

            SMonitor = Monitor;
            SHelper = helper;

            var harmony = new Harmony(ModManifest.UniqueID);

            // Location patches

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), "resetLocalState"),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.FarmHouse_resetLocalState_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.checkAction)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.FarmHouse_checkAction_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.loadSpouseRoom)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.FarmHouse_loadSpouseRoom_Prefix))
            );
           // harmony.Patch(

            //orig purpose of this patch was to bypass showspouseroom, it seems. I am using show spouse room. Let's murder this patch.

           //    original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.updateFarmLayout)),
           //    prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.FarmHouse_updateFarmLayout_Prefix))
           // );
            harmony.Patch(
               original: AccessTools.Method(typeof(DecoratableLocation), nameof(DecoratableLocation.MakeMapModifications)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.DecoratableLocation_MakeMapModifications_Postfix))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(DecoratableLocation), "IsFloorableOrWallpaperableTile", new Type[] { typeof(int), typeof(int),typeof(string) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.DecoratableLocation_IsFloorableOrWallpaperableTile_Prefix))
            );
            

            // NetWorldState patch 

            harmony.Patch(
               original: AccessTools.Method(typeof(NetWorldState), nameof(NetWorldState.hasWorldStateID)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.hasWorldStateID_Prefix))
            );

            harmony.Patch(
              original: AccessTools.DeclaredMethod(typeof(FarmHouse), nameof(FarmHouse.showSpouseRoom)),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.explode1_Transpiler)));


            harmony.Patch(
              original: AccessTools.DeclaredMethod(typeof(FarmHouse), nameof(FarmHouse.updateFarmLayout)),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.explode_Transpiler)));

            harmony.Patch(
               original: AccessTools.DeclaredMethod(typeof(FarmHouse), nameof(FarmHouse.showSpouseRoom)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.showSpouseRoom_Prefix)));

            harmony.Patch(
              original: AccessTools.DeclaredMethod(typeof(NPC), "engagementResponse"),
              postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_engagementResponse_Postfix))
           );


            SHelper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            SHelper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            SHelper.Events.Content.AssetRequested += Content_AssetRequested;
            SHelper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            SHelper.Events.GameLoop.DayStarted += OnDayStarted;
            SHelper.Events.Specialized.LoadStageChanged += OnLoadStateChanged;
            
        }

        private static void NPC_engagementResponse_Postfix(NPC __instance, Farmer who, bool asRoommate = false)
        {
            JustEngagedList.Add(__instance.Name);



        }





        private static void OnLoadStateChanged(object sender, LoadStageChangedEventArgs e)
        {
            /*
            if(e.NewStage == LoadStage.SaveAddedLocations)
            {
                foreach (Farmer farmer in Game1.getAllFarmers())
                {
                    foreach (var npc in farmer.friendshipData.Keys.ToList())
                    {
                        NPC fubar = Game1.getCharacterFromName(npc);

                        if (fubar != null && fubar.justEngaged().Value == true)
                        {
                            fubar.justEngaged().Value = false;

                        }

                    }
                }



            } */
        }



        public static bool justLoadedSave = true;


        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            justLoadedSave = true;

            JustEngagedList.Clear();


            /*
          foreach (Farmer farmer in Game1.getAllFarmers())
          {
              foreach (var npc in farmer.friendshipData.Keys.ToList())
              {
                  NPC fubar = Game1.getCharacterFromName(npc);

                  if(fubar != null && fubar.justEngaged().Value == true) 
                  { 
                  fubar.justEngaged().Value = false;

                  }

              }
          }*/











        }




        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (e.NameWithoutLocale.BaseName.Contains("custom_spouse_room_"))
                e.LoadFromModFile<Map>(e.NameWithoutLocale.BaseName + ".tmx", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            else if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, SpouseRoomData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            currentRoomData.Clear();
            /*
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                foreach (var npc in farmer.friendshipData.Keys)
                {
                    NPC fubar = Game1.getCharacterFromName(npc);

                    if(fubar != null && fubar.justEngaged().Value == true) 
                    { 
                    fubar.justEngaged().Value = false;
                    
                    }

                }
            }*/



                }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
           
        }


        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var sc = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");

            sc.RegisterCustomProperty(typeof(Farmer), "listWives", typeof(HashSet<string>), AccessTools.Method(typeof(Farmer_listWives), nameof(Farmer_listWives.haslistWives)), AccessTools.Method(typeof(Farmer_listWives), nameof(Farmer_listWives.set_haslistWives)));



            foreach (IContentPack contentPack in SHelper.ContentPacks.GetOwned())
            {
                SMonitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                SpouseRoomDataObject obj = contentPack.ReadJsonFile<SpouseRoomDataObject>("content.json");
                foreach (var srd in obj.data)
                {
                    try
                    {
                        customRoomData.Add(srd.name, srd);
                        SMonitor.Log($"Added {srd.name} room data, template {srd.templateName} start pos {srd.startPos}");
                    }
                    catch (Exception ex)
                    {
                        SMonitor.Log($"Error adding {srd.name} room data, template {srd.templateName} start pos {srd.startPos}: \n\n{ex}", LogLevel.Error);
                    }
                }

                SMonitor.Log($"Added {obj.data.Count} room datas from {contentPack.Manifest.Name}");
            }
            var dict = SHelper.GameContent.Load<Dictionary<string, SpouseRoomData>>(dictPath);
            foreach (var srd in dict.Values)
            {
                try
                {
                    customRoomData.Add(srd.name, srd);
                    SMonitor.Log($"Added {srd.name} room data, template {srd.templateName} start pos {srd.startPos}");
                }
                catch (Exception ex)
                {
                    SMonitor.Log($"Error adding {srd.name} room data, template {srd.templateName} start pos {srd.startPos}: \n\n{ex}", LogLevel.Error);
                }
            }

            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
               mod: ModManifest,
               reset: () => Config = new ModConfig(),
               save: () => Helper.WriteConfig(Config)
           );
            configMenu.AddBoolOption(
               mod: ModManifest,
               name: () => "Mod Enabled?",
               getValue: () => Config.EnableMod,
               setValue: value => Config.EnableMod = value
           );

            configMenu.AddBoolOption(
              mod: ModManifest,
              name: () => "Decorate Halls Individually?",
              getValue: () => Config.DecorateHallsIndividually,
              setValue: value => Config.DecorateHallsIndividually = value
          );


        }

        public static void ResetRooms(Farmer who, FarmHouse farmHouse, HashSet<string> ____appliedMapOverrides)
        {

            var dict = SHelper.GameContent.Load<Dictionary<string, SpouseRoomData>>(dictPath);
            foreach (var srd in dict.Values)
            {
                try
                {
                    customRoomData.Add(srd.name, srd);
                    SMonitor.Log($"Added {srd.name} room data, template {srd.templateName} start pos {srd.startPos}");
                }
                catch (Exception ex)
                {
                    SMonitor.Log($"Error adding {srd.name} room data, template {srd.templateName} start pos {srd.startPos}: \n\n{ex}", LogLevel.Error);
                }

                FarmHouse_loadSpouseRoom_Prefix(farmHouse, ____appliedMapOverrides);

        
            }

        }

        public override object GetApi()
        {
            return new SweetRoomsAPI();
        }
        
        public static IEnumerable<CodeInstruction> explode1_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var newCodes = new List<CodeInstruction>();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Stfld && ((FieldInfo)codes[i].operand).Name == "StardewValley.Locations.FarmHouse::lastSpouseRoom") // if at the part of code that sets alphaFade...
                {
                    newCodes.Remove(codes[i]); // ...intercept before setting and use custom formula
                }
                newCodes.Add(codes[i]);
            }
            return newCodes.AsEnumerable();
        } 

        public static IEnumerable<CodeInstruction> explode_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var newCodes = new List<CodeInstruction>();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Stfld && ((FieldInfo)codes[i].operand).Name == "StardewValley.Locations.FarmHouse::lastSpouseRoom") // if at the part of code that sets alphaFade...
                {
                    newCodes.Remove(codes[i]); // ...intercept before setting and use custom formula
                }
                newCodes.Add(codes[i]);
            }
            return newCodes.AsEnumerable();
        }





        private static bool showSpouseRoom_Prefix(FarmHouse __instance)
        {
            if (!Config.EnableMod )
                return true;

          

            bool displayingspouseroom = SHelper.Reflection.GetField<bool>(__instance, "displayingSpouseRoom").GetValue();

            
                var allSpouses = GetSpouses(__instance.owner, -1).Keys.ToList();
                if (allSpouses.Count == 0)
                    return true;

                if ( allSpouses.Count == 1)// single spouse, no customizations
                {
                    SMonitor.Log("Single uncustomized spouse room, letting vanilla game take over");
                    return true;
                }


                if (allSpouses.Count >= 2)
            {
                HashSet<string> ____appliedMapOverrides = new();

                bool showSpouse;
                showSpouse = __instance.HasNpcSpouseOrRoommate();
                bool num;
                num = displayingspouseroom;
                displayingspouseroom = showSpouse;
                __instance.updateMap();


                List<string> listowives = new List<string>();


                    __instance.owner.haslistWives().Clear();

                    foreach (var npc in __instance.owner.friendshipData.Keys)
                    {
                        if (__instance.owner.friendshipData[npc].IsMarried())
                        {
                        __instance.owner.haslistWives().Add(npc);
                        listowives.Add(npc);
                        }


                    }

                    




                    foreach ( string wives in listowives )
                {

                 //   if (num && !displayingspouseroom)
                    {
                        Point corner;
                        corner = __instance.GetSpouseRoomCorner();
                        Microsoft.Xna.Framework.Rectangle sourceArea;
                        sourceArea = CharacterSpouseRoomData.DefaultMapSourceRect;
                        if (NPC.TryGetData(__instance.owner.spouse, out var spouseData)) //Angel THISSSSSSSSSSS
                        {
                            sourceArea = spouseData.SpouseRoom?.MapSourceRect ?? sourceArea;
                        }
                        Microsoft.Xna.Framework.Rectangle spouseRoomBounds;
                        spouseRoomBounds = new Microsoft.Xna.Framework.Rectangle(corner.X, corner.Y, sourceArea.Width, sourceArea.Height);
                        spouseRoomBounds.X--;
                        List<Item> collected_items;
                        collected_items = new List<Item>();
                        Microsoft.Xna.Framework.Rectangle room_bounds;
                        room_bounds = new Microsoft.Xna.Framework.Rectangle(spouseRoomBounds.X * 64, spouseRoomBounds.Y * 64, spouseRoomBounds.Width * 64, spouseRoomBounds.Height * 64);
                        foreach (Furniture placed_furniture in new List<Furniture>(__instance.furniture))
                        {
                            if (placed_furniture.GetBoundingBox().Intersects(room_bounds))
                            {
                                if (placed_furniture is StorageFurniture storage_furniture)
                                {
                                    collected_items.AddRange(storage_furniture.heldItems);
                                    storage_furniture.heldItems.Clear();
                                }
                                if (placed_furniture.heldObject.Value != null)
                                {
                                    collected_items.Add(placed_furniture.heldObject.Value);
                                    placed_furniture.heldObject.Value = null;
                                }
                                collected_items.Add(placed_furniture);
                                __instance.furniture.Remove(placed_furniture);
                            }
                        }
                        for (int x = spouseRoomBounds.X; x <= spouseRoomBounds.Right; x++)
                        {
                            for (int y = spouseRoomBounds.Y; y <= spouseRoomBounds.Bottom; y++)
                            {
                                StardewValley.Object tile_object;
                                tile_object = __instance.getObjectAtTile(x, y);
                                if (tile_object == null || tile_object is Furniture)
                                {
                                    continue;
                                }
                                tile_object.performRemoveAction();
                                if (!(tile_object is Fence fence))
                                {
                                    if (!(tile_object is IndoorPot garden_pot))
                                    {
                                        if (tile_object is Chest chest)
                                        {
                                            collected_items.AddRange(chest.Items);
                                            chest.Items.Clear();
                                        }
                                    }
                                    else if (garden_pot.hoeDirt.Value?.crop != null)
                                    {
                                        garden_pot.hoeDirt.Value.destroyCrop(showAnimation: false);
                                    }
                                }
                                else
                                {
                                    tile_object = new StardewValley.Object(fence.ItemId, 1);
                                }
                                tile_object.heldObject.Value = null;
                                tile_object.minutesUntilReady.Value = -1;
                                tile_object.readyForHarvest.Value = false;
                                collected_items.Add(tile_object);
                                __instance.objects.Remove(new Vector2(x, y));
                            }
                        }
                        if (__instance.upgradeLevel >= 2)
                        {
                            Utility.createOverflowChest(__instance, new Vector2(39f, 32f), collected_items);
                        }
                        else
                        {
                            Utility.createOverflowChest(__instance, new Vector2(21f, 10f), collected_items);
                        }
                    }

                }
                __instance.loadObjects();
                if (__instance.upgradeLevel == 3)
                {
                    __instance.AddCellarTiles();
                    __instance.createCellarWarps();
                    Game1.player.craftingRecipes.TryAdd("Cask", 0);
                }




                if (showSpouse)

                {
                    //foreach (Farmer farmer in Game1.getAllFarmers())
                    {
                        // __instance.loadSpouseRoom();

                        FarmHouse_loadSpouseRoom_Prefix(__instance, ____appliedMapOverrides);
                    }
                }




                return false;

            }




            return true;
        }



    }
}