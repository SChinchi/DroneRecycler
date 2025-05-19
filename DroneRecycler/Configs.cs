using BepInEx.Bootstrap;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DroneRecycler;

internal class Configs
{
    internal static ConfigEntry<KeyboardShortcut> CommandKey { get; private set; }
    internal static ConfigEntry<bool> IsCommandManual { get; private set; }

    internal static void Init(ConfigFile config, string assemblyLocation)
    {
        CommandKey = config.Bind("Settings", "Command Key", new KeyboardShortcut(KeyCode.C), "Which key to use to manually issue or revoke the recycling command.");
        IsCommandManual = config.Bind("Settings", "Is Command Manual", false, "Whether assigning an Equipment Drone to recycle an item is done by pressing a key (see 'Command Key') after pinging a pickup or it is done automatically. If this is enabled, the 'Command Key' can further be used to revoke the command.");

        if (Chainloader.PluginInfos.ContainsKey(DroneRecycler.RiskOfOptionsGUID))
        {
            RiskOfOptionsInit(assemblyLocation);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void RiskOfOptionsInit(string assemblyLocation)
    {
        FileInfo iconFile = null;
        var files = new DirectoryInfo(Path.GetDirectoryName(assemblyLocation)).GetFiles("icon.png", SearchOption.TopDirectoryOnly);
        if (files != null && files.Length > 0)
        {
            iconFile = files[0];
        }
        if (iconFile != null)
        {
            var name = $"{DroneRecycler.PluginName}Icon";
            var texture = new Texture2D(256, 256);
            texture.name = name;
            if (texture.LoadImage(File.ReadAllBytes(iconFile.FullName)))
            {
                var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                sprite.name = name;
                ModSettingsManager.SetModIcon(sprite, DroneRecycler.PluginGUID, DroneRecycler.PluginName);
            }
        }

        ModSettingsManager.AddOption(new CheckBoxOption(IsCommandManual));
        ModSettingsManager.AddOption(new KeyBindOption(CommandKey));
    }
}
