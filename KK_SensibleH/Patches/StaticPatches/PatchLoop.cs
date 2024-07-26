using HarmonyLib;
using Illusion.Game;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Reflection;

namespace KK_SensibleH.Patches.StaticPatches
{
    internal class PatchLoop
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnPullClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnRelyClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertAnalNoVoiceClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertAnalClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertNoVoiceClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertClick))]

        public static IEnumerable<CodeInstruction> OnClickTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var methodToPatch = nameof(Utils.Sound.Play);
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo methodInfo
                    && methodInfo.Name.Equals(methodToPatch))
                //if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("Play"))
                {
                    //SensibleH.Logger.LogDebug("Transpiler[Loop]");
                    codes[i].opcode = OpCodes.Nop;
                    codes[i - 1].opcode = OpCodes.Nop;
                    break;
                }
            }
            return codes.AsEnumerable();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnAutoFinish))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertAnalClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnChangeMotionClick))] // Changes loop type. RMB.
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnSpeedUpClick))]
        public static void SetCondomPostfix()
        {
            LoopController.Instance.OnUserInput();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HSprite), nameof(HSprite.OnPullClick))]
        public static void HandleOnPullClick()
        {
            LoopController.Instance.OnUserInput();
            LoopController.Instance.DoSonyuClick(pullOut: true);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertNoVoiceClick))]
        public static void OnInsertClickPostfix()
        {
            LoopController.Instance.OnUserInput();
            LoopController.Instance.DoSonyuClick(pullOut: false);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertAnalNoVoiceClick))]
        public static void OnInsertAnalClickPostfix()
        {
            LoopController.Instance.OnUserInput();
            LoopController.Instance.DoAnalClick();
        }
    }
}
