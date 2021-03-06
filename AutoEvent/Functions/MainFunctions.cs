using MEC;
using Qurre;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Controllers.Items;
using Qurre.API.Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace AutoEvent.Functions
{
    public static class MainFunctions
    {
        /// <summary>Очистить всех игроков.</summary>
        public static void ClearAllPlayers()
        {
            Player.List.ToList().ForEach(x => x.Kill());
        }

        /// <summary>Переспавнить игрока без телепортиции на точку спавна.</summary>
        public static void BlockAndChangeRolePlayer(Player player, RoleType role)
        {
            player.BlockSpawnTeleport = true; // Блокирует телепортацию при спавне
            player.Role = role;               // Меняем роль
        }

        public static void TeleportPlayersToPosition(List<Player> players, Vector3 pos) // input -> Player.List.ToList(), Position
        {
            players.ForEach(player => player.Position = pos);
        }

        public static void TeleportAndChangeRolePlayers(List<Player> players, RoleType type, Vector3 pos) // input -> Player.List.ToList(), Position
        {
            players.ForEach(player =>
            {
                player.Role = type;
                Timing.CallDelayed(2f, () =>
                {
                    player.Position = pos;
                });
            });
        }

        /// <summary>Переспавнить игроков без телепортиции на точку спавна.</summary>
        public static void BlockAndChangeRolePlayers(List<Player> players, RoleType role)
        {
            players.ForEach(x => BlockAndChangeRolePlayer(x, role));
        }

        /// <summary>Проиграть аудиофайл</summary>
        public static void PlayAudio(string path, byte volume, bool loop, string eventName)
        {
            Audio.PlayFromFile(Path.Combine(Path.Combine(Qurre.PluginManager.PluginsDirectory, "Audio"), path), volume, false, loop, 1920, 48000, eventName);
        }

        /// <summary>Остановить прогирывание</summary>
        public static void StopAudio()
        {
            Audio.Microphone.Skip();
        }

        /// <summary>Написать броадкаст всем игрокам с предвадительной очисткой очереди</summary>
        public static void BroadcastPlayers(string message, ushort time)
        {
            Player.List.ToList().ForEach(player =>
            {
                player.ClearBroadcasts();
                player.Broadcast(message, time);
            });
        }

        /// <summary>Написать сообщение всем игрокам в консоль</summary>
        public static void ConsolePlayers(string message)
        {
            Player.List.ToList().ForEach(x => x.SendConsoleMessage(message, "white"));
        }

        /// <summary>Загрузить карту из JSON</summary>
        public static void CreatingMapFromJson(string path, Vector3 pos, out Model model)
        {
            SchematicUnity.SchematicUnity.Load(Path.Combine(Path.Combine(Qurre.PluginManager.PluginsDirectory, "Map"), path), pos, out Model _model);
            model = _model;
        }

        /// <summary>Подготовить игру к ивенту</summary>
        public static void StartEventParametres()
        {
            Round.LobbyLock = true;
            Round.Lock = true;
            Round.Start();
        }

        /// <summary>Подготовить игру к нормальному режиму</summary>
        public static void EndEventParametres()
        {
            Round.LobbyLock = false;
            Round.Lock = false;

            ClearAllPlayers();

            Round.End();
        }
        public static IEnumerator<float> TimingBeginBroadcastPlayers(string eventName, float time) // time = 30
        {
            while (time > 0)
            {
                Map.ClearBroadcasts();
                Map.Broadcast($"<color=#D71868><b><i>{eventName}</i></b></color>\n<color=#ABF000>До начала ивента осталось <color=red>{time}</color> секунд.</color>", 1);

                yield return Timing.WaitForSeconds(1f);
                time--;
            }
            yield break;
        }
        /// <summary>Выдача всем игрокам эффектов.</summary>
        public static void EnablePlayersEffect(List<Player> players, EffectType effect)
        {
            players.ForEach(x => x.EnableEffect(effect));
        }
        /// <summary>Убрать у всех игроков выданные эффекты.</summary>
        public static void DisablePlayersEffect(List<Player> players, EffectType effect)
        {
            players.ForEach(x => x.DisableEffect(effect));
        }
        /// <summary>Очистка мусора после ивента.</summary>
        public static IEnumerator<float> CleanUpAll()
        {
            //Task.Run(() => { });
            foreach (Pickup pickup in Map.Pickups)
            {
                pickup.Base.DestroySelf();
            }
            foreach (Ragdoll ragdoll in UnityEngine.Object.FindObjectsOfType<Ragdoll>())
            {
                Object.Destroy(ragdoll.gameObject);
            }
            yield break;
        }
        /// <summary>Очистка карты после ивента.</summary>
        public static IEnumerator<float> DestroyObjects(Model model)
        {
            //Task.Run(() => { });
            foreach (var prim in model.Primitives)
            {
                GameObject.Destroy(prim.GameObject);
            }
            foreach (var light in model.Lights)
            {
                GameObject.Destroy(light.GameObject);
            }
            model.Destroy();
            yield break;
        }
        public static bool IsHuman(Player player) => player.Team != Team.SCP && player.Team != Team.RIP;
        public static int HumanCount = Player.List.Count(r => r.Team != Team.SCP && r.Team != Team.RIP);
    }
}