using BepInEx.Configuration;
using UnityEngine;

namespace DroneRecycler;

internal class Configs
{
    internal static ConfigEntry<KeyCode> CommandKey { get; private set; }
    internal static ConfigEntry<bool> IsCommandManual { get; private set; }

    internal static void Init(ConfigFile config)
    {
        CommandKey = config.Bind("Settings", "Command Key", KeyCode.C, "Which key to use to manually issue or revoke the recycling command.");
        IsCommandManual = config.Bind("Settings", "Is Command Manual", false, "Whether assigning an Equipment Drone to recycle an item is done by pressing a key (see 'Command Key') after pinging a pickup or it is done automatically. If this is enabled, the 'Command Key' can further be used to revoke the command.");
    }
}
