using ADV;
using HarmonyLib;
using Illusion.Game;
using Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KK_SensibleH.Patches
{
    internal class PatchGame
    {
        private static Dictionary<ChaControl, bool> HoHoTracking = [];

        //public static int[] PersonalitiesKKS = { 39, 40, 41, 42, 43 };

        /// <summary>
        /// We change blush over time instead of instant. A game changer in VR. LF that Marco plugin with skin effects like this too. 
        /// </summary>

        [HarmonyPrefix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeHohoAkaRate))]
        public static bool ChangeHohoAkaRatePrefix(float value, ChaControl __instance)
        {
            if (__instance.fileStatus.hohoAkaRate != value && !KKAPI.SceneApi.GetLoadSceneName().Equals("CustomScene") && !KKAPI.SceneApi.GetAddSceneName().Equals("CustomScene"))
            {
                if (HoHoTracking.ContainsKey(__instance))
                {
                    if (!HoHoTracking[__instance])
                    {
                        __instance.StartCoroutine(ChangeOverTime(__instance.fileStatus.hohoAkaRate, value, __instance));
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    HoHoTracking.Add(__instance, true);
                    __instance.StartCoroutine(ChangeOverTime(__instance.fileStatus.hohoAkaRate, value, __instance));
                    return false;
                }

            }
            else
            {
                return true;
            }
        }
        public static IEnumerator ChangeOverTime(float from, float to, ChaControl instance)
        {
            // There is a bug that leaves the loop hanging at "to" value. Trying to catch it.
            // Correlation with disabled behavior? loop is still running though.
            HoHoTracking[instance] = true;
            var absStep = Mathf.Min(Time.deltaTime, 0.03f) * 0.2f;
            var step = from > to ? -absStep : absStep;
            //SensibleH.Logger.LogWarning($"StartChangeOverTime[{instance}][from:{from}][to:{to}][step:{step}][absStep:{absStep}][timeDelta:{timeDelta}]");
            while (Mathf.Abs(from - to) > absStep)
            {
                from += step;
                //SensibleH.Logger.LogDebug($"ChangeOverTime[{from}]");
                instance.ChangeHohoAkaRate(from);
                yield return null;
            }
            instance.ChangeHohoAkaRate(to);
            HoHoTracking[instance] = false;
            //SensibleH.Logger.LogWarning($"EndChangeOverTime[{instance}]");
        }
#if KKS
        /// <summary>
        /// New KKS voices of old personalities are of lower quality/higher base pitch(speed). Atleast in clients on my hands.
        /// So we tweak ADV voices a tad. There are some for H too, but for now we skip them.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(Voice), nameof(Voice.Play), new Type[] { typeof(Voice.Loader) })]
        public static void VoicePlayPrefix(ref Voice.Loader loader)
        {
            //SensibleH.Logger.LogDebug($"SetLipSync[{loader.no}][{loader.asset}][{loader.bundle}][{loader.pitch}]");
            if (loader.no < 39 && loader.asset.StartsWith("sun", StringComparison.Ordinal))
            {
                loader.pitch -= 0.02f;
            }
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(GameAssist), nameof(GameAssist.DecreaseTalkTime))]
#else
        /// <summary>
        /// We set extra talk attempts.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(Communication), nameof(Communication.DecreaseTalkTime))]
#endif
        public static void DecreaseTalkTimePrefix(ref int _value)
        {
            if ((1f / SensibleH.ConfigTalkTime.Value) < UnityEngine.Random.value)
            {
                _value = 0;
            }
        }

        /// <summary>
        /// Auto ADV default if there is no heroine present.
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(TextScenario), nameof(TextScenario.Initialize))]
        public static void TextScenarioInitializePostfix(TextScenario __instance)
        {
            if (SensibleH.AutoADV.Value &&  __instance.advScene != null)
            {
                __instance._isAuto = __instance.advScene.scenario.currentChara == null;
            }
        }

        /// <summary>
        /// We get rid of those pesky sounds on button clicks that poison VR experience.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Utils.Sound), nameof(Utils.Sound.Play), typeof(SystemSE))]
        public static bool UtilsSoundPlayPrefix(SystemSE se)
        {
            if (se == SystemSE.sel && SensibleH.DisablePeskySounds.Value)
            {
                return false;
            }
            return true;
        }
    }
}
