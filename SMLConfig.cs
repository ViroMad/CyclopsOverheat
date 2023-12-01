using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;

namespace SubOverheat
{
	[Menu("Sub Overheat", LoadOn = (MenuAttribute.LoadEvents.MenuRegistered | MenuAttribute.LoadEvents.MenuOpened))]
	public class SMLConfig : ConfigFile
	{
		[Toggle("Enable Alternate Timer", Tooltip = "If this is not on the game uses the default timer with its random chance of catching fire.")]
		public bool OverheatOveride = true;

		[Slider("Overheat Timer", 5f, 20f, DefaultValue = 10f, Step = 1f, Tooltip = "How many game ticks you can drive without overheating.")]
		public int OverheatTime = 10;

		[Toggle("Overheat Level Notification", Tooltip = "If alternate timer enabled this gives you a overheat percent, otherwise it just tells you the heat level. After 3 the random chance of fire kicks in.")]
		public bool OverheatNotify = true;
	}
}