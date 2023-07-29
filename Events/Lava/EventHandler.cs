﻿using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using InventorySystem.Configs;
using PlayerRoles;
using System.Collections.Generic;

namespace AutoEvent.Events.Lava
{
    public class EventHandler
    {
        public void OnReloading(ReloadingWeaponEventArgs ev)
        {
            SetMaxAmmo(ev.Player);
        }

        public void OnSpawned(SpawnedEventArgs ev)
        {
            SetMaxAmmo(ev.Player);
        }

        public void OnHurt(HurtingEventArgs ev)
        {
            if (ev.Player != null && ev.DamageHandler.Type == DamageType.Falldown)
            {
                ev.IsAllowed = false;
            }

            if (ev.Attacker != null && ev.Player != null)
            {
                ev.Player.Hurt(3f, "Shooting");
            }
        }

        public void OnJoin(VerifiedEventArgs ev) => ev.Player.Role.Set(RoleTypeId.Spectator);
        public void OnTeamRespawn(RespawningTeamEventArgs ev) => ev.IsAllowed = false;
        public void OnSpawnRagdoll(SpawningRagdollEventArgs ev) => ev.IsAllowed = false;
        public void OnPlaceBullet(PlacingBulletHole ev) => ev.IsAllowed = false;
        public void OnPlaceBlood(PlacingBloodEventArgs ev) => ev.IsAllowed = false;
        public void OnDropAmmo(DroppingAmmoEventArgs ev) => ev.IsAllowed = false;
        private void SetMaxAmmo(Player pl)
        {
            foreach (KeyValuePair<ItemType, ushort> AmmoLimit in InventoryLimits.StandardAmmoLimits)
                pl.SetAmmo(AmmoLimit.Key.GetAmmoType(), AmmoLimit.Value);
        }
    }
}
