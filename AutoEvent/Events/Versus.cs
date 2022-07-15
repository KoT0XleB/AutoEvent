using AutoEvent.Functions;
using AutoEvent.Interfaces;
using MEC;
using Mirror;
using Qurre;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Controllers;
using Qurre.API.Controllers.Items;
using Qurre.API.Events;
using Qurre.API.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static AutoEvent.Functions.MainFunctions;
using Random = UnityEngine.Random;
//What? Why are there so many NOT USED references?

namespace AutoEvent.Events
{
    internal class Versus : IEvent
    {
        public string Name => "Петушиные Бои [В работе!]";
        public string Description => "Дуель игроков на карте 35hp из cs 1.6";
        public string Color => "FFFF00";
        public string CommandName => "35hp";
        public Model Model { get; set; }
        public Model Doors { get; set; }
        public bool ClassdDoorOpened { get; set; } = false;
        public bool ScientistDoorOpened { get; set; } = false;
        public Player Scientist { get; set; }
        public Player ClassD { get; set; }
        public TimeSpan EventTime { get; set; }
        public int Votes { get; set; }

        public void OnStart()
        {
            Plugin.IsEventRunning = true;
            Qurre.Events.Player.Join += OnJoin;
            OnEventStarted();
        }
        public void OnStop()
        {
            Plugin.IsEventRunning = false;
            Qurre.Events.Player.Join -= OnJoin;
            Timing.CallDelayed(5f, () => EventEnd());
        }
        public void OnEventStarted()
        {
            EventTime = new TimeSpan(0, 0, 0);

            CreatingMapFromJson("35Hp.json", new Vector3(145.18f, 930f, -122.97f), out var model);
            Model = model;

            //PlayAudio("MGS4.f32le", 10, true, "БИТВА");
            var count = 0;
            foreach (Player player in Player.List)
            {
                if (count % 2 == 0)
                {
                    player.Role = RoleType.Scientist;
                    Timing.CallDelayed(2f, () =>
                    {
                        player.Position = Model.GameObject.transform.position + new Vector3(-56.1f, -6.3f, 3.12f);
                        player.ClearInventory();
                    });
                }
                else
                {
                    player.Role = RoleType.ClassD;
                    Timing.CallDelayed(2f, () =>
                    {
                        player.Position = Model.GameObject.transform.position + new Vector3(27.18f, -6.3f, 3.12f);
                        player.ClearInventory();
                    });
                }
                count++;
            }
            Timing.RunCoroutine(Cycle(), "35hp_time");
        }
        public IEnumerator<float> Cycle()
        {
            for (int time = 10; time > 0; time--)
            {
                BroadcastPlayers($"<size=100><color=red>{time}</color></size>", 1);
                yield return Timing.WaitForSeconds(1f);
            }
            // Player.List.Count(r => r.Team == Team.RSC) > 0 && Player.List.Count(r => r.Team == Team.CDP) > 0
            while (!Round.Ended)
            {
                Arena();
                BroadcastPlayers($"<color=#D71868><b><i>Петушиные Бои</i></b></color>\n" +
                $"<color=yellow><color=yellow>{Player.List.Count(r => r.Team == Team.RSC)}</color> VS <color=orange>{Player.List.Count(r => r.Team == Team.CDP)}</color></color>\n" +
                $"<color=yellow>Время ивента <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 1);
                yield return Timing.WaitForSeconds(1f);
                EventTime += TimeSpan.FromSeconds(1f);
            }
            if (Player.List.Count(r => r.Team == Team.RSC) == 0)
            {
                BroadcastPlayers($"<color=#D71868><b><i>Петушиные Бои</i></b></color>\n" +
                $"<color=yellow><color=yellow>{Player.List.Count(r => r.Team == Team.RSC)}</color> VS <color=orange>{Player.List.Count(r => r.Team == Team.CDP)}</color></color>\n" +
                $"<color=yellow>Время ивента <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 10);
            }
            else if (Player.List.Count(r => r.Team == Team.CDP) == 0)
            {
                BroadcastPlayers($"<color=#D71868><b><i>Петушиные Бои</i></b></color>\n" +
                $"<color=yellow><color=yellow>{Player.List.Count(r => r.Team == Team.RSC)}</color> VS <color=orange>{Player.List.Count(r => r.Team == Team.CDP)}</color></color>\n" +
                $"<color=yellow>Время ивента <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 10);
            }
            OnStop();
            yield break;
        }
        public void Arena()
        {
            foreach (Player player in Player.List)
            {
                if (Scientist is null)
                {
                    // Открываем двери перед началом, чтобы можно было пройти внутрь
                    // Делаем проверку на расстояние до объекта внутри комнаты и даем Scientist
                    // Если это не Scientist, то перемещаем назад
                    // Если Scientist живой то закрываем комнату
                    // Если Scientist мертвый то открываем комнату

                    List<Player> newList = (List<Player>)Player.List.Select(r => r.Team == Team.RSC);
                    Scientist = newList.RandomItem();
                    Scientist.Position = new Vector3(-30, -5.45f, 3.12f);

                    if (!ScientistDoorOpened)
                    {
                        Doors.Primitives[0].Primitive.Position += new Vector3(0, 0, 3.16f);
                        ScientistDoorOpened = true;
                    }
                }
                else
                {
                    if (Scientist.Role == RoleType.Spectator)
                    {
                        Scientist = null;
                        if (ScientistDoorOpened)
                        {
                            Doors.Primitives[0].Primitive.Position -= new Vector3(0, 0, 3.16f);
                            ScientistDoorOpened = false;
                        }
                    }
                }

                if (ClassD is null)
                {
                    List<Player> newList = (List<Player>)Player.List.Select(r => r.Team == Team.CDP);
                    ClassD = newList.RandomItem();
                    ClassD.Position = new Vector3(20, -5.45f, 3.12f);

                    if (!ClassdDoorOpened)
                    {
                        Doors.Primitives[1].Primitive.Position += new Vector3(0, 0, 3.16f);
                        ScientistDoorOpened = true;
                    }
                }
                else
                {
                    if (ClassD.Role == RoleType.Spectator)
                    {
                        ClassD = null;
                        if (ClassdDoorOpened)
                        {
                            Doors.Primitives[1].Primitive.Position -= new Vector3(0, 0, 3.16f);
                            ScientistDoorOpened = false;
                        }
                    }
                }

            }
        }
        public void EventEnd()
        {
            if (Audio.Microphone.IsRecording) StopAudio();
            Timing.RunCoroutine(DestroyObjects(Model));
            Timing.RunCoroutine(CleanUpAll());
        }
        public void CreateDoors()
        { // 3.16
            Doors = new Model("PrisonerDoors", Model.GameObject.transform.position);
            Doors.AddPart(new ModelPrimitive(Doors, PrimitiveType.Cube, new Color32(85, 87, 85, 51), new Vector3(-30.15f, -5.45f, 3.12f), new Vector3(90, 90, 0), new Vector3(3.1f, 1f, 6.52f)));
            Doors.AddPart(new ModelPrimitive(Doors, PrimitiveType.Cube, new Color32(85, 87, 85, 51), new Vector3(1.76f, -5.45f, 3.12f), new Vector3(90, 90, 0), new Vector3(3.1f, 1f, 6.52f)));
        }
        public void OnJoin(JoinEvent ev)
        {
            ev.Player.Role = RoleType.Spectator;
        }
    }
}
