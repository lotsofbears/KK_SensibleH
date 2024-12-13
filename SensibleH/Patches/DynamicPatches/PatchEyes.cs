using HarmonyLib;
using static KK_SensibleH.Caress.Kiss;


namespace KK_SensibleH.Patches.DynamicPatches
{
    internal class PatchEyes
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyebrowOpenMax))]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyesOpenMax))]
        public static void PrefixChangeEyebrowOpenMax(ref float maxValue, ChaControl __instance)
        {
            if (__instance.sex == 1)
            {
                maxValue = _eyesOpenness;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyebrowPtn))]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyesPtn))]
        public static void ChangePtnPrefix(ref int ptn, ref bool blend, ChaControl __instance)
        {
            if (__instance.sex == 1)
            {
                ptn = 0;
                blend = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyesBlinkFlag))]
        public static void ChangeEyesBlinkFlagPrefix(ref bool blink)
        {
            blink = false;
        }
    }
}
