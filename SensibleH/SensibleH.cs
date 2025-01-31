using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KK_SensibleH.Caress;
using KKAPI.MainGame;
using UnityEngine;
using static KK_SensibleH.Caress.Kiss;
using KKAPI;
using KKAPI.Utilities;

namespace KK_SensibleH
{
    [BepInPlugin(GUID, Name, Version)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
#if KK
    [BepInProcess(KoikatuAPI.GameProcessNameSteam)]
#endif
    [BepInDependency(KoikatuAPI.GUID)]
    [BepInDependency(KK_VR.VRPlugin.GUID)]
    [BepInDependency(KK_BetterSquirt.BetterSquirt.GUID)] // F it, normal version stays in hard dependencies, no clue how to pass reflected enum type to the delegate.

    public class SensibleH : BaseUnityPlugin
    {
        public const string GUID = "kk.sensible.h";
        public const string Name = "KK_SensibleH";
        public const string Version = "1.2.5";
        public new static PluginInfo Info { get; private set; }
        public new static ManualLogSource Logger;
        public static ConfigEntry<PluginState> ConfigEnabled { get; set; }
        public static ConfigEntry<AutoModeKind> ConfigAutoMode { get; set; }
        public static ConfigEntry<AutoPoseType> ConfigAutoPickPose { get; set; }
        public static ConfigEntry<float> ConfigAutoRestart { get; set; }
        public static ConfigEntry<EdgeType> ConfigEdge { get; set; }
        public static ConfigEntry<bool> EyeNeckControl { get; set; }
#if DEBUG
        public static ConfigEntry<bool> HoldPubicHair { get; set; }
        public static ConfigEntry<KeyboardShortcut> Cfg_TestKey { get; set; }
#endif
        public static ConfigEntry<bool> DisablePeskySounds { get; set; }
#if KKS
        public static ConfigEntry<bool> ProlongObi { get; set; }
#endif
        public static ConfigEntry<float> ActionFrequency { get; set; }
        public static ConfigEntry<float> EdgeFrequency { get; set; }
        public static ConfigEntry<FrenchType> FrenchKiss { get; set; }

        #region Tweaks

        public static ConfigEntry<bool> AutoADV { get; set; }
        public static ConfigEntry<float> NeckLimit { get; set; }
        public static ConfigEntry<int> KissEyesLimit { get; set; }
        public static ConfigEntry<bool> AddReverb { get; set; }
        public static ConfigEntry<float> AskCondom { get; set; }
        public static ConfigEntry<int> ConfigTalkTime { get; set; }
        public static ConfigEntry<bool> ConfigHelpBP { get; set; }

        #endregion

        #region Gauge

        public static ConfigEntry<int> GaugeSpeed { get; set; }
        public static ConfigEntry<float> ConfigBiasF { get; set; }
        public static ConfigEntry<float> ConfigBiasM { get; set; }
        public static ConfigEntry<int> ConfigMaleOrgCount { get; set; }
        public static ConfigEntry<int> ConfigFemaleOrgCount { get; set; }
        public static ConfigEntry<bool> ConfigFemaleOrgProgression { get; set; }

        #endregion


        public static bool MoveNeckGlobal;
        public static int[] EyeNeckPtn = { -1, -1, -1 };

        internal static List<HeadManipulator> headManipulators = [];
        internal static HandCtrl handCtrl;
        internal static HandCtrl handCtrl1;
        internal static HMotionEyeNeckFemale _eyeneckFemale;
        internal static HMotionEyeNeckFemale _eyeneckFemale1;
        internal static HFlag hFlag;
        internal static HFlag.EMode mode {  get; set; }
        internal static List<ChaControl> lstFemale;
        internal static ChaControl male;
        internal static HVoiceCtrl _hVoiceCtrl;
        internal static HSprite _sprite;
        internal static GameObject MalePoI;
        internal static GameObject[] FemalePoI;

        internal static float BiasF { get; set; }
        internal static float BiasM { get; set; }
        internal static float gaugeMultiplier { get; set; }
        internal static bool OLoop { get; set; }
        internal static bool MoMiActive { get; set; }
        internal static bool FirstTouch { get; set; }
        internal static int CurrentMain { get; set; }
        internal static int MaleOrgCount { get; set; }
        internal static bool SuppressVoice { get; set; }
        internal static bool OverrideSquirt { get; set; }
        //internal static bool BetterSquirtEnabled;
        internal static bool[] IsNeckSet = new bool[2];
        internal static Dictionary<string, int> LstHeroine;
        //internal static float[] NeckChangeRate = { 1f, 1f };
        internal static AnimatorStateInfo sLoopInfo { get; private set; }
        //internal delegate bool RunSquirt(bool softSE, FakeType trigger, bool sound, MonoBehaviour handCtrl, bool setTouchCooldown);
        //internal static RunSquirt RunSquirtsDelegate;
        public enum AutoModeKind
        {
            Disable,
            UserStart,
            UserStartFinish,
            Auto
    }
    public enum AutoPoseType
    {
        Disable,
        OnlyService,
        OnlyIntercourse,
        FemdomOnly,
            AllPositions
        }
        public enum EdgeType
        {
            Disable,
            Outside,
            Inside,
            Both
        }
        public enum PluginState
        {
            Disable,
            VrOnly,
            Enable,
        }


        public void Awake()
        {
            Logger = base.Logger;


            ConfigEnabled = Config.Bind(
                section: "",
                key: "Enable",
                defaultValue: PluginState.VrOnly,
                new ConfigDescription(
                    "Changes take place immediately")
                );


            ConfigAutoMode = Config.Bind(
                section: "AutoMode",
                key: "AutoState",
                defaultValue: AutoModeKind.UserStartFinish,
                new ConfigDescription(
                "Automate Service/Intercourse\n" +
                "Prompt Start - Disabled by default at the beginning, to start click on any interactable element of the lower half of the screen(except leaving H) or touch/kiss\n" +
                "Prompt Start Finish - Also gets disabled after climax, to continue same as above\n" +
                "Auto - Automatic managment of action, its restart and position/mode change",

                //"Warning. All proactive functions have mild disrespect for persistent user input, bad things (that will be solved only by the reboot of the game) may happen " +
                //"for those who seek two pilots in one seat." 
                null,
                new ConfigurationManagerAttributes { Order = 10 }
                ));


            ConfigAutoPickPose = Config.Bind(
                section: "AutoMode",
                key: "AutoPosition",
                defaultValue: AutoPoseType.AllPositions,
                new ConfigDescription(
                "Allow to change position after climax",
                //"FemdomOnly - Choses only position where the girl is dominant. By default it's only one (game's default) cowgirl position. With modified AnimationLoader manifest comes much more.\n" +
                //"AllPositions - random non caress animation.\n" +
                //"If both AutoPositionChange and AutoRestart are enabled, position change takes a bit of precedence.",
                null,
                new ConfigurationManagerAttributes { Order = 9 }
                ));


            ConfigEdge = Config.Bind(
                section: "AutoMode",
                key: "AutoEdge",
                defaultValue: EdgeType.Disable,
                new ConfigDescription(
                "Pull out/stop for a moment for whatever reason",
                null,
                new ConfigurationManagerAttributes { Order = 6 }
                ));


            ActionFrequency = Config.Bind(
                section: "AutoMode",
                key: "AutoFrequency",
                defaultValue: 1f,
                new ConfigDescription("Frequency of actions performed by AutoMode\nSmaller -> More frequent",
                new AcceptableValueRange<float>(0.1f, 2f),
                new ConfigurationManagerAttributes { Order = 8 }
                ));


            ConfigAutoRestart = Config.Bind(
                section: "AutoMode",
                key: "RestartChance",
                defaultValue: 0.3f,
                new ConfigDescription("Chance to restart action after climax before opting for position change. Will happen regardless if AutoMode enabled but AutoPosition disabled",
                new AcceptableValueRange<float>(0f, 1f),
                new ConfigurationManagerAttributes { Order = -20 }
                ));


            EdgeFrequency = Config.Bind(
                section: "AutoMode",
                key: "AutoEdgeFrequency",
                defaultValue: 1f,
                new ConfigDescription("AutoEdge's frequency of activation\nSmaller -> More frequent",
                new AcceptableValueRange<float>(0.1f, 2f),
                new ConfigurationManagerAttributes { Order = 5 }
                ));


            EyeNeckControl = Config.Bind(
                section: "Tweaks",
                key: "EyeNeck control",
                defaultValue: true,
                "Allow plugin to introduce alternative control of eyes and neck"
                );

            AutoADV = Config.Bind(
                section: "Tweaks",
                key: "Auto ADV",
                defaultValue: true,
                new ConfigDescription(
                    "Enables auto mode in any text scenario by default if there is no heroine present",
                    null,
                    new ConfigurationManagerAttributes { Order = -9 }
                ));


            FrenchKiss = Config.Bind(
                section: "Kiss",
                key: "Stick out tongue",
                defaultValue: Kiss.FrenchType.Auto,
                new ConfigDescription(
                "Stick out tongue during kiss.",
                null,
                new ConfigurationManagerAttributes { Order = 10 }
                ));


            KissEyesLimit = Config.Bind(
                section: "Kiss",
                key: "Close eyes",
                defaultValue: 50,
                new ConfigDescription("The ceiling of the eyes' openness during kiss.\n" +
                "0 - close, 100 - fully open",
                new AcceptableValueRange<int>(0, 100),
                new ConfigurationManagerAttributes { Order = 9, ShowRangeAsPercent = false }
                ));


            #region Tweaks


#if KKS
            ProlongObi = Config.Bind(
                section: "Tweaks",
                key: "Cum stays longer",
                defaultValue: true,
                "Don't clean cum until pose change"
                );
#endif


#if DEBUG
            HoldPubicHair = Config.Bind(
                section: "Tweaks",
                key: "HoldPubicHair",
                defaultValue: true,
                "Hold the scale of pubic hair accessory attached to the crouch"
                );
#endif


            AddReverb = Config.Bind(
                section: "Tweaks",
                key: "Reverb",
                defaultValue: true,
                new ConfigDescription("Add reverb SFX to some maps",
                null,
                new ConfigurationManagerAttributes { Order = -10 }
                ));


            NeckLimit = Config.Bind(
                section: "Tweaks",
                key: "Neck limit",
                defaultValue: 1f,
                new ConfigDescription("Adjust the limits of neck movements, 1.0 being the default value.\n" +
                "Changes take place on scene reload or new position.",
                new AcceptableValueRange<float>(0.5f, 1.5f))
                );


            AskCondom = Config.Bind(
                section: "Tweaks",
                key: "Ask condom",
                defaultValue: 0f,
                new ConfigDescription("Chance to ask for condom regardless of things",
                new AcceptableValueRange<float>(0, 1f),
                new ConfigurationManagerAttributes { Order = -15, ShowRangeAsPercent = false }
                ));


            DisablePeskySounds = Config.Bind(
                section: "Tweaks",
                key: "Button click SFX",
                defaultValue: true,
                new ConfigDescription(
                "Disable them",
                null,
                new ConfigurationManagerAttributes { Order = -10 }
                ));


            ConfigTalkTime = Config.Bind(
                section: "Tweaks",
                key: "Extra talk",
                defaultValue: 1,
                new ConfigDescription("Extra attempts in Talk Scene",
                new AcceptableValueRange<int>(1, 20),
                new ConfigurationManagerAttributes { Order = -20, ShowRangeAsPercent = false }
                ));


            ConfigHelpBP = Config.Bind(
                section: "Tweaks",
                key: "Help BP",
                defaultValue: true,
                new ConfigDescription(
                "Add better penetration colliders to caress aitems",
                null,
                new ConfigurationManagerAttributes { Order = -25 }
                ));


            #endregion

            #region Gauge

            GaugeSpeed = Config.Bind(
                section: "Gauge",
                key: "Excitement slowdown",
                defaultValue: 4,
                new ConfigDescription(
                    "Slow down excitement gauge by this amount of times",
                    new AcceptableValueRange<int>(1, 10),
                new ConfigurationManagerAttributes { Order = 20 })
                );


            ConfigBiasF = Config.Bind(
                section: "Gauge",
                key: "Bias female",
                defaultValue: 1f,
                new ConfigDescription("Influence female bias manually\nDriven by H Experience, main game stats if present and orgasm count",
                new AcceptableValueRange<float>(0.1f, 3f),
                new ConfigurationManagerAttributes { Order = 19 }
                )); 
            
            
            ConfigBiasM = Config.Bind(
                section: "Gauge",
                key: "Bias male",
                defaultValue: 1f,
                new ConfigDescription("Influence male bias manually\nDriven by partner's H Experience and orgasm count",
                new AcceptableValueRange<float>(0.1f, 3f),
                new ConfigurationManagerAttributes { Order = 18 }
                ));


            ConfigMaleOrgCount = Config.Bind(
                section: "Gauge",
                key: "Male ceiling",
                defaultValue: 5,
                new ConfigDescription(
                   "Number of orgasms per day that male is capable of\nSet 0 to disable",
                    new AcceptableValueRange<int>(0, 10),
                new ConfigurationManagerAttributes { Order = 16 })
                );


            ConfigFemaleOrgCount = Config.Bind(
                section: "Gauge",
                key: "Female ceiling",
                defaultValue: 5,
                new ConfigDescription(
                   "Number of orgasms per day that female is capable of\nCan be overridden by high H Experience in FreeH and H Experience + heroine stats in main game\nSet 0 to disable",
                    new AcceptableValueRange<int>(0, 10),
                new ConfigurationManagerAttributes { Order = 17 })
                );


            ConfigFemaleOrgProgression = Config.Bind(
                section: "Gauge",
                key: "Geometric progression",
                defaultValue: true,
                new ConfigDescription(
                   "Use geometric progression instead of arithmetic to increase acceleration of female gauge when applicable",
                    null,
                new ConfigurationManagerAttributes { Order = 15 }
                ));


            #endregion


#if DEBUG
            Cfg_TestKey = Config.Bind(
                section: "SensibleH",
                key: "TestKey1",
                defaultValue: KeyboardShortcut.Empty,
                "Key to manually trigger test"
                );
#endif


            sLoopInfo = new AnimatorStateInfo();
            object dummyInfo = sLoopInfo;
            Traverse.Create(dummyInfo).Field("m_Name").SetValue(-1715982390);
            Traverse.Create(dummyInfo).Field("m_SpeedMultiplier").SetValue(3f);
            Traverse.Create(dummyInfo).Field("m_Speed").SetValue(1f);
            Traverse.Create(dummyInfo).Field("m_Loop").SetValue(1);
            Traverse.Create(dummyInfo).Field("m_NormalizedTime").SetValue(59.73729f);
            Traverse.Create(dummyInfo).Field("m_Length").SetValue(0.4444448f);
            sLoopInfo = (AnimatorStateInfo)dummyInfo;

            GameAPI.RegisterExtraBehaviour<SensibleHController>(GUID);
        }
    }
}


