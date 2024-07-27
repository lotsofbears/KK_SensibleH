using System.Collections.Generic;
using System.Linq;
using ADV.Commands.Base;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KK_SensibleH.Caress;
using KKAPI.MainGame;
using UnityEngine;
using static KK_SensibleH.Caress.Kiss;

namespace KK_SensibleH
{
    [BepInPlugin(GUID, "KK_SensibleH", Version)]
    [BepInProcess("Koikatu")]
    [BepInDependency("marco.kkapi")]
    [BepInDependency("mosirnik.kk-main-game-vr")]
    [BepInDependency("MK.KK_BetterSquirt")] // TODO use unmodified version, without "forced" start.
    [BepInDependency("KK_Fix_ResourceUnloadOptimizations")]
    public class SensibleH : BaseUnityPlugin
    {
        public const string GUID = "kk.sensible.h";
        public const string Version = "0.1.1";
        public new static PluginInfo Info { get; private set; }
        public new static ManualLogSource Logger;
        public static ConfigEntry<AutoModeKind> AutoMode { get; set; }
        public static ConfigEntry<AutoPosMode> AutoPickPosition { get; set; }
        public static ConfigEntry<bool> AutoRestartAction { get; set; }
        public static ConfigEntry<bool> Edge { get; set; }
        public static ConfigEntry<bool> MomiMomi { get; set; }
        public static ConfigEntry<float> ActionFrequency { get; set; }
        public static ConfigEntry<float> EdgeFrequency { get; set; }
        public static ConfigEntry<float> NeckLimit { get; set; }
        public static ConfigEntry<Kiss.FrenchType> FrenchKiss { get; set; }
        public static ConfigEntry<int> KissEyesLimit { get; set; }
        public static ConfigEntry<KeyboardShortcut> Cfg_TestKey { get; set; }
        public static ConfigEntry<KeyboardShortcut> Cfg_TestKey2 { get; set; }
        public static ConfigEntry<KeyboardShortcut> Cfg_TestKey3 { get; set; }
        internal static List<GirlController> _girlController;
        internal static HandCtrl _handCtrl;
        internal static HandCtrl _handCtrl1;
        internal static HMotionEyeNeckFemale _eyeneckFemale;
        internal static HMotionEyeNeckFemale _eyeneckFemale1;
        internal static HFlag _hFlag;
        internal static List<ChaControl> _chaControl;
        internal static ChaControl _chaControlM;
        internal static HVoiceCtrl _hVoiceCtrl;
        internal static HSprite _sprite;
        public static bool MoveNeckGlobal;
        public static int[] EyeNeckPtn = { -1, -1, -1 };
        public static float BiasF;
        public static float BiasM;
        public static GameObject MalePoI;
        public static GameObject[] FemalePoI;
        public static bool OLoop;
        public static AnimatorStateInfo sLoopInfo;
        public static bool MoMiActive;
        public static bool FirstTouch;
        public static int CurrentMain;
        public static Dictionary<string, int> LstHeroine;
        public static int MaleOrgCount;
        public enum AutoModeKind
        {
            Disabled,
            BeginWithPrompt,
            BeginAndProceedWithPrompt,
            Automatic
        }
        public enum AutoPosMode
        {
            Disabled,
            FemdomOnly,
            AllPositions
        }
        private Harmony _triggers;

