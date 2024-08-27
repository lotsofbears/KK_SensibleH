using HarmonyLib;
using Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KK_SensibleH.Patches.StaticPatches
{
    internal class PatchGame
    {
        private static Dictionary<ChaControl, bool> HoHoTracking = new Dictionary<ChaControl, bool>();

        //public static int[] PersonalitiesKKS = { 39, 40, 41, 42, 43 };

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
            HoHoTracking[instance] = true;
            var timeDelta = Time.deltaTime;
            var absStep = Time.deltaTime * 0.2f;
            var step = from > to ? -absStep : absStep;
            SensibleH.Logger.LogWarning($"StartChangeOverTime[{instance}][from:{from}][to:{to}][step:{step}][absStep:{absStep}][timeDelta:{timeDelta}]");
            while (Mathf.Abs(from - to) > absStep)
            {
                from += step;
                //SensibleH.Logger.LogDebug($"ChangeOverTime[{from}]");
                instance.ChangeHohoAkaRate(from);
                yield return null;
            }
            instance.ChangeHohoAkaRate(to);
            HoHoTracking[instance] = false;
            SensibleH.Logger.LogWarning($"EndChangeOverTime[{instance}]");
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
        [HarmonyPrefix, HarmonyPatch(typeof(Manager.Communication), nameof(Manager.Communication.DecreaseTalkTime))]
#endif
        public static void DecreaseTalkTimePrefix(ref int _value)
        {
            //SensibleH.Logger.LogDebug($"DecreaseTalkTime");
            if (UnityEngine.Random.value > 0.1f)
            {
                _value = 0;
            }

        }
    }
}
