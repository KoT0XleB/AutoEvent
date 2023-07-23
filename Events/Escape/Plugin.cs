﻿using AutoEvent.Interfaces;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AutoEvent.Events.Escape
{
    public class Plugin : Event
    {
        public override string Name { get; set; } = AutoEvent.Singleton.Translation.EscapeName;
        public override string Description { get; set; } = AutoEvent.Singleton.Translation.EscapeDescription;
        public override string Color { get; set; } = "FFFF00";
        public override string CommandName { get; set; } = "escape";
        public TimeSpan EventTime { get; set; }

        EventHandler _eventHandler;

        public override void OnStart()
        {
            _eventHandler = new EventHandler();

            Exiled.Events.Handlers.Player.Verified += _eventHandler.OnJoin;
            Exiled.Events.Handlers.Cassie.SendingCassieMessage += _eventHandler.OnSendCassie;
            Exiled.Events.Handlers.Server.RespawningTeam += _eventHandler.OnTeamRespawn;
            Exiled.Events.Handlers.Scp173.PlacingTantrum += _eventHandler.OnPlaceTantrum;
            OnEventStarted();
        }
        public override void OnStop()
        {
            Exiled.Events.Handlers.Player.Verified -= _eventHandler.OnJoin;
            Exiled.Events.Handlers.Cassie.SendingCassieMessage -= _eventHandler.OnSendCassie;
            Exiled.Events.Handlers.Server.RespawningTeam -= _eventHandler.OnTeamRespawn;
            Exiled.Events.Handlers.Scp173.PlacingTantrum -= _eventHandler.OnPlaceTantrum;

            _eventHandler = null;
            Timing.CallDelayed(5f, () => EventEnd());
        }

        public void OnEventStarted()
        {
            EventTime = new TimeSpan(0, 0, 0);

            GameObject _startPos = new GameObject();
            _startPos.transform.parent = Room.Get(RoomType.Lcz173).transform;
            _startPos.transform.localPosition = new Vector3(16.5f, 13f, 8f);

            Player.List.ToList().ForEach(player =>
            {
                player.Role.Set(RoleTypeId.Scp173, SpawnReason.None, RoleSpawnFlags.None);
                player.Position = _startPos.transform.position;
                //player.Rotation = _startPos.transform.localEulerAngles; // camera to door
                player.EnableEffect(EffectType.Ensnared, 10);
            });

            Extensions.PlayAudio("Escape.ogg", 25, true, Name);

            Warhead.DetonationTimer = 120f;
            Warhead.Start();
            Warhead.IsLocked = true;

            Timing.RunCoroutine(OnEventRunning(), "escape_run");
        }
        public IEnumerator<float> OnEventRunning()
        {
            var trans = AutoEvent.Singleton.Translation;

            for (int time = 10; time > 0; time--)
            {
                Extensions.Broadcast(trans.EscapeBeforeStart.Replace("{name}", Name).Replace("{time}", ((int)time).ToString()), 1);
                yield return Timing.WaitForSeconds(1f);
                EventTime += TimeSpan.FromSeconds(1f);
            }
            var explosionTime = 80;

            while (EventTime.TotalSeconds != explosionTime && Player.List.Count(r => r.IsAlive) > 0)
            {
                Extensions.Broadcast(trans.EscapeCycle.Replace("{name}", Name).Replace("{time}", (explosionTime - EventTime.TotalSeconds).ToString()), 1);
                yield return Timing.WaitForSeconds(1f);
                EventTime += TimeSpan.FromSeconds(1f);
            }

            Warhead.IsLocked = false;
            Warhead.Stop();

            foreach (Player player in Player.List)
            {
                player.EnableEffect<CustomPlayerEffects.Flashed>(1);
                if (player.Position.y < 980f)
                {
                    player.Kill(DamageType.Warhead);
                }
            }
            Extensions.Broadcast(trans.EscapeEnd.Replace("{name}", Name), 10);
            OnStop();
            yield break;
        }
        public void EventEnd()
        {
            Extensions.CleanUpAll();
            Extensions.TeleportEnd();
            Extensions.StopAudio();
            AutoEvent.ActiveEvent = null;
        }
    }
}
