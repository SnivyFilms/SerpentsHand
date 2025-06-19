using Exiled.API.Interfaces;
using System.ComponentModel;

namespace SerpentsHand.Configs
{
    public sealed class Config : IConfig
    {
        [Description("Whether or not the plugin is enabled")]
        public bool IsEnabled { get; set; } = true;

        [Description("Whether or not debug messages will be shown")]
        public bool Debug { get; set; } = false;

        [Description("Whether or not to block Chaos Entry Announcements")]
        public bool BlockChaosCassie { get; set; } = true;

        [Description("How many seconds before a spawnwave occurs should it calculate the spawn chance. This value **MUST** be lower than 13")]
        public int SpawnWaveCalculation { get; set; } = 10;
        
        public Roles.SerpentsHandLeader SerpentsHandLeader { get; set;} = new();
        public Roles.SerpentsHandSpecialist SerpentsHandSpecialist { get; set; } = new();
        public Roles.SerpentsHand SerpentsHand { get; set; } = new();
        public Configs.SpawnManager SpawnManager { get; set; } = new();
    }
}
