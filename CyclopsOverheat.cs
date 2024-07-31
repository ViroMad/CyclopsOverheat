//#define BepInEX_Config // Switch to BepInEX ONLY, No Nautilus

using BepInEx;

#if BepInEX_Config
	using BepInEx.Configuration;
#else
	using Nautilus;
	using Nautilus.Utility;
	using Nautilus.Handlers;
#endif
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using UnityEngine.PlayerLoop;
using TMPro;
using System.Globalization;
using System;
using static VFXParticlesPool;
//using BepInEx.Logging;

namespace SubOverheat
{
	[BepInPlugin(ModGUID, ModName, ModVersion)]
#if !BepInEX_Config
    [BepInDependency(Nautilus.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
#endif

    public class CyclopsOverheat : BaseUnityPlugin
    {
        #region
		const string Author = "ViroMan";
		const string ModName = "CyclopsOverheat";
		const string ModVersion = "2.0.0";
		const string ModGUID = Author + ".SN1." + ModName;
        #endregion

		public static BepInEx.Logging.ManualLogSource myLogger = new BepInEx.Logging.ManualLogSource(ModName);
        private static float DeltaTime;
        public static int CurrentOverheat;
        public static SubFire SubEntity;
#if BepInEX_Config
		public static ConfigSettings BepConfigSettings;		
#else
        internal static ConfigSettings ConfigSettings { get; } = OptionsPanelHandler.RegisterModOptions<ConfigSettings>();
#endif

        private void Awake()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();

			BepInEx.Logging.Logger.Sources.Add(myLogger);

#if BepInEX_Config
			BepConfigSettings = new ConfigSettings(this.Config);
#else
			SaveUtils.RegisterOnSaveEvent(new System.Action(ConfigSettings.Save)); 
#endif

            Harmony Harmonything = new Harmony(ModGUID);

            myLogger.LogInfo("Patching SubFire(EngineOverheatSimulation, Start)");
            Harmonything.PatchAll(typeof(SubFire_Patch));

            myLogger.LogInfo("Patching ErrorMessage()");
            Harmonything.PatchAll(typeof(ErrorMessage_Patch));

            myLogger.LogInfo("Patching Complete.");
		}
		private void FixedUpdate() //updates every 0.02
		{
			DeltaTime += Time.deltaTime;
            bool OverheatOveride;
            int OverheatTime;
            bool OverheatNotify;

#if BepInEX_Config
            OverheatOveride = ConfigSettings.OverheatOveride.Value;
            OverheatTime = ConfigSettings.OverheatTime.Value;
            OverheatNotify = ConfigSettings.OverheatNotify.Value;
#else
			OverheatOveride = CyclopsOverheat.ConfigSettings.OverheatOveride;
			OverheatTime = CyclopsOverheat.ConfigSettings.OverheatTime;
			OverheatNotify = CyclopsOverheat.ConfigSettings.OverheatNotify;
#endif

			int TimeOverheat;

            if (DeltaTime > 1.00)
			{
				DeltaTime = 0;
				if(SubEntity!=null)
				{
                    if (!SubEntity.LOD.IsFull())
                        return;

					if (!OverheatOveride)
						TimeOverheat = 10;
					else
						TimeOverheat = OverheatTime;

                    ErrorMessage.AddMessage("§1.00§Overheat Value:" + CyclopsOverheat.CurrentOverheat + " Overheat Timer: " + OverheatTime);
                    if (SubEntity.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Flank && SubEntity.subControl.appliedThrottle && SubEntity.cyclopsMotorMode.engineOn)
                    {
                        if (!OverheatOveride)
                            // since the old code ran every 5 (now its 2) ticks set this to +2 per so that it will average out to 10 in 5 seconds
                            //CyclopsOverheat.CurrentOverheat = Mathf.Min(CyclopsOverheat.CurrentOverheat + 2, 10);
                            CyclopsOverheat.CurrentOverheat = Mathf.Min(CyclopsOverheat.CurrentOverheat + 2, 10);
                        else
                            //CyclopsOverheat.CurrentOverheat = Mathf.Min(CyclopsOverheat.CurrentOverheat + 1, OverheatTime);
                            CyclopsOverheat.CurrentOverheat = Mathf.Min(CyclopsOverheat.CurrentOverheat + 1, OverheatTime);
                    }
					else
					{
						//not driving forward
						if(SubEntity.cyclopsMotorMode.engineOn)
						{
							switch(SubEntity.cyclopsMotorMode.cyclopsMotorMode)
							{
								//case CyclopsMotorMode.CyclopsMotorModes.Flank:
                                //    CyclopsOverheat.CurrentOverheat = Mathf.Clamp(CyclopsOverheat.CurrentOverheat - 1, 0, TimeOverheat);
                                //    break;
								case CyclopsMotorMode.CyclopsMotorModes.Standard:
                                    CyclopsOverheat.CurrentOverheat = Mathf.Clamp(CyclopsOverheat.CurrentOverheat - 1, 0, TimeOverheat);
                                    break;
                                case CyclopsMotorMode.CyclopsMotorModes.Slow:
                                    CyclopsOverheat.CurrentOverheat = Mathf.Clamp(CyclopsOverheat.CurrentOverheat - 2, 0, TimeOverheat);
                                    break;
                            }
                        }
						else
                            CyclopsOverheat.CurrentOverheat = Mathf.Clamp(CyclopsOverheat.CurrentOverheat - 4, 0, TimeOverheat);
                    }
                }
            }
        }
        /*private void Update() // updates randomly around 0.016
        {
            ErrorMessage.AddMessage("Update Time is: " + Time.deltaTime);
        }*/
    }

    [HarmonyPatch]
    public class SubFire_Patch
    {
        [HarmonyPatch(typeof(SubFire), "EngineOverheatSimulation")]
        [HarmonyPrefix]
        public static bool Patch_EngineOverheatSimulation(SubFire __instance)
        {
			bool OverheatOveride;
			int OverheatTime;
			bool OverheatNotify;
			string TimerString = "§1.50§";
            CyclopsOverheat.SubEntity = __instance;

#if BepInEX_Config
            OverheatOveride = ConfigSettings.OverheatOveride.Value;
			OverheatTime = ConfigSettings.OverheatTime.Value;
			OverheatNotify = ConfigSettings.OverheatNotify.Value;
#else
			OverheatOveride = CyclopsOverheat.ConfigSettings.OverheatOveride;
			OverheatTime = CyclopsOverheat.ConfigSettings.OverheatTime;
			OverheatNotify = CyclopsOverheat.ConfigSettings.OverheatNotify;
#endif

			if (!__instance.LOD.IsFull())
				return false;
            if (__instance.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Flank && __instance.subControl.appliedThrottle && __instance.cyclopsMotorMode.engineOn)
			{
                if (!OverheatOveride)
                {
					ErrorMessage.AddMessage("Doing Old Math");
                    //CyclopsOverheat.CurrentOverheat = Mathf.Min(CyclopsOverheat.CurrentOverheat + 1, 10); //<-- Check Main Timer
                    int num = 0;
					if (CyclopsOverheat.CurrentOverheat > 5)
					{						
						num = UnityEngine.Random.Range(1, 4);
						__instance.subRoot.voiceNotificationManager.PlayVoiceNotification(__instance.subRoot.engineOverheatCriticalNotification, true, false);
						if (OverheatNotify && num != 1)
							ErrorMessage.AddMessage(TimerString + "Engine Critical Overheat Level: " + CyclopsOverheat.CurrentOverheat + "  Over Heat Chance: 25%");
					}
					else if (CyclopsOverheat.CurrentOverheat > 3)
					{
						num = UnityEngine.Random.Range(1, 6);
						__instance.subRoot.voiceNotificationManager.PlayVoiceNotification(__instance.subRoot.engineOverheatNotification, true, false);
						if (OverheatNotify && num != 1)
							ErrorMessage.AddMessage(TimerString + "Engine Overheat Level: " + CyclopsOverheat.CurrentOverheat + "  Over Heat Chance: 17%");
					}
					else if(OverheatNotify && CyclopsOverheat.CurrentOverheat < 4)
						ErrorMessage.AddMessage(TimerString + "Engine Heat Level: " + CyclopsOverheat.CurrentOverheat);
					if (num == 1)
					{
						__instance.CreateFire(__instance.roomFires[CyclopsRooms.EngineRoom]);
						return false;
					}
				}
				else
                {
					//Alternate Timer
					//CyclopsOverheat.CurrentOverheat = Mathf.Min(CyclopsOverheat.CurrentOverheat + 1, OverheatTime);  //<-- Check Main Timer
					if (OverheatNotify)
						ErrorMessage.AddMessage(TimerString + "Engine Overheat: " + ((float)CyclopsOverheat.CurrentOverheat / OverheatTime) * 100 + "%");
					if (CyclopsOverheat.CurrentOverheat > (int)(OverheatTime * 0.75f))
					{						
						__instance.subRoot.voiceNotificationManager.PlayVoiceNotification(__instance.subRoot.engineOverheatCriticalNotification, true, false);
					}
					else if (CyclopsOverheat.CurrentOverheat > (int)(OverheatTime * 0.50f))
						__instance.subRoot.voiceNotificationManager.PlayVoiceNotification(__instance.subRoot.engineOverheatNotification, true, false);
					if (CyclopsOverheat.CurrentOverheat >= OverheatTime)
						__instance.CreateFire(__instance.roomFires[CyclopsRooms.EngineRoom]);
				}
			}
			else
			{
				/*if (__instance.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Flank)
					CyclopsOverheat.CurrentOverheat = Mathf.Max(1, CyclopsOverheat.CurrentOverheat - 1);
				else
					CyclopsOverheat.CurrentOverheat = Mathf.Max(0, CyclopsOverheat.CurrentOverheat - 1);
				*/

				if( ( (__instance.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Flank && CyclopsOverheat.CurrentOverheat > 1) ||
					(__instance.cyclopsMotorMode.cyclopsMotorMode != CyclopsMotorMode.CyclopsMotorModes.Flank && CyclopsOverheat.CurrentOverheat > 0)) &&
					OverheatNotify)
                {
					if (OverheatOveride)
						ErrorMessage.AddMessage(TimerString + "Engine Overheat: " + ((float)CyclopsOverheat.CurrentOverheat / OverheatTime) * 100 + "%");
					else
						ErrorMessage.AddMessage(TimerString + "Engine Heat Level: " + CyclopsOverheat.CurrentOverheat);
				}
				
			}
			return false;
		}

        [HarmonyPatch(typeof(SubFire), "Start")]
        [HarmonyPrefix]
        public static bool Patch_Start(SubFire __instance)
		{
            __instance.roomFires.Clear();
            foreach (object obj in __instance.fireSpawnsRoot)
            {
                Transform transform = (Transform)obj;
                RoomLinks component = transform.GetComponent<RoomLinks>();
                if (component)
                {
                    SubFire.RoomFire roomFire = new SubFire.RoomFire(transform);
                    __instance.roomFires.Add(component.room, roomFire);
                }
            }
            __instance.smokeController = MainCamera.camera.GetComponent<CyclopsSmokeScreenFXController>();
            __instance.smokeController.intensity = __instance.curSmokeVal;
            Color color = new Color(0.2f, 0.2f, 0.2f, __instance.smokeImpostorRemap.Evaluate(__instance.curSmokeVal));
            __instance.smokeImpostorRenderer.material.SetColor(ShaderPropertyID._Color, color);
            if (__instance.fireCount > 0)
            {
                int num = Enum.GetNames(typeof(CyclopsRooms)).Length;
                for (int i = 0; i < __instance.fireCount; i++)
                {
                    CyclopsRooms cyclopsRooms = (CyclopsRooms)global::UnityEngine.Random.Range(0, num);
                    __instance.CreateFire(__instance.roomFires[cyclopsRooms]);
                }
            }
            __instance.InvokeRepeating("SmokeSimulation", 3f, 3f);
            __instance.InvokeRepeating("FireSimulation", 10f, 10f);
            __instance.InvokeRepeating("EngineOverheatSimulation", 5f, 1f); // 5f 5f
			return false;
        }
    }

    [HarmonyPatch]
    public class ErrorMessage_Patch
	{
        [HarmonyPatch(typeof(ErrorMessage), "_AddMessage")]
        [HarmonyPrefix]
        private static bool Patch__AddMessage(ErrorMessage __instance, string messageText)
		{
			float OutTime;
			string OutMessage;

			ParseMessage(messageText, out OutMessage, out OutTime);
            if(OutTime == 0)
				return true;

            if (string.IsNullOrEmpty(OutMessage))
            {
                return false;
            }
            ErrorMessage._Message message = __instance.GetExistingMessage(OutMessage);
            if (message == null)
            {
                Rect rect = __instance.messageCanvas.rect;
                TextMeshProUGUI entry = __instance.GetEntry();
                entry.gameObject.SetActive(true);
                RectTransform rectTransform = entry.rectTransform;
                entry.text = OutMessage;
                message = new ErrorMessage._Message();
                message.entry = entry;
                message.messageText = OutMessage;
                message.num = 1;
                message.timeEnd = PDA.time + OutTime + __instance.timeFlyIn;
                __instance.messages.Add(message);
                return false;
            }
            TMP_Text entry2 = message.entry;
            message.timeEnd = PDA.time + OutTime;
			entry2.text = OutMessage;
            return false;
		}

		private static void ParseMessage(in string Message, out string OutMessage, out float Time)
		{
			Time = 0;
            OutMessage = Message;
            char Test = Message[0];
			string TempStr;
            
            if (Test=='§')
			{
                TempStr = Message.Substring(1, 4);
                Time = float.Parse(TempStr, CultureInfo.InvariantCulture.NumberFormat);
                OutMessage = Message.Remove(0, 6);
            }
		}
    }
}