        public void Awake()
        {
            Logger = base.Logger;

            AutoMode = Config.Bind(
                section: "AutoMode",
                key: "AutoMode",
                defaultValue: AutoModeKind.Disabled,
                "Minimize the need for inputs from the player during H.\n" +
                "Disabled - Waits for setting to change any moment.\n" +
                "BeginWithPrompt - For action to begin necessary:\n" +
                "Click on any interactable element of the lower half of the screen(except leaving H) or Touch or Kiss\n" +
                "BeginAndProceedWithPrompt - Same as above but now after climax too.\n" +
                "Automatic - Will start/finish action, change position (If setting is enabled) and restart action (If setting is enabled will do it considerably faster) without any user input.\n" +
                "Warning. All proactive functions have mild disrespect for persistent user input, bad things (that will be solved only by the reboot of the game) may happen " +
                "for those who seeks two pilots in one seat." 
                );
            AutoPickPosition = Config.Bind(
                section: "AutoMode",
                key: "AutoPositionChange",
                defaultValue: AutoPosMode.AllPositions,
                "Allows auto change of positions after climax.\n" +
                "Disabled - Waits for setting to change any moment.\n" +
                "FemdomOnly - Choses only position where the girl is dominant. By default it's only one (game's default) cowgirl position. With modified AnimationLoader manifest comes much more.\n" +
                "AllPositions - Choses random non caress animation found.\n" +
                "If both AutoPositionChange and AutoRestart are enabled, position change takes a bit of precedence."
                );
            AutoRestartAction = Config.Bind(
                section: "AutoMode",
                key: "AutoRestart",
                defaultValue: true,
                "With  AutoPositionChange enabled, attempts to restart action after climax (and all the voices). If unsuccessful, changes position.\n" +
                "With AutoPositionChange disabled,  restarts action after climax (and all the voices). \n" +
                "Even if disabled, as long as AutoMode is in functional state (enabled and if necessary with inputs from user) sooner or later " +
                "restart will happen."

                );
            Edge = Config.Bind(
                section: "AutoMode",
                key: "Edge",
                defaultValue: true,
                "Allows one of the partners to pull out/stop for a moment " +
                "for whatever reason it may be. Available in Service/Intercourse."
                );
            ActionFrequency = Config.Bind(
                section: "AutoMode",
                key: "ActionFrequency",
                defaultValue: 1f,
                new ConfigDescription("Frequency of manipulations in AutoMode.\n" +
                "Lesser value -> smaller pause between actions.",
                new AcceptableValueRange<float>(0.1f, 2f))
                ); 
            EdgeFrequency = Config.Bind(
                section: "AutoMode",
                key: "EdgeFrequency",
                defaultValue: 1f,
                new ConfigDescription("Cooldown of edge mode.\n" +
                "Lesser value -> smaller pause between edges.",
                new AcceptableValueRange<float>(0.1f, 2f))
                );
            NeckLimit = Config.Bind(
                section: "Tweaks",
                key: "NeckLimit",
                defaultValue: 0.8f,
                new ConfigDescription("Adjust the limits of neck movements, 1.0 being the default value.\n" +
                "Changes take place on scene reload or new position.",
                new AcceptableValueRange<float>(0.5f, 1.5f))
                ); 
            FrenchKiss = Config.Bind(
                section: "Caress",
                key: "KissType",
                defaultValue: Kiss.FrenchType.Auto,
                "Set alternative type of the mouth during kiss.\n" +
                "Disabled - Waits for setting to change any moment.\n" +
                "Auto - Changes mouth with certain probability based on random/girl's H experience/girl's excitement.\n" +
                "Always - Self explanatory."
                );
            KissEyesLimit = Config.Bind(
                section: "Caress",
                key: "EyesOpenness",
            defaultValue: 50,
            new ConfigDescription("Maximum openness of eyes and eyelids during kissing.\n" +
            "Set to 0 to keep eyes closed during kiss",
            new AcceptableValueRange<int>(0, 100))
                );
            MomiMomi = Config.Bind(
                section: "Caress",
                key: "MomiMomi",
                defaultValue: true,
                "Attach items(hands/tongue/etc) to girl's points of interest then press and hold the mouse button for a second (or trigger if in MainGameVR)" +
                "and enjoy items moving by themselves (button may be released). A click anywhere to stop it. This setting is just description."
                );

            Cfg_TestKey = Config.Bind(
                section: "SensibleH",
                key: "TestKey1",
                defaultValue: KeyboardShortcut.Empty,
                "Key to manually trigger test"
                );
            Cfg_TestKey2 = Config.Bind(
                section: "SensibleH",
                key: "TestKey2",
                defaultValue: KeyboardShortcut.Empty,
                "Key to manually trigger test"
                );
            Cfg_TestKey3 = Config.Bind(
                section: "SensibleH",
                key: "TestKey3",
                defaultValue: KeyboardShortcut.Empty,
                "Key to manually trigger test"
                );



            // God knows where the original info is stored. Will make do for now.
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
            _triggers = Harmony.CreateAndPatchAll(typeof(Triggers));
        }

