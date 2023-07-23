﻿using AutoEvent.Events.TipToe.Features;
using AutoEvent.Interfaces;
using Exiled.API.Enums;
using Exiled.API.Features;
using MapEditorReborn.API.Features.Objects;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AutoEvent.Events.SkipRope
{
    public class Plugin : Event
    {
        public override string Name { get; set; } = "Skipping Rope";
        public override string Description { get; set; } = "Нужно перепрыгивать скакалку.";
        public override string Color { get; set; } = "FF4242";
        public override string CommandName { get; set; } = "rope";
        public SchematicObject GameMap { get; set; }
        public TimeSpan EventTime { get; set; }

        EventHandler _eventHandler;

        public override void OnStart()
        {
            _eventHandler = new EventHandler();

            Exiled.Events.Handlers.Player.Verified += _eventHandler.OnJoin;
            Exiled.Events.Handlers.Server.RespawningTeam += _eventHandler.OnTeamRespawn;
            Exiled.Events.Handlers.Player.SpawningRagdoll += _eventHandler.OnSpawnRagdoll;
            Exiled.Events.Handlers.Map.PlacingBulletHole += _eventHandler.OnPlaceBullet;
            Exiled.Events.Handlers.Map.PlacingBlood += _eventHandler.OnPlaceBlood;
            Exiled.Events.Handlers.Player.DroppingItem += _eventHandler.OnDropItem;
            Exiled.Events.Handlers.Player.DroppingAmmo += _eventHandler.OnDropAmmo;

            OnEventStarted();
        }

        public override void OnStop()
        {
            Exiled.Events.Handlers.Player.Verified -= _eventHandler.OnJoin;
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
            GameMap = Extensions.LoadMap("SkipRope", new Vector3(20f, 1026.5f, -45f), Quaternion.Euler(Vector3.zero), Vector3.one);

            Extensions.PlayAudio("LineLite.ogg", 10, true, Name);

            foreach (Player player in Player.List)
            {
                player.Role.Set(RoleTypeId.ClassD, SpawnReason.None, RoleSpawnFlags.None);
                player.Position = RandomClass.GetSpawnPosition(GameMap);
            }

            Timing.RunCoroutine(OnEventRunning(), "rope_run");
        }

        public IEnumerator<float> OnEventRunning()
        {
            EventTime = new TimeSpan(0, 2, 0);

            for (int time = 10; time > 0; time--)
            {
                Extensions.Broadcast(time.ToString(), 1);
                yield return Timing.WaitForSeconds(1f);
            }

            while (Player.List.Count(r => r.IsAlive) > 1 && EventTime.TotalSeconds > 0)
            {
                Extensions.Broadcast($"<color=#{Color}>{Name}</color>\n" +
                    $"<color=blue>Осталось {EventTime.Minutes}:{EventTime.Seconds}</color>\n" +
                    $"<color=yellow>Осталось игроков - {Player.List.Count(r=>r.IsAlive)}</color>", 1);

                yield return Timing.WaitForSeconds(1f);
                EventTime -= TimeSpan.FromSeconds(1f);
            }

            if (Player.List.Count(r => r.IsAlive) > 1)
            {
                Extensions.Broadcast($"<color=yellow>Выжило {Player.List.Count(r=>r.IsAlive)} игроков</color>\n<color=red>Победа</color>", 10);
            }
            else if (Player.List.Count(r => r.IsAlive) == 1)
            {
                Extensions.Broadcast($"</color=green>Победил игрок {Player.List.First(r => r.IsAlive).Nickname}</color>\n<color=yellow>Поздравляем тебя!</color>", 10);
            }
            else
            {
                Extensions.Broadcast($"<color=red>Никто не выжил.</color>\n<color=yellow>Конец мини-игры</color>", 10);
            }

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