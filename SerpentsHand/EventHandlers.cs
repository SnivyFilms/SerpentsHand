using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Exiled.Loader;
using MEC;
using PlayerRoles;
using Respawning;
using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.Events.EventArgs.Map;
using Respawning.Waves;

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

			Timing.KillCoroutines(calcuationCoroutine);

			calcuationCoroutine = Timing.RunCoroutine(SpawnCalculation());
		}

		private IEnumerator<float> SpawnCalculation()
		{
			while (true)
			{
				yield return Timing.WaitForSeconds(1f);

				if (Round.IsEnded)
					break;

				//if (Math.Round(Respawn.TimeUntilSpawnWave.TotalSeconds, 0) != Plugin.Instance.Config.SpawnWaveCalculation)
				//	continue;

				if (Respawn.IsSpawning)
					continue;

				if (Respawn.NextKnownSpawnableFaction == SpawnableFaction.ChaosMiniWave &&
				    Plugin.Instance.Config.SpawnManager.ShSpawnsDuringMiniWave)
				{
					Plugin.Instance.IsSpawnable = Loader.Random.Next(100) <= Plugin.Instance.Config.SpawnManager.SpawnChance &&
						Respawns >= Plugin.Instance.Config.SpawnManager.RespawnDelay &&
						SHRespawns < Plugin.Instance.Config.SpawnManager.MaxSpawns || Plugin.Instance.IsForced;
				}
				else if (Respawn.NextKnownSpawnableFaction == SpawnableFaction.ChaosWave)
				{
					Plugin.Instance.IsSpawnable = Loader.Random.Next(100) <= Plugin.Instance.Config.SpawnManager.SpawnChance &&
						Respawns >= Plugin.Instance.Config.SpawnManager.RespawnDelay &&
						SHRespawns < Plugin.Instance.Config.SpawnManager.MaxSpawns || Plugin.Instance.IsForced;
				}
					
			}
		}
		public void OnAnnouncingChaosEntrance(AnnouncingChaosEntranceEventArgs ev)
		{
			if (Plugin.Instance.IsSpawnable)
			{
				Cassie.Clear();
				ev.IsAllowed = false;
				return;
			}

			if (Plugin.Instance.Config.BlockChaosCassie)
			{
				Cassie.Clear();
				ev.IsAllowed = false;
			}
		}
		public void OnRespawningTeam(RespawningTeamEventArgs ev)
		{
			if (Plugin.Instance.IsSpawnable || Plugin.Instance.IsForced)
			{
				bool scpAlive = Player.List.Count(x => x.Role.Team == Team.SCPs) != 0;
				if (!scpAlive && !Plugin.Instance.Config.SpawnManager.CanSpawnWithoutScps)
					return;
				if (ev.Wave.IsMiniWave && !Plugin.Instance.Config.SpawnManager.ShSpawnsDuringMiniWave)
					return;
				
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
					Cassie.MessageTranslated(Plugin.Instance.Config.SpawnManager.EntryAnnoucement, Plugin.Instance.Config.SpawnManager.CassieText, isSubtitles: Plugin.Instance.Config.SpawnManager.Subtitles);

				if (Plugin.Instance.Config.SpawnManager.EntryBroadcast.Duration > 0 || !string.IsNullOrEmpty(Plugin.Instance.Config.SpawnManager.EntryBroadcast.Content))
					foreach (Player player in Player.List.Where(x => x.Role.Team == Team.SCPs))
						player.Broadcast(Plugin.Instance.Config.SpawnManager.EntryBroadcast);

				Plugin.Instance.IsSpawnable = false;
				Plugin.Instance.IsForced = false;
				ev.IsAllowed = false;
			}

			//Timing.CallDelayed(1 /*, UpdateCounter*/);
			
			Respawns++;
		}

		public void OnEndingRound(EndingRoundEventArgs ev)
		{
			if (_plugin.Config.SerpentsHand.TrackedPlayers.Count + _plugin.Config.SerpentsHandSpecialist.TrackedPlayers.Count + _plugin.Config.SerpentsHandLeader.TrackedPlayers.Count <= 0) return;
			
			if (ev.ClassList.mtf_and_guards != 0 || ev.ClassList.scientists != 0 || ev.ClassList.class_ds != 0) 
				ev.IsAllowed = false;
			else if (!_plugin.Config.SpawnManager.ScpsWinWithChaos && ev.ClassList.chaos_insurgents != 0)
			{
				ev.IsAllowed = ev.ClassList is { mtf_and_guards: 0, scientists: 0, scps_except_zombies: 0, zombies: 0, class_ds: 0};
			}
		}

		/*public void OnSpawned(SpawnedEventArgs ev)
		{
			if(ev.Player.IsCHI && ev.Reason != SpawnReason.Respawn)
				UpdateCounter();
		}

		private void UpdateCounter() =>
			RoundSummary.singleton.ExtraTargets = Plugin.Instance.Config.SpawnManager.ScpsWinWithChaos ? 0 : Player.List.Count(p => p.IsCHI);*/
	}
}