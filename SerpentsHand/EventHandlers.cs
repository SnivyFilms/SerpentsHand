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
			if (ev.Wave.Faction != Faction.FoundationEnemy)
			{
				Log.Debug("Returning since its not a Chaos Spawn");
				return;
			}

			if (Respawns < Plugin.Instance.Config.SpawnManager.RespawnDelay)
			{
				Log.Debug($"Respawn delay not reached yet. Respawns: {Respawns}/{Plugin.Instance.Config.SpawnManager.RespawnDelay}");
				Respawns++;
				return;
			}
			Respawns++;

			if (Plugin.Instance.Config.SpawnManager.MaxSpawns <= SHRespawns)
			{
				Log.Debug("Not spawning Serpents because max spawns have been reached.");
				return;
			}

			int randomChance = Loader.Random.Next(101);
			if (Plugin.Instance.Config.SpawnManager.SpawnChance <= randomChance)
			{
				Log.Debug($"Random roll failed. SpawnChance: {Plugin.Instance.Config.SpawnManager.SpawnChance}, Random: {randomChance}");
				return;
			}

			bool scpAlive = Player.List.Count(x => x.Role.Team == Team.SCPs) != 0;
			if (!scpAlive && !Plugin.Instance.Config.SpawnManager.CanSpawnWithoutScps)
			{
				Log.Debug("Not spawning Serpents because there are no SCPs alive and CanSpawnWithoutScps is false");
				return;
			}

			if (ev.Wave.IsMiniWave && !Plugin.Instance.Config.SpawnManager.ShSpawnsDuringMiniWave)
			{
				Log.Debug("Not spawning Serpents because it's a mini wave and ShSpawnsDuringMiniWave is false");
				return;
			}

			Log.Debug($"Random Chance: {randomChance}, SpawnChance: {Plugin.Instance.Config.SpawnManager.SpawnChance}");
			Log.Debug("Actually Spawning Serpents");
			Plugin.Instance.IsSpawnable = true;
			List<Player> players = ev.Players.GetRange(0, ev.Players.Count > Plugin.Instance.Config.SpawnManager.MaxSquad 
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
				LabApi.Features.Wrappers.Announcer.Message(Plugin.Instance.Config.SpawnManager.EntryAnnoucement, Plugin.Instance.Config.SpawnManager.CassieText);

			if (Plugin.Instance.Config.SpawnManager.EntryBroadcast.Duration > 0 || !string.IsNullOrEmpty(Plugin.Instance.Config.SpawnManager.EntryBroadcast.Content))
				foreach (Player player in Player.List.Where(x => x.Role.Team == Team.SCPs))
					player.Broadcast(Plugin.Instance.Config.SpawnManager.EntryBroadcast);

			Plugin.Instance.IsSpawnable = false;
			Plugin.Instance.IsForced = false;
			ev.IsAllowed = false;
		}
		
		public void OnWaveSpawned(RespawnedTeamEventArgs ev)
		{
			if (Plugin.Instance.IsSpawnable)
				Plugin.Instance.IsSpawnable = false;
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