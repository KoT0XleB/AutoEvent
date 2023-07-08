﻿using AutoEvent.Interfaces;
using Exiled.API.Features;
using MapEditorReborn.API.Features.Objects;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AutoEvent.Events.Speedrun
{
    public class Plugin// : Event
    {
        public string Name { get; set; } = "Speedrun";
        public string Description { get; set; } = "Crab game [Alpha]";
        public string Color { get; set; } = "FF4242";
        public string CommandName { get; set; } = "speedrun";
        public static SchematicObject GameMap { get; set; }
        public static TimeSpan EventTime { get; set; }

        EventHandler _eventHandler;

        public void OnStart()
        {
            _eventHandler = new EventHandler();

            Exiled.Events.Handlers.Player.Verified += _eventHandler.OnJoin;
            Exiled.Events.Handlers.Player.Died += _eventHandler.OnDead;
            Exiled.Events.Handlers.Player.Hurting += _eventHandler.OnDamage;
            Exiled.Events.Handlers.Server.RespawningTeam += _eventHandler.OnTeamRespawn;
            Exiled.Events.Handlers.Player.SpawningRagdoll += _eventHandler.OnSpawnRagdoll;
            Exiled.Events.Handlers.Map.PlacingBulletHole += _eventHandler.OnPlaceBullet;
            Exiled.Events.Handlers.Map.PlacingBlood += _eventHandler.OnPlaceBlood;
            Exiled.Events.Handlers.Player.DroppingItem += _eventHandler.OnDropItem;
            Exiled.Events.Handlers.Player.DroppingAmmo += _eventHandler.OnDropAmmo;

            OnEventStarted();
        }
        public void OnStop()
        {
            Exiled.Events.Handlers.Player.Verified -= _eventHandler.OnJoin;
            Exiled.Events.Handlers.Player.Died -= _eventHandler.OnDead;
            Exiled.Events.Handlers.Player.Hurting -= _eventHandler.OnDamage;
            Exiled.Events.Handlers.Server.RespawningTeam -= _eventHandler.OnTeamRespawn;
            Exiled.Events.Handlers.Player.SpawningRagdoll -= _eventHandler.OnSpawnRagdoll;
            Exiled.Events.Handlers.Map.PlacingBulletHole -= _eventHandler.OnPlaceBullet;
            Exiled.Events.Handlers.Map.PlacingBlood -= _eventHandler.OnPlaceBlood;
            Exiled.Events.Handlers.Player.DroppingItem -= _eventHandler.OnDropItem;
            Exiled.Events.Handlers.Player.DroppingAmmo -= _eventHandler.OnDropAmmo;

            _eventHandler = null;
            Timing.CallDelayed(10f, () => EventEnd());
        }

        public void OnEventStarted()
        {
            EventTime = new TimeSpan(0, 0, 0);
            GameMap = Extensions.LoadMap("Speedrun", new Vector3(115.5f, 1030f, -43.5f), Quaternion.Euler(Vector3.zero), Vector3.one);
            //Extensions.PlayAudio(, 15, true, Name);

            foreach (Player player in Player.List)
            {
                player.Role.Set(RoleTypeId.ClassD, Exiled.API.Enums.SpawnReason.None, RoleSpawnFlags.None);
                player.Position = RandomPosition.GetSpawnPosition(Plugin.GameMap);
            }

            Timing.RunCoroutine(OnEventRunning(), "speed_run");
        }

        public IEnumerator<float> OnEventRunning()
        {
            var trans = AutoEvent.Singleton.Translation;

            for (float time = 15; time > 0; time--)
            {
                Extensions.Broadcast($"Ивент {Name}", 1);
                yield return Timing.WaitForSeconds(1f);
            }

            //List<GameObject> platformes = GameMap.AttachedBlocks.Where(x => x.name == "Platform").ToList();

            while (Player.List.Count(r => r.IsAlive) > 0) // EventTime.TotalMinutes == 3
            {
                //var count = Player.List.Count(r => r.Role == RoleTypeId.ClassD);
                //var time = $"{EventTime.Minutes}:{EventTime.Seconds}";
                //Extensions.Broadcast(trans.ZombieCycle.Replace("{name}", Name).Replace("{count}", count.ToString()).Replace("{time}", time), 1);

                //var platform = platformes.RandomItem();
                //platformes.Remove(platform);
                //GameObject.Destroy(platform);

                yield return Timing.WaitForSeconds(0.1f);
                EventTime += TimeSpan.FromSeconds(0.1f);
            }

            /*
            if (Player.List.Count(r => r.Role == RoleTypeId.ClassD) == 0)
            {
                Extensions.Broadcast(trans.ZombieWin.Replace("{time}", time), 10);
            }
            else
            {
                Extensions.Broadcast(trans.ZombieLose.Replace("{time}", time), 10);
            }
            */
            OnStop();
            yield break;
        }

        public void EventEnd()
        {
            Extensions.CleanUpAll();
            Extensions.TeleportEnd();
            Extensions.UnLoadMap(GameMap);
            Extensions.StopAudio();
            AutoEvent.ActiveEvent = null;
        }
    }
}
