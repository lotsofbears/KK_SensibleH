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
        //[HarmonyPatch(typeof(FBSBase), nameof(ChaInfo.eyesCtrl.ChangePtn))]
        //[HarmonyPatch(typeof(FBSBase), nameof(ChaInfo.eyebrowCtrl.ChangePtn))]
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
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HVoiceCtrl), nameof(HVoiceCtrl.VoiceProc))]
        //public static void VoiceProcPrefix(AnimatorStateInfo _ai, ChaControl _female, int _main, HVoiceCtrl __instance)
        //{
        //    if (_frenchKiss || Cyu.Instance.kissPhase == Cyu.Phase.Disengaging)
        //        __instance.flags.voice.playVoices[_main] = -1;
        //}
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeLookEyesPtn))]
        //public static void ChangeEyebrowOpenMaxPrefix(ref int ptn)
        //{
        //    ptn = 0;
        //}

        //[HarmonyTranspiler, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyebrowOpenMax))]
        //public static IEnumerable<CodeInstruction> ChangeEyebrowOpenMaxTranspiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    var code = new List<CodeInstruction>(instructions);
        //    code[0].opcode = OpCodes.Ldsfld;
        //    code[0].operand = AccessTools.Field(typeof(Cyu), nameof(Cyu.EyesOpenness));
        //    code[1].opcode = OpCodes.Starg;
        //    code[1].operand = 1;
        //    code[2].opcode = OpCodes.Nop;
        //    code[3].opcode = OpCodes.Nop;
        //    return code.AsEnumerable();

        //}
        //[HarmonyTranspiler, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyesOpenMax))]
        //public static IEnumerable<CodeInstruction> ChangeEyesOpenMaxTranspiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    var code = new List<CodeInstruction>(instructions);
        //    code[0].opcode = OpCodes.Ldsfld;
        //    code[0].operand = AccessTools.Field(typeof(Cyu), nameof(Cyu.EyesOpenness));
        //    code[1].opcode = OpCodes.Starg;
        //    code[1].operand = 1;
        //    code[2].opcode = OpCodes.Nop;
        //    code[3].opcode = OpCodes.Nop;
        //    return code.AsEnumerable();

        //}

    }
}
