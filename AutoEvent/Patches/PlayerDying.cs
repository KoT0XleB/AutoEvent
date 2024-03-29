﻿using AutoEvent.API;
using AutoEvent.Events.EventArgs;
using AutoEvent.Events.Handlers;
using HarmonyLib;
using PlayerStatsSystem;
using PluginAPI.Core;
using UnityEngine;

namespace AutoEvent.Patches
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.KillPlayer))]
    internal static class PlayerDying
    {
        private static bool Prefix(PlayerStats __instance, DamageHandlerBase handler)
        {
            Player player = Player.Get(__instance?._hub);

            PlayerDyingArgs ev = new(player, handler);
            Players.OnPlayerDying(ev);

            if (ev.IsAllowed)
            {
                if (Extensions.InfiniteAmmoList is not null && Extensions.InfiniteAmmoList.ContainsKey(player))
                {
                    Extensions.InfiniteAmmoList.Remove(player);
                }
            }
            return ev.IsAllowed;
        }
    }
}
