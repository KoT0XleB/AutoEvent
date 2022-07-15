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

namespace AutoEvent.Events
{
    internal class Lava : IEvent
    {
        public string Name => "Пол - это ЛАВА";
        public string Description => "Выживание, в котором необходимо избегать лавы и стрелять в других.";
        public string Color => "FFFF00";
        public string CommandName => "lava";
        public Model Model { get; set; }
        public Model LavaModel { get; set; }
        public TimeSpan EventTime { get; set; }
        public int Votes { get; set; }

        public void OnStart()
        {
            Plugin.IsEventRunning = true;
            Qurre.Events.Player.Damage += OnDamage;
            Qurre.Events.Player.RagdollSpawn += OnRagdollSpawn;
            Qurre.Events.Player.Join += OnJoin;

            OnEventStarted();
        }
        public void OnStop()
        {
            Plugin.IsEventRunning = false;
            Qurre.Events.Player.Damage -= OnDamage;
            Qurre.Events.Player.RagdollSpawn -= OnRagdollSpawn;
            Qurre.Events.Player.Join -= OnJoin;
            Timing.CallDelayed(5f, () => EventEnd());
        }
        public void OnEventStarted()
        {
            // Обнуление Таймера
            EventTime = new TimeSpan(0, 0, 0);
            // Создание карты
            CreatingMapFromJson("Lava.json", new Vector3(145.18f, 930f, -122.97f), out var model);
            Model = model;
            Server.FriendlyFire = true;
            // Запуск музыки
            PlayAudio("FallGuys_DnB.f32le", 10, true, "LavaAudio");
            for (int i = 0; i < 20; i++)
            {
                var item = ItemType.GunCOM15;
                var rand = Random.Range(0, 100);
                if (rand < 40) item = ItemType.GunCOM15;
                else if (rand >= 40 && rand < 80) item = ItemType.GunCOM18;
                else if (rand >= 80 && rand < 90) item = ItemType.GunRevolver;
                else if (rand >= 90 && rand < 100) item = ItemType.GunFSP9;
                Pickup pickup = new Item(item).Spawn(Model.GameObject.transform.position + new Vector3(Random.Range(-30, 31), 30, Random.Range(-30, 31)));
            }
            Timing.CallDelayed(2f, () =>
            {
                // Делаем всех д классами
                foreach (var player in Qurre.API.Player.List)
                {
                    player.Role = RoleType.ClassD;
                    player.EnableEffect(EffectType.Ensnared);
                    Timing.CallDelayed(2f, () =>
                    {
                        player.Position = Model.GameObject.transform.position + RandomPlayerPosition();
                    });
                }
            });
            // Запуск ивента
            Timing.RunCoroutine(Cycle(), "lava_time");
        }
        public IEnumerator<float> Cycle()
        {
            // Отсчет обратного времени
            for (int time = 10; time > 0; time--)
            {
                BroadcastPlayers($"<size=100><color=red>{time}</color></size>", 1);
                yield return Timing.WaitForSeconds(1f);
            }
            Player.List.ToList().ForEach(player =>
            {
                player.DisableEffect(EffectType.Ensnared);
                player.GameObject.AddComponent<BoxCollider>();
                player.GameObject.AddComponent<BoxCollider>().size = new Vector3(1f, 3f, 1f);
            });

            // Создание лавы
            LavaModel = new Model("lava", Model.GameObject.transform.position);
            LavaModel.AddPart(new ModelPrimitive(LavaModel, PrimitiveType.Cube, new Color32(255, 0, 0, 255), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(100, 1, 100)));
            foreach (var prim in LavaModel.Primitives)
            {
                prim.GameObject.AddComponent<LavaComponent>();
            }

            while (Player.List.ToList().Count(r => r.Role != RoleType.Spectator) > 1)
            {
                string text = string.Empty;
                if (EventTime.TotalSeconds % 2 == 0)
                {
                    text = "<size=90><color=red><b>《 ! 》</b></color></size>\n";
                }
                else
                {
                    text = "<size=90><color=red><b>  !  </b></color></size>\n";
                }
                BroadcastPlayers(text + $"<size=20><color=red><b>Живых: {Player.List.ToList().Count(r => r.Role != RoleType.Spectator)} Игроков</b></color></size>", 1);
                LavaModel.GameObject.transform.position += new Vector3(0, 0.1f, 0);
                yield return Timing.WaitForSeconds(1f);
                EventTime += TimeSpan.FromSeconds(1f);
            }
            if (Player.List.ToList().Count(r => r.Role != RoleType.Spectator) == 1)
            {
                BroadcastPlayers($"<size=80><color=red><b>Победитель\n{Player.List.ToList().First(r => r.Role != RoleType.Spectator).Nickname}</b></color></size>", 10);
            }
            else
            {
                BroadcastPlayers($"<size=70><color=red><b>Все утонули в Лаве)))))</b></color></size>", 10);
            }
            OnStop();
            yield break;
        }
        public void EventEnd()
        {
            if (Audio.Microphone.IsRecording) StopAudio();
            Server.FriendlyFire = false;
            Timing.RunCoroutine(DestroyObjects(Model));
            Timing.RunCoroutine(DestroyObjects(LavaModel));
            Timing.RunCoroutine(CleanUpAll());
        }
        // Рандомная позиция игрока
        public Vector3 RandomPlayerPosition()
        {
            Vector3 pos = new Vector3(0f, 0f, 0f);
            switch (Random.Range(0, 10))
            {
                case 0: pos = new Vector3(-13.43136f, 2.6f, 25.6032f); break; // 2.52
                case 1: pos = new Vector3(2.91f, 2.6f, 16.35f); break;
                case 2: pos = new Vector3(18.59f, 2.6f, 23.39f); break;
                case 3: pos = new Vector3(27.92f, 2.6f, 7.71f); break;
                case 4: pos = new Vector3(15.34f, 2.6f, -0.27f); break;
                case 5: pos = new Vector3(18.02f, 2.6f, -23.75f); break;
                case 6: pos = new Vector3(9.88f, 2.6f, -12.95f); break;
                case 7: pos = new Vector3(0.11f, 2.6f, -16.04f); break;
                case 8: pos = new Vector3(-13.26f, 2.6f, -14.57f); break;
                case 9: pos = new Vector3(-26.03f, 2.6f, -9.07f); break;
                case 10: pos = new Vector3(-14.85f, 2.6f, 4.35f); break;
                case 11: pos = new Vector3(-24.42f, 2.6f, 16.25f); break;
                case 12: pos = new Vector3(-3.49f, 2.6f, 29.02f); break;
            }
            return pos;
        }
        // Рандомная позиция пушек
        public List<Vector3> RandomGunPosition { get; set; } = new List<Vector3>()
        {
            new Vector3(-16.077f, 5.86f, 22.593f),
            new Vector3(5.484f, 10.339f, 22.564f),
            new Vector3(22.36f, 4.73f, 8.22f),
            new Vector3(18.904f, 9.852f, 11.129f),
            new Vector3(0.2160301f, 15.04f, 0.1276894f),
            new Vector3(9.688f, 19.16f, -24.429f),
            new Vector3(-16.69451f, 4.02f, -24.33174f),
            new Vector3(-20.75f, 4.36f, -5.99f),
            new Vector3(25.351f, 7.429f, -6.61f),
            new Vector3(24.05f, 13.53f, -11.1f),
            new Vector3(9.58f, 13.53f, -22.4f),
            new Vector3(-24.58f, 6.43f, 4.68f)
        };
        // Ивенты
        public void OnJoin(JoinEvent ev)
        {
            ev.Player.Role = RoleType.Spectator;
        }
        public void OnDamage(DamageEvent ev)
        {
            if (ev.Target.Role != RoleType.Spectator) ev.Amount = 3.5f;
        }
        public void OnRagdollSpawn(RagdollSpawnEvent ev)
        {
            ev.Allowed = false;
        }
    }
}
