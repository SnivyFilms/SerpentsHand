using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using Exiled.Loader;
using MEC;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using Exiled.Events.EventArgs.Map;
using Player = Exiled.API.Features.Player;

namespace SerpentsHand
{
	internal sealed class EventHandlers
	{
		private Plugin _plugin;
        public EventHandlers(Plugin plugin) => _plugin = plugin;

		private int Respawns;
		private int SHRespawns;
        private CoroutineHandle calcuationCoroutine;

		public void OnRoundStarted()
		{
			Plugin.Instance.IsSpawnable = false;
			Plugin.Instance.IsForced = false;
			Respawns = 0;
			SHRespawns = 0;
		}
		
		public void OnAnnouncingChaosEntrance(AnnouncingChaosEntranceEventArgs ev)
		{
			if (Plugin.Instance.IsSpawnable)
			{
				ev.IsAllowed = false;
				LabApi.Features.Wrappers.Announcer.Clear();
				return;
			}

			if (Plugin.Instance.Config.BlockChaosCassie)
			{
				ev.IsAllowed = false;
				LabApi.Features.Wrappers.Announcer.Clear();
			}
		}

		public void OnRespawningTeam(RespawningTeamEventArgs ev)
		{
			if (ev.Wave.Faction == Faction.FoundationEnemy)
			{

				bool forced = Plugin.Instance.IsForced;
				bool hasScpAlive = Player.List.Any(x => x.Role.Team == Team.SCPs);
				bool miniWaveBlocked =
					ev.Wave.IsMiniWave && !Plugin.Instance.Config.SpawnManager.ShSpawnsDuringMiniWave;
				bool conditionsMet = !forced &&
				                     Respawns >= Plugin.Instance.Config.SpawnManager.RespawnDelay &&
				                     SHRespawns < Plugin.Instance.Config.SpawnManager.MaxSpawns &&
				                     Loader.Random.Next(101) < Plugin.Instance.Config.SpawnManager.SpawnChance &&
				                     (hasScpAlive || Plugin.Instance.Config.SpawnManager.CanSpawnWithoutScps) &&
				                     miniWaveBlocked;

				if (!forced && !conditionsMet)
					return;

				Log.Debug("Actually Spawning Serpents");
				Plugin.Instance.IsSpawnable = true;
				List<Player> players = ev.Players.GetRange(0,
					ev.Players.Count > Plugin.Instance.Config.SpawnManager.MaxSquad
						? Plugin.Instance.Config.SpawnManager.MaxSquad
						: ev.Players.Count);
				int SerpentsSpawnCount = 0;
				foreach (Player player in players)
				{
					if (player is null)
						continue;

					if (SerpentsSpawnCount == 0)
					{
						Plugin.Instance.Config.SerpentsHandLeader.AddRole(player);
						SerpentsSpawnCount++;
					}
					else if (SerpentsSpawnCount >= 1 && SerpentsSpawnCount <= 3)
					{
						Plugin.Instance.Config.SerpentsHandSpecialist.AddRole(player);
						SerpentsSpawnCount++;
					}
					else
					{
						Plugin.Instance.Config.SerpentsHand.AddRole(player);
					}
				}

				SHRespawns++;
				if (!string.IsNullOrEmpty(Plugin.Instance.Config.SpawnManager.EntryAnnoucement))
					LabApi.Features.Wrappers.Announcer.Message(Plugin.Instance.Config.SpawnManager.EntryAnnoucement,
						Plugin.Instance.Config.SpawnManager.CassieText);

				if (Plugin.Instance.Config.SpawnManager.EntryBroadcast.Duration > 0 ||
				    !string.IsNullOrEmpty(Plugin.Instance.Config.SpawnManager.EntryBroadcast.Content))
					foreach (Player player in Player.List.Where(x => x.Role.Team == Team.SCPs))
						player.Broadcast(Plugin.Instance.Config.SpawnManager.EntryBroadcast);

				Plugin.Instance.IsSpawnable = false;
				Plugin.Instance.IsForced = false;
				ev.IsAllowed = false;
			}
			Respawns++;
		}

		public void OnEndingRound(EndingRoundEventArgs ev)
		{
			if (_plugin.Config.SerpentsHand.TrackedPlayers.Count +
			    _plugin.Config.SerpentsHandSpecialist.TrackedPlayers.Count +
			    _plugin.Config.SerpentsHandLeader.TrackedPlayers.Count <= 0) return;

			if (ev.ClassList.mtf_and_guards != 0 || ev.ClassList.scientists != 0 || ev.ClassList.class_ds != 0)
				ev.IsAllowed = false;
			else if (!_plugin.Config.SpawnManager.ScpsWinWithChaos && ev.ClassList.chaos_insurgents != 0)
			{
				ev.IsAllowed = ev.ClassList is
					{ mtf_and_guards: 0, scientists: 0, scps_except_zombies: 0, zombies: 0, class_ds: 0 };
			}
		}
	}
}