using System.ComponentModel;

namespace SerpentsHand.Configs
{
    public class SpawnManager
    {
        public float SpawnChance { get; set; } = 75f;
        [Description("The maximum size of a Serpents Hand squad.")]
        public int MaxSquad { get; set; } = 8;

        [Description("How many respawn waves must occur before considering Serpents Hand to spawn.")]
        public int RespawnDelay { get; set; } = 1;

        [Description("The maximum number of times Serpents can spawn per game.")]
        public int MaxSpawns { get; set; } = 1;

        [Description("Should Tutorial automaticly be converted to Serpends Hand?")]
        public bool AutoConvertTutorial { get; set; } = false;

        [Description("Determines if Serpents Hand should be able to spawn when there is no SCPs.")]
        public bool CanSpawnWithoutScps { get; set; } = false;

        [Description("Set this to false if Chaos and SCPs CANNOT win together on your server")]
        public bool ScpsWinWithChaos { get; set; } = false;

        [Description("The message annouced by CASSIE when Serpents hand spawn. (Empty = Disabled)")]
        public string EntryAnnoucement { get; set; } = "SERPENTS HAND HASENTERED";

        [Description("The CASSIE text that is shown to the player, Subtitles must be set to true")]
        public string CassieText { get; set; } = "Serpents Hand has entered the facility";

        [Description("The broadcast shown to SCPs when the Serpents Hand respawns.")]
        public Exiled.API.Features.Broadcast EntryBroadcast { get; set; } = new Exiled.API.Features.Broadcast("<color=orange>Serpents Hand has entered the facility!</color>");
        
        [Description("Can SH Spawn during a Mini-CI Wave?")]
        public bool ShSpawnsDuringMiniWave { get; set; } = false;
    }
}
