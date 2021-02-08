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
        bladeOPTrash = 200909,

    }

    [HarmonyPatch(typeof(FTK_itembase), "GetLocalizedName")]
    class itembaseGetLocalizedNameHook
    {
        static bool Prefix(ref string __result, FTK_itembase __instance)
        {
            //TODO: Automate.
            //If the weapon has our item's ID
            if (__instance.m_ID == "bladeOPTrash")
            {
                //Return our item name.
                __result = "OP Trash";
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

    [HarmonyPatch(typeof(FTK_itemsDB), "GetEntry")]
    class itemsDBGetEntryHook
    {
        static bool Prefix(FTK_itemsDB __instance, ref FTK_items __result, FTK_itembase.ID _enumID)
        {
            //Never called but here for completeness sake.
            if ((Items)_enumID == Items.bladeOPTrash)
            {
                __result = __instance.GetEntryByInt(100109);
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
            if ((Items)_enumID == Items.bladeOPTrash)
            {
                //Copy stats from base item Rusty Blade.
                __result = __instance.GetEntryByInt(100109);

                //Change the ID to our custom ID (REQUIRED).

                __result.m_ID = "bladeOPTrash";
                
                //Change the damage type to magic.
                __result._dmgtype = FTK_weaponStats2.DamageType.magic;
                
                //Give it unreasonable damage.
                __result._maxdmg = 50000;
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(FTK_itembase), "GetEnum")]
    class itembaseGetEnumHook
    {
        static bool Prefix(ref FTK_itembase.ID __result, string _id)
        {
            //Not 100% sure if this is required.
            //If item doesn't exist in array.
            if (TableManager.Instance.Get<FTK_itemsDB>().GetEntryByStringID("bladeOPTrash") == null)
            {
                //Add our item to the array.
                TableManager.Instance.Get<FTK_itemsDB>().AddEntry("bladeOPTrash");
                
                //Update the dictionary.
                TableManager.Instance.Get<FTK_itemsDB>().CheckAndMakeIndex();
            }
            if (TableManager.Instance.Get<FTK_weaponStats2DB>().GetEntryByStringID("bladeOPTrash") == null)
            {
                //Add our item to the array.
                TableManager.Instance.Get<FTK_weaponStats2DB>().AddEntry("bladeOPTrash");

                //Update the dictionary.
                TableManager.Instance.Get<FTK_weaponStats2DB>().CheckAndMakeIndex();
            }
            //Return our enum if ID matches ours.
            if (_id == "bladeOPTrash")
            {
                __result = (FTK_itembase.ID)Items.bladeOPTrash;
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
                __result.m_StartWeapon = (FTK_itembase.ID)(Items.bladeOPTrash);
                Debug.Log("Hunter hook Activated " + ((FTK_itembase.ID)(Items.bladeOPTrash)).ToString());
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

    }
}