        public void OnDestroy()
        {
            _triggers?.UnpatchSelf();
        }

        private static int PrettyNumber(int number)
        {
            if (Mathf.Abs(number % 5) > 2)
                return number > 0 ? ((number / 5) + 1) * 5 : ((number / 5) - 1) * 5;
            else
                return (number / 5) * 5;
        }
        private static class Triggers
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddAibuOrg))]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuOrg))]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuAnalOrg))]
            public static void OrgasmFemalePostfix()
            {
                SensibleH.Logger.LogDebug("OrgasmFemalePostfix");
                LoopController.Instance.DoOrgasmF();
            }
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddHoushiInside))]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddHoushiOutside))]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuInside))]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuOutside))]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuCondomInside))]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuAnalInside))]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuAnalOutside))]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuAnalCondomInside))]
            public static void OrgasmMalePostfix()
            {
                SensibleH.Logger.LogDebug("OrgasmMalePostfix");
                LoopController.Instance.DoOrgasmM();
            }
            /// <summary>
            /// Wa waltz in on masturbating/lesbianing wonder, and the scene is already HOT. Outside of FreeH that is.
            /// And we change limits for neck movement.
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.ChangeAnimator))]
            public static void ChangeAnimatorPostfix(HSceneProc.AnimationListInfo _nextAinmInfo, HSceneProc __instance)
            {
                SensibleHController.Instance.OnPositionChange(_nextAinmInfo);
                if (!__instance.flags.isFreeH && (__instance.flags.mode == HFlag.EMode.masturbation || __instance.flags.mode == HFlag.EMode.lesbian))
                    __instance.flags.gaugeFemale = UnityEngine.Random.Range(0, 100);

                if (__instance.flags.mode == HFlag.EMode.masturbation)
                {
                    __instance.flags.timeMasturbation.timeMin = UnityEngine.Random.Range(20, 30);
                    __instance.flags.timeMasturbation.timeMax = __instance.flags.timeMasturbation.timeMin + 20f;
                }
                if (__instance.flags.mode == HFlag.EMode.lesbian || __instance.flags.mode == HFlag.EMode.houshi3P || __instance.flags.mode == HFlag.EMode.sonyu3P)
                {
                    for (int i = 0; i < __instance.eyeneckFemale1.dicEyeNeck.Count; i++)
                    {
                        var eyeNeck = __instance.eyeneckFemale1.dicEyeNeck.ElementAt(i).Value;
                        eyeNeck.rangeNeck.up = PrettyNumber((int)(eyeNeck.rangeNeck.up * NeckLimit.Value));
                        eyeNeck.rangeNeck.down -= 10;
                        eyeNeck.rangeNeck.left = PrettyNumber((int)(eyeNeck.rangeNeck.left * NeckLimit.Value));
                        eyeNeck.rangeNeck.right = PrettyNumber((int)(eyeNeck.rangeNeck.right * NeckLimit.Value));
                        eyeNeck.rangeFace.up = PrettyNumber((int)(eyeNeck.rangeFace.up * NeckLimit.Value));
                        eyeNeck.rangeFace.down -= 10;
                        eyeNeck.rangeFace.left = PrettyNumber((int)(eyeNeck.rangeFace.left * NeckLimit.Value));
                        eyeNeck.rangeFace.right = PrettyNumber((int)(eyeNeck.rangeFace.right * NeckLimit.Value));
                    }
                }
                for (int i = 0; i < __instance.eyeneckFemale.dicEyeNeck.Count; i++)
                {
                    var eyeNeck = __instance.eyeneckFemale.dicEyeNeck.ElementAt(i).Value;
                    eyeNeck.rangeNeck.up = PrettyNumber((int)(eyeNeck.rangeNeck.up * NeckLimit.Value));
                    eyeNeck.rangeNeck.down -= 10;
                    eyeNeck.rangeNeck.left = PrettyNumber((int)(eyeNeck.rangeNeck.left * NeckLimit.Value));
                    eyeNeck.rangeNeck.right = PrettyNumber((int)(eyeNeck.rangeNeck.right * NeckLimit.Value));
                    eyeNeck.rangeFace.up = PrettyNumber((int)(eyeNeck.rangeFace.up * NeckLimit.Value));
                    eyeNeck.rangeFace.down -= 10;
                    eyeNeck.rangeFace.left = PrettyNumber((int)(eyeNeck.rangeFace.left * NeckLimit.Value));
                    eyeNeck.rangeFace.right = PrettyNumber((int)(eyeNeck.rangeFace.right * NeckLimit.Value));
                }

                //if (__instance.flags.mode == HFlag.EMode.lesbian)
                //{
                //    __instance.flags.timeLesbian.timeMin = UnityEngine.Random.Range(20, 30);
                //    __instance.flags.timeLesbian.timeMax = __instance.flags.timeMasturbation.timeMin + 20f;
                //}

            }

            /// <summary>
            /// We check for non Orgasm/OrgasmAfter loops and run the timer that by default is being used only for the action restart after the finish.
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HMasturbation), nameof(HMasturbation.Proc))]
            public static void HMasturbationProc(HMasturbation __instance)
            {
                if (!__instance.flags.nowAnimStateName.StartsWith("O", System.StringComparison.Ordinal) && __instance.flags.timeMasturbation.IsIdleTime())
                {
                    if (__instance.flags.gaugeFemale < 40f)
                        __instance.flags.voice.playVoices[0] = 402;

                    else if (__instance.flags.gaugeFemale < 70f)
                        __instance.flags.voice.playVoices[0] = 403;

                    else// if (__instance.flags.gaugeFemale < 100f)
                        __instance.flags.voice.playVoices[0] = 404;
                }
                
            }

            /// <summary>
            /// We check for non Orgasm, OrgasmAfter loops and run the timer that by default is being used only for the action restart after the finish.
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HLesbian), nameof(HLesbian.Proc))]
            public static void HLesbianProc(HLesbian __instance)
            {
                if (!__instance.flags.nowAnimStateName.StartsWith("O", System.StringComparison.Ordinal) && __instance.flags.timeLesbian.IsIdleTime())
                {
                    __instance.speek = false;
                }
            }
            /// <summary>
            /// We catch the voiceProc to, perhaps, interrupt it and run it at the latter time.
            /// </summary>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HVoiceCtrl), nameof(HVoiceCtrl.VoiceProc))]
            public static void PrefixVoiceProc(HVoiceCtrl __instance, int _main)
            {
                if (__instance.flags.voice.playVoices[_main] != -1 && _hFlag != null)// && __instance.nowVoices[_main].state != HVoiceCtrl.VoiceKind.voice)
                {
                    if (_frenchKiss || _kissPhase == Phase.Disengaging)
                        __instance.flags.voice.playVoices[_main] = -1;
                    else
                        SensibleHController.Instance.DoVoiceProc(_main);
                    //_voiceController.NicknamePlay();
                }
            }
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.FemaleGaugeUp))]
            public static void PrefixFemaleGaugeUp(ref float _addPoint)
            {
                if (_addPoint < 0)// && biasF < 1f)
                    _addPoint = _addPoint * 0.25f;
                else
                    _addPoint = _addPoint * 0.25f * BiasF;
            }
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.MaleGaugeUp))]
            public static void PrefixMaleGaugeUp(ref float _addPoint)
            {
                if (_addPoint < 0)// && biasM < 1f)
                    _addPoint = _addPoint * 0.25f;
                else
                    _addPoint = _addPoint * 0.25f * BiasM;
            }
            /// <summary>
            /// We override basic neck with our pick, somewhere down the line we need to change the "_tag" too.
            /// </summary>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HMotionEyeNeck), nameof(HMotionEyeNeck.SetEyeNeckPtn))]
            public static void PrefixSetEyeNeckPtn(ref int _id, ref GameObject _objCamera, bool _isConfigEyeDisregard, bool _isConfigNeckDisregard, HMotionEyeNeckMale __instance)
            {
                if (MoveNeckGlobal && __instance.chara.sex == 1)
                {
                    for (var i = 0; i < _chaControl.Count; ++i)
                    {
                        if (__instance.chara == _chaControl[i])
                        {
                            _id = EyeNeckPtn[i];
                            if (FemalePoI[i] != null)
                                _objCamera = FemalePoI[i];
                        }
                    }
                }
                if (__instance.chara.sex == 0 && _chaControlM != null && _chaControlM.visibleAll)
                {
                    _id = EyeNeckPtn[2];
                    if (MalePoI != null)
                        _objCamera = MalePoI;
                }
                //__instance.chara.ChangeEyebrowOpenMax
                //__instance.chara.ChangeLookNeckTarget(0, __instance.objKokan.transform, 0.5f, 0f, 1f, 0.8f);

            }

            //[HarmonyPostfix]
            //[HarmonyPatch(typeof(FaceListCtrl), nameof(FaceListCtrl.SetFace))]
            //public static void AlterSetFace(int _idFace, ChaControl _chara, int _voiceKind, int _action)
            //{


            //}

            /// <summary>
            /// TODO do it with transpiler.
            /// We run voiceProcs for caress actions in non-Aibu modes.
            /// </summary>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.ClickAction))]
            public static void ClickActionPrefix(HandCtrl __instance)
            {
                //SensibleH.Logger.LogDebug("ClickAction");
                if ((__instance.flags.mode == HFlag.EMode.houshi || __instance.flags.mode == HFlag.EMode.sonyu) &&
                    (__instance.voicePlayClickCount + 1) % __instance.voicePlayClickLoop == 0 &&
                    __instance.voice.nowVoices[__instance.numFemale].state != HVoiceCtrl.VoiceKind.voice)
                {
                    __instance.voicePlayClickCount = 0;
                    __instance.voicePlayClickLoop = UnityEngine.Random.Range(10, 20);
                    int num = __instance.useItems[__instance.actionUseItem].kindTouch - HandCtrl.AibuColliderKind.mouth;
                    int[] array = new int[]
                    {
                        0,
                        1,
                        1,
                        2,
                        3,
                        4,
                        4
                    };
                    int[,] array2 = new int[,]
                        {
                            {
                                -1,
                                111,
                                113,
                                115,
                                117,
                                -1
                            },
                            {
                                -1,
                                123,
                                119,
                                121,
                                -1,
                                -1
                            },
                            {
                                -1,
                                131,
                                125,
                                127,
                                129,
                                -1
                            },
                            {
                                -1,
                                137,
                                133,
                                -1,
                                135,
                                -1
                            },
                            {
                                -1,
                                -1,
                                139,
                                -1,
                                -1,
                                -1
                            },
                            {
                                -1,
                                -1,
                                -1,
                                -1,
                                -1,
                                -1
                            },
                            {
                                -1,
                                -1,
                                -1,
                                -1,
                                -1,
                                -1
                            },
                            {
                                -1,
                                -1,
                                -1,
                                -1,
                                -1,
                                -1
                            },
                            {
                                -1,
                                -1,
                                -1,
                                -1,
                                -1,
                                -1
                            },
                            {
                                -1,
                                -1,
                                -1,
                                -1,
                                -1,
                                -1
                            }
                        };
                    __instance.flags.voice.playVoices[__instance.numFemale] = array2[__instance.useItems[__instance.actionUseItem].idObj, array[num]];
                    __instance.isClickDragVoice = true;
                    __instance.voice.nowVoices[__instance.numFemale].notOverWrite = false;
                }
            }
            /// <summary>
            /// Obsolete by transpiler.
            /// We run voiceProcs for caress actions in non-Aibu modes.
            /// </summary>
            //[HarmonyPrefix]
            //[HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.DragAction))]
            //public static void DragActionPrefix(HandCtrl __instance)
            //{
            //    if ((__instance.flags.mode == HFlag.EMode.houshi || __instance.flags.mode == HFlag.EMode.sonyu) &&
            //        (__instance.voicePlayActionMoveOld > (__instance.voicePlayActionMove + __instance.calcDragLength.magnitude) % (float)__instance.voicePlayActionLoop) &&
            //        __instance.voice.nowVoices[__instance.numFemale].state != HVoiceCtrl.VoiceKind.voice)
            //    {
            //        __instance.voicePlayActionMove = 0f;
            //        __instance.voicePlayActionMoveOld = 0f;
            //        __instance.voicePlayActionLoop = UnityEngine.Random.Range(1000, 1500);
            //        int[] array = new int[]
            //        {
            //            0,
            //            1,
            //            1,
            //            2,
            //            3,
            //            4,
            //            4
            //        };
            //        int num = 0;
            //        if (__instance.actionUseItem != -1)
            //        {
            //            num = __instance.useItems[__instance.actionUseItem].kindTouch - HandCtrl.AibuColliderKind.mouth;
            //        }
            //        int[,] array3 = new int[,]
            //        {
            //            {
            //                -1,
            //                112,
            //                114,
            //                116,
            //                118,
            //                -1
            //            },
            //            {
            //                -1,
            //                124,
            //                120,
            //                122,
            //                -1,
            //                -1
            //            },
            //            {
            //                -1,
            //                132,
            //                126,
            //                128,
            //                130,
            //                -1
            //            },
            //            {
            //                -1,
            //                138,
            //                134,
            //                -1,
            //                136,
            //                -1
            //            },
            //            {
            //                -1,
            //                -1,
            //                140,
            //                -1,
            //                -1,
            //                -1
            //            },
            //            {
            //                -1,
            //                -1,
            //                -1,
            //                -1,
            //                -1,
            //                -1
            //            },
            //            {
            //                -1,
            //                -1,
            //                -1,
            //                -1,
            //                -1,
            //                -1
            //            },
            //            {
            //                -1,
            //                -1,
            //                -1,
            //                -1,
            //                -1,
            //                -1
            //            },
            //            {
            //                -1,
            //                -1,
            //                -1,
            //                -1,
            //                -1,
            //                -1
            //            },
            //            {
            //                -1,
            //                -1,
            //                -1,
            //                -1,
            //                -1,
            //                -1
            //            }
            //        };
            //        __instance.flags.voice.playVoices[__instance.numFemale] = array3[__instance.useItems[__instance.actionUseItem].idObj, array[num]];
            //        __instance.isClickDragVoice = true;
            //        __instance.voice.nowVoices[__instance.numFemale].notOverWrite = false;
            //    }
            //}
            /// <summary>
            /// We disable (half the time) override of a voice with the short by HitReactionPlay().
            /// Otherwise happens way too often in non-Aibu modes during caress.
            /// </summary>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.HitReactionPlay))]
            public static void HitReactionPlayPrefix(ref bool _playShort, HandCtrl __instance)
            {
                if (__instance.actionUseItem != -1 && __instance.voice.nowVoices[__instance.numFemale].state == HVoiceCtrl.VoiceKind.voice && UnityEngine.Random.value < 0.5f)
                    _playShort = false;
                LoopController.Instance.OnUserInput();
            }
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HMotionEyeNeck), nameof(HMotionEyeNeck.SetNeckTarget))]
            public static void SetNeckTargetPrefix(ref int _tag, float _rate, GameObject _objCamera, bool _isConfigDisregard, HMotionEyeNeckMale __instance)
            {
                if (MoveNeckGlobal && _tag != 0 && __instance.chara.sex == 1)
                {
                    for (var i = 0; i < _chaControl.Count; ++i)
                    {
                        if (__instance.chara == _chaControl[i] && FemalePoI[i] != null)
                            _tag = 0;
                    }
                }
                //if (__instance.chara.sex == 0 && _tag != 0 && MalePoI != null)
                //{
                //    _tag = 0;
                //}

            }
            /// <summary>
            /// We run voiceProc for first item attached.
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.FinishAction))]
            public static void HandCtrlFinishActionPostfix(HandCtrl __instance)
            {
                if (FirstTouch)
                {
                    if (__instance.flags.mode == HFlag.EMode.aibu)
                        __instance.flags.voice.timeAibu.timeIdle = 0.75f;
                    else
                        SensibleHController.Instance.DoFirstTouchProc();
                }
                FirstTouch = false;
            }

            /// <summary>
            /// We prevent counters for voiceProc from resetting on consecutive click actions.
            /// And add a bit for every click.
            /// </summary>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.JudgeProc))]
            public static void HandCtrlJudgeProcPrefix(HandCtrl __instance, ref float __state)
            {
                if (MoMiActive)
                    __state = __instance.voicePlayActionMove + UnityEngine.Random.Range(25f, 50f);
            }
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.JudgeProc))]
            public static void HandCtrlJudgeProcPostfix(HandCtrl __instance, float __state)
            {
                if (MoMiActive)
                {
                    __instance.voicePlayActionMove = __state;
                    __instance.voicePlayActionMoveOld = __state;
                    __instance.voicePlayActionLoop = 750;
                }
            }
            /// <summary>
            /// OLoop outside of orgasm enabler.
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSonyu), nameof(HSonyu.Proc))]
            public static void AfterHSonyuProc(HSonyu __instance)
            {
                if (OLoop && __instance.flags.mode == HFlag.EMode.sonyu)
                    __instance.LoopProc(true);
            }

            /// <summary>
            /// Breath in OLoop enabler.
            /// </summary>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HVoiceCtrl), nameof(HVoiceCtrl.BreathProc))]
            public static void BreathProcPrefix(ref AnimatorStateInfo _ai, HVoiceCtrl __instance)
            {
                if (OLoop && __instance.flags.mode == HFlag.EMode.sonyu)
                    _ai = sLoopInfo;
            }

            /// <summary>
            /// By default when there is an item and no action is conducted, there is no voice procs.
            /// We look for lack of action and non "Idle" pose or "Orgasm" pose and run IsIdleTime() timer on proc, and we handle it ourselves.
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HAibu), nameof(HAibu.Proc))]
            public static void HAibuProcPostfix(HAibu __instance)
            {
                if (__instance.hand.actionUseItem == -1
                    && __instance.voice.nowVoices[0].state != HVoiceCtrl.VoiceKind.voice
                    && (__instance.flags.nowAnimStateName.EndsWith("_Idle", System.StringComparison.Ordinal) 
                    || __instance.flags.nowAnimStateName.EndsWith("A", System.StringComparison.Ordinal)))
                {
                    if (__instance.flags.voice.timeAibu.IsIdleTime()
                        && UnityEngine.Random.value < 0.75f)
                        if (__instance.flags.nowAnimStateName.EndsWith("A", System.StringComparison.Ordinal))
                        {
                            // We run after orgasm voice.
                            __instance.flags.voice.isAfterVoicePlay = false;
                            __instance.flags.voice.playVoices[0] = 143;
                        }
                        else
                        {
                            // We run caress voice for idle item attached.
                            SensibleHController.Instance.DoFirstTouchProc();
                        }
                }
            }
            //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.SetLayerWeight))]
            //public static void SetLayerWeight()
            //{
            //    SensibleH.Logger.LogDebug($"SetLayerWeight");
            //}
            //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.EnableDynamicBone))]
            //public static void EnableDynamicBone()
            //{
            //    SensibleH.Logger.LogDebug($"EnableDynamicBone");
            //}
            //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.SetItem))]
            //public static void SetItemPrefix()
            //{
            //    SensibleH.Logger.LogDebug("SetItem");
            //}
            //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.AnimatrotRestrart))]
            //public static void AnimatrotRestrartPrefix()
            //{
            //    SensibleH.Logger.LogDebug("AnimatrotRestrart");
            //}
            //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.KissAction))]
            //public static void KissActionPrefix()
            //{
            //    SensibleH.Logger.LogDebug("KissAction");
            //}
            //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.DragAction))]
            //public static void DragActionPrefix()
            //{
            //    SensibleH.Logger.LogDebug("DragAction");
            //}
            //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.FinishAction))]
            //public static void FinishActionPrefix(HandCtrl __instance)
            //{
            //    SensibleH.Logger.LogDebug($"FinishAction[timeDragCalc: {__instance.flags.timeDragCalc}]");
            //}
            //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.JudgeProc))]
            //public static void JudgeProcPrefix()
            //{
            //    SensibleH.Logger.LogDebug("JudgeProc");
            //}
            //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.SetDragStartLayer))]
            //public static void SetDragStartLayerPrefix()
            //{
            //    SensibleH.Logger.LogDebug("SetDragStartLayer");
            //}
            //[HarmonyPrefix, HarmonyPatch(typeof(HFlag), nameof(HFlag.WaitSpeedProcItem))]
            //public static void WaitSpeedProcItemPrefix()
            //{
            //    SensibleH.Logger.LogDebug("WaitSpeedProcItem");
            //}
            //[HarmonyPrefix, HarmonyPatch(typeof(HFlag), nameof(HFlag.WaitSpeedProcAibu))]
            //public static void WaitSpeedProcAibuPrefix()
            //{
            //    SensibleH.Logger.LogDebug("WaitSpeedProcAibu");
            //}
            //[HarmonyPrefix, HarmonyPatch(typeof(HAibu), nameof(HAibu.Proc))]
            //public static void HaibuProcPrefix(HAibu __instance)
            //{
            //    SensibleH.Logger.LogDebug($"HaibuProc[{__instance.hand.actionUseItem}");
            //}

            //[HarmonyPrefix, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.ClickAction))]
            //public static void ClickActionPrefix()
            //{
            //    SensibleH.Logger.LogDebug("ClickAction");
            //}
            //[HarmonyPrefix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeLookNeckTarget))]
            //public static void ChangeChangeLookNeckTargetPrefix()//(int targetType, Transform trfTarg = null, float rate = 0.5f, float rotDeg = 0f, float range = 1f, float dis = 0.8f)
            //{
            //    SensibleH.Logger.LogDebug("ChangeLookNeckTarget ");
            //}
            //[HarmonyPrefix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeLookNeckPtn))]
            //public static void ChangeLookNeckPtnPrefix(int ptn, float rate = 1f)
            //{
            //    SensibleH.Logger.LogDebug($"ChangeLookNeckPtn {ptn}");
            //}
            /*
             * During kiss neckPtn is 3;
             * eyes - 0, 2, 4, 25
             * brows - 0, 2, 11
             */

            //[HarmonyPrefix]
            //[HarmonyPatch(typeof(FBSBase), nameof(ChaInfo.eyesCtrl.ChangePtn))]
            //public static void ChangePtnEyesPrefix(ref int ptn, ref bool blend)
            //{
            //    ptn = 25;
            //    blend = true;
            //    SensibleH.Logger.LogDebug($"ChangePtnEyes {ptn}");
            //}

            //[HarmonyPrefix]
            //[HarmonyPatch(typeof(FBSBase), nameof(ChaInfo.eyebrowCtrl.ChangePtn))]
            //public static void ChangePtnEyebrowPrefix(ref int ptn, ref bool blend)
            //{
            //    ptn = 14;
            //    blend = true;
            //    SensibleH.Logger.LogDebug($"ChangePtnEyebrow {ptn}");
            //}
            //[HarmonyPrefix]
            //[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyebrowOpenMax))]
            //public static void ChangeEyebrowOpenMaxPrefix(ref float maxValue)
            //{
            //    maxValue = 0f;
            //}
            //[HarmonyPrefix]
            //[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyesOpenMax))]
            //public static void ChangeEyesOpenMaxPrefix(ref float maxValue)
            //{
            //    maxValue = 0f;
            //}ChangeCoordinateTypeAndReload
            //[HarmonyPrefix]
            //[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCoordinateType), new System.Type[] { typeof(bool) })]
            //[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCoordinateType), new System.Type[] { typeof(ChaFileDefine.CoordinateType), typeof(bool) })]
            //public static void ChangeCoordinateType(ref bool changeBackCoordinateType)
            //{
            //    changeBackCoordinateType = false;
            //}
        }


    }
}


