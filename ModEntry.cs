﻿using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using StardewModdingAPI.Events;
using PolyamorySweetLove;

namespace PolyamorySweetRooms
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        
        public static string dictPath = "ApryllForever.PolyamorySweetRooms/dict";

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
            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.updateFarmLayout)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.FarmHouse_updateFarmLayout_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(DecoratableLocation), nameof(DecoratableLocation.MakeMapModifications)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.DecoratableLocation_MakeMapModifications_Postfix))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(DecoratableLocation), "IsFloorableOrWallpaperableTile"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.DecoratableLocation_IsFloorableOrWallpaperableTile_Prefix))
            );
            

            // NetWorldState patch 

            harmony.Patch(
               original: AccessTools.Method(typeof(NetWorldState), nameof(NetWorldState.hasWorldStateID)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.hasWorldStateID_Prefix))
            );

            SHelper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            SHelper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            SHelper.Events.Content.AssetRequested += Content_AssetRequested;
            SHelper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            
        }

        public static bool justLoadedSave = true;
        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            justLoadedSave = true;
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
        }
        public static IPolyamorySweetLoveAPI polyamorySweetLoveAPI;


        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            /*
             * 
             * Lets' see where the hell turning this off goes. It's making bugs. Content packs are no longer used. 
             * 
             * 
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
            }*/
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


    }
}