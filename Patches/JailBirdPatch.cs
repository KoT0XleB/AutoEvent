﻿using System.Collections.Generic;
using System.Reflection.Emit;
using Exiled.Events.EventArgs.Item;
using HarmonyLib;
using InventorySystem.Items.Jailbird;
using Mirror;
using NorthwoodLib.Pools;
using static HarmonyLib.AccessTools;

namespace AutoEvent.Patches
{
    internal class JailBirdPatch
    {
        [HarmonyPatch(typeof(JailbirdItem), nameof(JailbirdItem.ServerProcessCmd))]
        static class ServerProcessCmdPatch
        {
            [HarmonyPrefix]
            internal static bool Yes(JailbirdItem __instance, NetworkReader reader)
            {
                if (AutoEvent.ActiveEvent == null) return true;

                if (AutoEvent.Singleton.Config.IsJailbirdHasInfinityCharges)
                    __instance.TotalChargesPerformed = 0;

                 if (!AutoEvent.Singleton.Config.IsJailbirdAbilityEnable) __instance._chargeDuration = 0;

                return true;
            }
        }
    }
}