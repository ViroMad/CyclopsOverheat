#define BepInEX_Config // Switch to BepInEX ONLY

#if BepInEX_Config
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace SubOverheat
{
	public class ConfigSettings
	{
		public static ConfigEntry<bool> OverheatOveride { get; set; }
		public static ConfigEntry<int> OverheatTime { get; set; }
		public static ConfigEntry<bool> OverheatNotify { get; set; }
		
		public ConfigSettings(ConfigFile Config)
        {
			OverheatOveride = Config.Bind("General", "Enable Alternate Timer", true, new ConfigDescription("If this is not on the game uses the default timer with its random chance of catching fire."));
			OverheatTime = Config.Bind("General", "Overheat Timer", 10, new ConfigDescription("How many game ticks you can drive without overheating.", new AcceptableValueRange<int>(5, 20)));
			OverheatNotify = Config.Bind("General", "Overheat Level Notification", true, new ConfigDescription("If alternate timer enabled this gives you a overheat percent, otherwise it just tells you the heat level. After heat level 3, the random chance of fire kicks in."));
		}
	}
}
#else
using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using UnityEngine;

namespace SubOverheat
{
	[Menu("Sub Overheat")]
	//[Menu("Sub Overheat", LoadOn = (MenuAttribute.LoadEvents.MenuRegistered | MenuAttribute.LoadEvents.MenuOpened))]
	
	public class ConfigSettings : ConfigFile
	{
		//public MenuAttribute.LoadEvents LoadOn { get; set; }
		public MenuAttribute.LoadEvents LoadOn => (MenuAttribute.LoadEvents.MenuRegistered | MenuAttribute.LoadEvents.MenuOpened);

		[Toggle("Enable Alternate Timer", Tooltip = "If this is not on the game uses the default timer with its random chance of catching fire.")]
		public bool OverheatOveride = true;

		[Slider("Overheat Timer", 5f, 20f, DefaultValue = 10f, Step = 1f, Tooltip = "How many game ticks you can drive without overheating.")]
		public int OverheatTime = 10;

		[Toggle("Overheat Level Notification", Tooltip = "If alternate timer enabled this gives you a overheat percent, otherwise it just tells you the heat level. After heat level 3, the random chance of fire kicks in.")]
		public bool OverheatNotify = true;
	}	
}
#endif