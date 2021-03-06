using AutoEvent.Functions;
using AutoEvent.Interfaces;
using MEC;
using Mirror;
using Qurre;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Events;
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
    internal class Glass : IEvent
    {
        public string Name => "Прыжок Веры";
        public string Description => "Пропрыгайте в конец карты через препятствия.";
        public string Color => "FF4242";
        public string CommandName => "glass";
        public static Model Model { get; set; }
        public static Model Platformes { get; set; }
        public static Model ModelCheckPoint { get; set; }
        public static TimeSpan EventTime { get; set; }
        public int Votes { get; set; }

        public void OnStart()
        {
            Plugin.IsEventRunning = true;
            Qurre.Events.Player.Join += OnJoin;
            //Qurre.Events.Voice.PressAltChat += OnPressV;
            OnWaitingEvent();
        }
        public void OnStop()
        {
            Plugin.IsEventRunning = false;
            Qurre.Events.Player.Join -= OnJoin;
            //Qurre.Events.Voice.PressAltChat -= OnPressV;
            Timing.CallDelayed(5f, () => EventEnd());
        }
        public void OnWaitingEvent()
        {
            EventTime = new TimeSpan(0, 0, 0);

            CreatingMapFromJson("Glass.json", new Vector3(140f, 930f, -122.97f), out var model);
            Model = model;

            CreateCheckPoint();

            foreach(Player player in Player.List)
            {
                player.GameObject.AddComponent<BoxCollider>();
                player.GameObject.AddComponent<BoxCollider>().size = new Vector3(1f, 3.5f, 1f);
            }

            PlayAudio("FallGuys_BeanThieves.f32le", 10, true, "STEKLO");
            OnEventStarted(Player.List.ToList());
        }
        public void OnEventStarted(List<Player> players)
        {
            EventTime = new TimeSpan(0, 0, 0);
            // Телепортируем игроков в листе на одну из трёх платформ
            TeleportAndChangeRolePlayers(players, RoleType.ClassD, Model.GameObject.transform.position + new Vector3(0f, 36.17f, -29.33f));
            // Создаем новые платформы
            CreatePlatformes();
            Timing.RunCoroutine(Cycle(), "glass_time");
        }
        // Отсчет до начала ивента
        public IEnumerator<float> Cycle()
        {
            // Обнуление таймера
            EventTime = new TimeSpan(0, 1, 0);
            while (EventTime.TotalSeconds > 0)
            {
                int count = Player.List.ToList().Count(r => r.Role != RoleType.Spectator);

                // Если все сдохли, то обнуляем;
                if (count <= 0) EventTime = new TimeSpan(0, 0, 0);

                BroadcastPlayers($"<size=50>Прыжок Веры\n" +
                    $"Пройдите до конца уровня!</size>\n" +
                    $"<size=20>Игроков: {count} | <color=red>До конца: {EventTime.Minutes}:{EventTime.Seconds} секунд</color></size>", 1);

                yield return Timing.WaitForSeconds(1f);
                EventTime -= TimeSpan.FromSeconds(1f);
            }
            // Проверка нахождения на примитиве
            foreach (Player player in Player.List.ToList())
            {
                if (Vector3.Distance(ModelCheckPoint.GameObject.transform.position, player.Position) >= 8)
                {
                    player.Role = RoleType.Spectator;
                }
            }

            if (Player.List.ToList().Count(r => r.Role != RoleType.Spectator) > 1)
            {
                BroadcastPlayers($"Прыжок Веры\n" +
                    $"Осталось много игроков.\n" +
                    $"Рестарт Ивента!", 5);
                // рестарт мини-игры
                RestartEvent();
                yield break;
            }
            else if (Player.List.ToList().Count(r => r.Role != RoleType.Spectator) == 1)
            {
                // Победитель
                BroadcastPlayers($"Прыжок Веры\n" +
                    $"<color=yellow>ПОБЕДИТЕЛЬ {Player.List.ToList().First(r => r.Role != RoleType.Spectator).Nickname}</color>", 10);
            }
            else if (Player.List.ToList().Count(r => r.Role != RoleType.Spectator) == 0)
            {
                // Все проиграли
                BroadcastPlayers($"Прыжок Веры\n" +
                    $"<color=red>Все погибли)))))))</color>", 10);
            }
            OnStop();
            yield break;
        }
        public void CreateCheckPoint()
        {
            Model checkPoint = new Model("check", Model.GameObject.transform.position + new Vector3(0, 35f, 0f));
            checkPoint.AddPart(new ModelPrimitive(checkPoint, PrimitiveType.Cube, new Color32(113, 69, 69, 255), Vector3.zero, Vector3.zero, new Vector3(12, 1, 5)));
            checkPoint.AddPart(new ModelPrimitive(checkPoint, PrimitiveType.Cube, new Color32(113, 69, 69, 255), new Vector3(5.5f, 0.99f, 0.5f), new Vector3(0, 90, 0), new Vector3(4, 1, 1)));
            checkPoint.AddPart(new ModelPrimitive(checkPoint, PrimitiveType.Cube, new Color32(113, 69, 69, 255), new Vector3(-5.5f, 0.99f, 0.5f), new Vector3(0, 90, 0), new Vector3(4, 1, 1)));
            checkPoint.AddPart(new ModelPrimitive(checkPoint, PrimitiveType.Cube, new Color32(113, 69, 69, 255), new Vector3(0f, 0.99f, 2f), new Vector3(0, 0, 0), new Vector3(10.4f, 1, 1)));
            ModelCheckPoint = checkPoint;
        }
        public void CreatePlatformes()
        {
            Platformes = new Model("platforme", Model.GameObject.transform.position);
            int parkourNumber = 1;
            int playerCount = Player.List.ToList().Count(r => r.Role != RoleType.Spectator);

            if (playerCount <= 5) parkourNumber = 1;
            else if (playerCount > 5 && playerCount <= 15) parkourNumber = 2;
            else if (playerCount > 15) parkourNumber = 3;

            Vector3 pos = new Vector3(0f, 35f, -16.23f); // -2.3 2.3
            Vector3 delta = new Vector3(0f, 0f, 4.77f);
            for (int i = 0; i < parkourNumber * 3; i++) // 3 6 9
            {
                var model = new ModelPrimitive(Platformes, PrimitiveType.Cube, new Color32(153, 153, 153, 255), pos + new Vector3(-2.3f, 0, 0), Vector3.zero, new Vector3(3, 0.2f, 3));
                var model1 = new ModelPrimitive(Platformes, PrimitiveType.Cube, new Color32(153, 153, 153, 255), pos + new Vector3(2.3f, 0, 0), Vector3.zero, new Vector3(3, 0.2f, 3));

                if (Random.Range(0, 2) == 0) model.GameObject.AddComponent<GlassComponent>();
                else model1.GameObject.AddComponent<GlassComponent>();

                Platformes.AddPart(model);
                Platformes.AddPart(model1);
                pos += delta;
            }
            ModelCheckPoint.GameObject.transform.position = Model.GameObject.transform.position + pos + new Vector3(0, 0, 2);
        }
        //Рестарт ивента
        public void RestartEvent()
        {
            // Ожидаем рестарта
            Timing.CallDelayed(5f, () =>
            {
                // Киляем платформы
                GameObject.Destroy(Platformes.GameObject);
                // Запуск Ивента
                OnEventStarted(Player.List.Where(r => r.Role != RoleType.Spectator).ToList());
            });
        }
        // Подведение итогов ивента и возврат в лобби
        public void EventEnd()
        {
            if (Audio.Microphone.IsRecording) StopAudio();
            Timing.RunCoroutine(DestroyObjects(Platformes));
            Timing.RunCoroutine(DestroyObjects(ModelCheckPoint));
            Timing.RunCoroutine(DestroyObjects(Model));
            Timing.RunCoroutine(CleanUpAll());
        }
        // Ивенты
        public void OnJoin(JoinEvent ev)
        {
            ev.Player.Role = RoleType.Spectator;
        }
        public void OnPressV(PressAltChatEvent ev)
        {
            if (ev.Value)
            {
                foreach(Player player in Player.List)
                {
                    if (ev.Player != player)
                    {
                        ev.Player.GameObject.TryGetComponent<Rigidbody>(out Rigidbody rig);
                        rig.AddForce(player.Transform.forward * 0.5f, ForceMode.Impulse);
                    }
                }
            }
        }
    }
}
