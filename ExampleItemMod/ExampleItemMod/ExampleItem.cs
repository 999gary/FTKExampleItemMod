using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MelonLoader;
using Harmony;
using GridEditor;
using UnityEngine;

namespace ExampleItemMod
{

    //This mod makes heavy use of the Harmony library. You can find more info here: https://github.com/pardeike/Harmony/wiki

    public enum Items
    {
        //With a mod api of some kind this shouldn't be required. But for now you need to manually check IDs.
        //Anything over 100000? is a weapon, anything under is a regular item.
        bladeBetterDagger = 200909,

    }



    //Disable bug reporting to the official devs.
    [HarmonyPatch(typeof(SerializeGO), "ShowBugForm")]
    class SerializeGOShowBugFormHook
    {
        static bool Prefix()
        {
            MelonLogger.Log("Please report all bugs to the mod developer. https://github.com/999gary/FTKExampleItemMod");
            return false;
        }
    }

    [HarmonyPatch(typeof(FTK_itembase), "GetLocalizedName")]
    class itembaseGetLocalizedNameHook
    {
        static bool Prefix(ref string __result, FTK_itembase __instance)
        {
            //TODO: Automate.
            //If the weapon has our item's ID
            if (__instance.m_ID == "bladeBetterDagger")
            {
                //Return our item name.
                __result = "Better Dagger";
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FTK_itemsDB), "GetIntFromID")]
    class itemsDBGetIntFromIDHook
    {
        static bool Prefix(ref int __result, FTK_itemsDB __instance, string _id)
        {
            try
            {
                //Attempts to return our enum and calls the original function if it errors.
                __result = (int)Enum.Parse(typeof(Items), _id, true);
                return false;
            }
            catch (ArgumentException)
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(FTK_weaponStats2DB), "GetIntFromID")]
    class weaponStats2DBGetIntFromIDHook
    {
        static bool Prefix(ref int __result, FTK_weaponStats2DB __instance, string _id)
        {
            try
            {
                //Attempts to return our enum and calls the original function if it errors.
                __result = (int)Enum.Parse(typeof(Items), _id, true);
                return false;
            }
            catch (ArgumentException)
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(FTK_itemsDB), "GetEntry")]
    class itemsDBGetEntryHook
    {
        static bool Prefix(FTK_itemsDB __instance, ref FTK_items __result, FTK_itembase.ID _enumID)
        {
            //Never called but here for completeness sake.
            if ((Items)_enumID == Items.bladeBetterDagger)
            {
                __result = __instance.GetEntryByInt((int)Items.bladeBetterDagger);
                return false;
            }
            return true;
        }
    }



    [HarmonyPatch(typeof(FTK_weaponStats2DB), "GetEntry")]
    class weaponStats2DBGetEntryHook
    {
        static bool Prefix(FTK_weaponStats2DB __instance, ref FTK_weaponStats2 __result, FTK_itembase.ID _enumID)
        {
            //TODO: Automate this process.
            if ((Items)_enumID == Items.bladeBetterDagger)
            {
                //Copy stats from base item Dagger.
                __result = __instance.GetEntryByInt((int)FTK_itembase.ID.bladeDagger);

                //Make the item ID different.
                __result.m_ID = "bladeBetterDagger";

                //Give it different stats.
                __result._maxdmg = 20;

                return false;
            }
            return true;
        }
    }

    //Hacky fix for bug.
    [HarmonyPatch(typeof(FTK_weaponStats2DB), "IsContainID")]
    class weaponStats2DBIsContainIDHook
    {
        static bool Prefix(ref bool __result, string _id)
        {
            if (_id == "bladeBetterDagger")
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FTK_itemsDB), "Awake")]
    class itemsDBAwakeHook
    {
        static void Prefix()
        {
            if (TableManager.Instance.Get<FTK_itemsDB>().GetEntryByStringID("bladeBetterDagger") == null)
            {
                //Add our item to the array.
                TableManager.Instance.Get<FTK_itemsDB>().AddEntry("bladeBetterDagger");

                //Update the dictionary.
                TableManager.Instance.Get<FTK_itemsDB>().CheckAndMakeIndex();
            }
        }
    }

    [HarmonyPatch(typeof(FTK_weaponStats2DB), "Awake")]
    class weaponStats2DBAwakeHook
    {
        static void Prefix()
        {
            if (TableManager.Instance.Get<FTK_weaponStats2DB>().GetEntryByStringID("bladeBetterDagger") == null)
            {
                //Add our item to the array.
                TableManager.Instance.Get<FTK_weaponStats2DB>().AddEntry("bladeBetterDagger");

                //Update the dictionary.
                TableManager.Instance.Get<FTK_weaponStats2DB>().CheckAndMakeIndex();
            }
        }
    }

    [HarmonyPatch(typeof(FTK_itembase), "GetEnum")]
    class itembaseGetEnumHook
    {
        static bool Prefix(ref FTK_itembase.ID __result, string _id)
        {
            //Not 100% sure if this is required.
            //If item doesn't exist in array.

            //Return our enum if ID matches ours.
            if (_id == "bladeBetterDagger")
            {
                __result = (FTK_itembase.ID)Items.bladeBetterDagger;
                return false;
            }
            return true;
        }
    }


    //Gives item to Hunter as starting weapon for an example.
    [HarmonyPatch(typeof(FTK_playerGameStartDB), "GetEntry")]
    class playerGameStartDBGetEntryHook
    {
        static bool Prefix(ref FTK_playerGameStart __result, FTK_playerGameStartDB __instance, FTK_playerGameStart.ID _enumID)
        {
            if (_enumID == FTK_playerGameStart.ID.hunter)
            {
                __result = __instance.GetEntryByInt((int)_enumID);
                __result.m_StartWeapon = (FTK_itembase.ID)(Items.bladeBetterDagger);
                Debug.Log("Hunter hook Activated " + ((FTK_itembase.ID)(Items.bladeBetterDagger)).ToString());
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public class ExampleItem : MelonMod
    {
        public override void OnApplicationStart()
        {
            MelonLogger.Log("Initalizing method patches.");
            //All items are stored in Enums, accessed through methods. Therefore we can hook the methods and inject our own enum.
            var harmony = HarmonyInstance.Create("com.ExampleMod.Item");
            harmony.PatchAll();
            MelonLogger.Log("Methods patched, have fun. :)");
        }

        public override void OnLevelWasLoaded(int level)
        {
            if (level == 1)
                FTKVersion.Instance.m_VersionNum = FTKVersion.Instance.m_VersionNum + ".modded";
        }

    }
}
