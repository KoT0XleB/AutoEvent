using AutoEvent.Functions;
using AutoEvent.Interfaces;
using Interactables.Interobjects.DoorUtils;
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

namespace AutoEvent.Events
{
    internal class DeathParty : IEvent
    {
        public string Name => "Смертельная Вечеринка";
        public string Description => "Избегайте гранат и выживите.";
        public string Color => "FFFF00";
        public string CommandName => "death";
        public static Model Model { get; set; }
        public static TimeSpan EventTime { get; set; }
        public static int CountRound = 1;
        public int Votes { get; set; }

        public void OnStart()
        {
            Plugin.IsEventRunning = true;
            Qurre.Events.Player.Damage += OnDamage;
            Qurre.Events.Player.Join += OnJoin;
            OnEventStarted();
        }
        public void OnStop()
        {
            Plugin.IsEventRunning = false;
            Qurre.Events.Player.Damage -= OnDamage;
            Qurre.Events.Player.Join -= OnJoin;
            Timing.CallDelayed(5f, () => EventEnd());
        }

        public void OnEventStarted()
        {
            CreatingMapFromJson("Death.json", new Vector3(145.18f, 945.26f, -122.97f), out var model);
            PlayAudio("FallGuys_FallRoll.f32le", 15, true, "Death");
            Model = model;
            WaitingEvent();
        }
        public void WaitingEvent()
        {
            // Делаем всех д классами
            Player.List.ToList().ForEach(player =>
            {
                player.Role = RoleType.ClassD;
                Timing.CallDelayed(2f, () =>
                {
                    player.Position = Model.GameObject.transform.position + RandomPosition();
                });
            });

            // Запуск ивента
            Timing.RunCoroutine(Cycle(), "death_time");
            // Гранатовый ивент
            Timing.RunCoroutine(GrenadeEvent(), "grenades");
        }
        public IEnumerator<float> Cycle()
        {
            // Обнуление таймера
            EventTime = new TimeSpan(0, 0, 0);
            // Отсчет обратного времени
            for (int time = 10; time > 0; time--)
            {
                BroadcastPlayers($"<size=100><color=red>{time}</color></size>", 1);
                yield return Timing.WaitForSeconds(1f);
            }
            var text = string.Empty;
            while (Player.List.ToList().Count(r => r.Role != RoleType.Spectator) > 1) // >= 0
            {
                if (EventTime.TotalSeconds % 2 == 0)
                {
                    text = "<size=90><color=red><b>《 ! 》</b></color></size>\n";
                }
                else
                {
                    text = "<size=90><color=red><b>  !  </b></color></size>\n";
                }

                BroadcastPlayers(text + $"<size=20><color=yellow>Осталось игроков {Player.List.ToList().Count(r => r.Role != RoleType.Spectator)}</color>\n" +
                    $"<color=red>Раунд {CountRound}: Время - {EventTime.Minutes}:{EventTime.Seconds}</color></size>", 1);
                yield return Timing.WaitForSeconds(1f);
                EventTime += TimeSpan.FromSeconds(1f);
            }
            if (Player.List.ToList().Count(r => r.Role != RoleType.Spectator) == 1)
            {
                var player = Player.List.ToList().First(r => r.Role != RoleType.Spectator);
                player.Hp = 1000;
                BroadcastPlayers($"<color=red>Смертельная вечеринка</color>\n" +
                    $"<color=yellow>ПОБЕДИТЕЛЬ - <color=red>{player.Nickname}</color></color>\n" +
                    $"<color=#ffc0cb>{EventTime.Minutes}:{EventTime.Seconds}</color>", 10);
            }
            else
            {
                BroadcastPlayers($"<color=red>Смертельная вечеринка</color>\n" +
                    $"<color=yellow>Все погибли((</color>\n" +
                    $"<color=#ffc0cb>{EventTime.Minutes}:{EventTime.Seconds}</color>", 10);
            }
            OnStop();
            yield break;
        }
        public IEnumerator<float> GrenadeEvent()
        {
            CountRound = 1;
            float fuse = 10f;
            float radius = 7f;
            float y = 20f;
            float count = 50;
            float time = 1f;

            while (Player.List.ToList().Count() > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    GrenadeFrag grenade = new GrenadeFrag(ItemType.GrenadeHE);
                    grenade.FuseTime = fuse;
                    grenade.MaxRadius = radius;
                    grenade.Spawn(Model.GameObject.transform.position + new Vector3(Random.Range(-38, 38), y, Random.Range(-38, 38)), new Vector3(0, 0, 0), new Vector3(5, 5, 5));
                    yield return Timing.WaitForSeconds(time);
                }
                yield return Timing.WaitForSeconds(20f);
                if (CountRound < 4)
                {
                    fuse -= 2f;
                    y -= 5f;
                    time -= 0.3f;
                }
                radius += 7f;
                count += 40;
                CountRound++;
            }
            yield break;
        }
        // Подведение итогов ивента и возврат в лобби
        public void EventEnd()
        {
            if (Audio.Microphone.IsRecording) StopAudio();
            Timing.RunCoroutine(DestroyObjects(Model));
            Timing.RunCoroutine(CleanUpAll());
        }
        // Манипуляции с дверьми и примитивами
        public Vector3 RandomPosition()
        {
            Vector3 position = new Vector3(0, 0, 0);
            var rand = Random.Range(0, 25);
            switch (rand)
            {
                case 0: position = new Vector3(0f, 5.56f, 0f); break;
                case 1: position = new Vector3(0f, 5.56f, -13.7f); break;
                case 2: position = new Vector3(0f, 5.56f, -26.9f); break;
                case 3: position = new Vector3(15.75f, 5.56f, -26.9f); break;
                case 4: position = new Vector3(15.75f, 5.56f, -15.3f); break;
                case 5: position = new Vector3(15.75f, 5.56f, -1.46f); break;
                case 6: position = new Vector3(15.75f, 5.56f, 11.74f); break;
                case 7: position = new Vector3(-0.1f, 5.56f, 11.74f); break;
                case 8: position = new Vector3(-0.1f, 5.56f, 28.1f); break;
                case 9: position = new Vector3(13.92f, 5.56f, 28.1f); break;
                case 10: position = new Vector3(31f, 5.56f, 28.1f); break;
                case 11: position = new Vector3(31f, 5.56f, 13.63f); break;
                case 12: position = new Vector3(31f, 5.56f, 1.39f); break;
                case 13: position = new Vector3(31f, 5.56f, -10.96f); break;
                case 14: position = new Vector3(31f, 5.56f, -25.36f); break;
                case 15: position = new Vector3(-14.26f, 5.56f, -26.73f); break;
                case 16: position = new Vector3(-14.26f, 5.56f, -12.21f); break;
                case 17: position = new Vector3(-14.26f, 5.56f, 2.25f); break;
                case 18: position = new Vector3(-14.26f, 5.56f, 14.09f); break;
                case 19: position = new Vector3(-14.26f, 5.56f, 28.4f); break;
                case 20: position = new Vector3(-26.7f, 5.56f, 28.4f); break;
                case 21: position = new Vector3(-26.7f, 5.56f, 13.9f); break;
                case 22: position = new Vector3(-26.7f, 5.56f, 2.24f); break;
                case 23: position = new Vector3(-26.7f, 5.56f, -11.82f); break;
                case 24: position = new Vector3(-26.7f, 5.56f, -27.22f); break;
            }
            return position;
        }
        // Ивенты
        public void OnJoin(JoinEvent ev)
        {
            ev.Player.Role = RoleType.Spectator;
        }
        public void OnDamage(DamageEvent ev)
        {
            if (ev.DamageType == DamageTypes.Explosion)
            {
                if (ev.Target.Hp < 1) ev.Target.Kill("<color=red>Взорвался</color>");
                ev.Target.Hp -= 50;
            }
        }
    }
}
