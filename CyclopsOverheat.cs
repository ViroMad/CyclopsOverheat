using BepInEx;
using SMLHelper.V2.Handlers;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
//using BepInEx.Logging;

namespace SubOverheat
{
    [BepInPlugin("com.ViroMan.SN1.CyclopsOverheat", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]	
	[BepInDependency("com.ahk1221.smlhelper")]
	public class CyclopsOverheat : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource myLogger = new BepInEx.Logging.ManualLogSource(PluginInfo.PLUGIN_NAME);
        internal static SMLConfig SMLConfig { get; } = OptionsPanelHandler.RegisterModOptions<SMLConfig>();
		//internal static MyOptions MyOptions { get; } = OptionsPanelHandler.RegisterModOptions<MyOptions>();
		private void Awake()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
			//string EAText = "ViroMan.SN1." + executingAssembly.GetName().Name;

			BepInEx.Logging.Logger.Sources.Add(myLogger);

			IngameMenuHandler.RegisterOnSaveEvent(new System.Action(CyclopsOverheat.SMLConfig.Save));			

			Logger.LogInfo("Patching SubFire.EngineOverheatSimulation()");
			new Harmony($"ViroMan.SN1." + executingAssembly.GetName().Name).PatchAll(executingAssembly);
			Logger.LogInfo("Patching Complete.");
		}
    }

    [HarmonyPatch]
    public class EngineOverheatSimulation_Patch
    {
        [HarmonyPatch(typeof(SubFire), "EngineOverheatSimulation")]
        [HarmonyPrefix]
        public static bool Patch_EngineOverheatSimulation(SubFire __instance)
        {			
			if (!__instance.LOD.IsFull())
				return false;
			if (__instance.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Flank && __instance.subControl.appliedThrottle && __instance.cyclopsMotorMode.engineOn)
			{				
				if (!CyclopsOverheat.SMLConfig.OverheatOveride)
                {					
					__instance.engineOverheatValue = Mathf.Min(__instance.engineOverheatValue + 1, 10);
					int num = 0;
					if (__instance.engineOverheatValue > 5)
					{						
						num = UnityEngine.Random.Range(1, 4);
						__instance.subRoot.voiceNotificationManager.PlayVoiceNotification(__instance.subRoot.engineOverheatCriticalNotification, true, false);
						if (CyclopsOverheat.SMLConfig.OverheatNotify && num != 1)
							ErrorMessage.AddMessage("Engine Critical Overheat Level: " + __instance.engineOverheatValue + "  Over Heat Chance: 25%");
					}
					else if (__instance.engineOverheatValue > 3)
					{
						num = UnityEngine.Random.Range(1, 6);
						__instance.subRoot.voiceNotificationManager.PlayVoiceNotification(__instance.subRoot.engineOverheatNotification, true, false);
						if (CyclopsOverheat.SMLConfig.OverheatNotify && num != 1)
							ErrorMessage.AddMessage("Engine Overheat Level: " + __instance.engineOverheatValue + "  Over Heat Chance: 17%");
					}
					else if(CyclopsOverheat.SMLConfig.OverheatNotify && __instance.engineOverheatValue < 4)
						ErrorMessage.AddMessage("Engine Heat Level: " + __instance.engineOverheatValue);
					if (num == 1)
					{
						__instance.CreateFire(__instance.roomFires[CyclopsRooms.EngineRoom]);
						return false;
					}
				}
				else
                {
					//Alternate Timer
					__instance.engineOverheatValue = Mathf.Min(__instance.engineOverheatValue + 1, CyclopsOverheat.SMLConfig.OverheatTime);
					if (CyclopsOverheat.SMLConfig.OverheatNotify)
						ErrorMessage.AddMessage("Engine Overheat: " + ((float)__instance.engineOverheatValue / CyclopsOverheat.SMLConfig.OverheatTime) * 100 + "%");
					if (__instance.engineOverheatValue > (int)(CyclopsOverheat.SMLConfig.OverheatTime * 0.75f))
					{						
						__instance.subRoot.voiceNotificationManager.PlayVoiceNotification(__instance.subRoot.engineOverheatCriticalNotification, true, false);
					}
					else if (__instance.engineOverheatValue > (int)(CyclopsOverheat.SMLConfig.OverheatTime * 0.50f))
						__instance.subRoot.voiceNotificationManager.PlayVoiceNotification(__instance.subRoot.engineOverheatNotification, true, false);
					if (__instance.engineOverheatValue >= CyclopsOverheat.SMLConfig.OverheatTime)
						__instance.CreateFire(__instance.roomFires[CyclopsRooms.EngineRoom]);
				}
			}
			else
			{
				if (__instance.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Flank)
					__instance.engineOverheatValue = Mathf.Max(1, __instance.engineOverheatValue - 1);
				else
					__instance.engineOverheatValue = Mathf.Max(0, __instance.engineOverheatValue - 1);
				
				if( ( (__instance.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Flank && __instance.engineOverheatValue > 1) ||
					(__instance.cyclopsMotorMode.cyclopsMotorMode != CyclopsMotorMode.CyclopsMotorModes.Flank && __instance.engineOverheatValue > 0)) &&
					CyclopsOverheat.SMLConfig.OverheatNotify)
                {
					if (CyclopsOverheat.SMLConfig.OverheatOveride)
						ErrorMessage.AddMessage("Engine Overheat: " + ((float)__instance.engineOverheatValue / CyclopsOverheat.SMLConfig.OverheatTime) * 100 + "%");
					else
						ErrorMessage.AddMessage("Engine Heat Level: " + __instance.engineOverheatValue);
				}
				
			}
			return false;
		}
    }
}