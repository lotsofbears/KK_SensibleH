using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace KK_SensibleH.Patches.StaticPatches
{
    /// <summary>
    /// We throw away camera/effects related things that trigger GC like a clock on the kiss and present zero use in VR.
     /// </summary>
    class PatchHandCtrlVR
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.OnCollision))]
        public static IEnumerable<CodeInstruction> OnCollisionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var opcodeRet = 0;
            var firstPart = false;
            var secondPartStart = 0;
            var secondPartEnd = 0;
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (opcodeRet != 2)
                {
                    if (codes[i].opcode == OpCodes.Ret)
                        opcodeRet += 1;
                }
                else
                {
                    if (!firstPart && codes[i].opcode == OpCodes.Stfld
                        && codes[i].operand.ToString().Contains("ctrl"))
                    {
                        //SensibleH.Logger.LogDebug($"OnCollisionTranspiler[FirstPart] {codes[i].opcode} - {codes[i].operand}");
                        firstPart = true;
                        codes[i + 1].opcode = OpCodes.Nop;
                        codes[i + 2].opcode = OpCodes.Nop;
                        codes[i + 3].opcode = OpCodes.Nop;
                        codes[i + 4].opcode = OpCodes.Nop;
                        codes[i + 5].opcode = OpCodes.Nop;
                    }
                    else if (secondPartStart == 0 && codes[i].opcode == OpCodes.Stfld
                        && codes[i].operand.ToString().Contains("isKiss"))
                    {
                        secondPartStart = i + 1;
                    }
                    else if (codes[i].opcode == OpCodes.Ret)
                    {
                        secondPartEnd = i - 4;
                    }
                }
            }
            codes.RemoveRange(secondPartStart, secondPartEnd - secondPartStart);
            return codes.AsEnumerable();
        }
#if KK
        //[HarmonyTranspiler, HarmonyPatch(typeof(TalkScene), nameof(TalkScene.Start), MethodType.Enumerator)]
        //public static IEnumerable<CodeInstruction> TalkSceneStartTranspiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    var done = false;
        //    var counter = 0;
        //    SensibleH.Logger.LogDebug($"Trans:TalkScene:Start");
        //    foreach (var code in instructions)
        //    {
        //        if (!done)
        //        {
        //            if (counter > 1)
        //            {
        //                counter++;
        //                SensibleH.Logger.LogDebug($"Trans:TalkScene:{code}:{code.operand}:{counter}");
        //                yield return new CodeInstruction(OpCodes.Nop);
        //                if (counter == 5)
        //                {
        //                    done = true;
        //                }
        //                continue;
        //            }
        //            else if (code.opcode == OpCodes.Call && code.operand is MethodInfo info
        //                && info.Name.Equals($"get_Instance"))
        //            {
        //                counter++;
        //                SensibleH.Logger.LogDebug($"Trans:TalkScene:{code}:{code.operand}:{counter}");
        //                if (counter == 2)
        //                {
        //                    yield return new CodeInstruction(OpCodes.Call, AccessTools.FirstMethod(typeof(VRHelper), m => m.Name.Equals(nameof(VRHelper.GetOriginPosition))));
        //                    continue;
        //                }
        //            }
        //        }

        //        yield return code;
        //    }
        //}
#endif

        // No clue why i did this.
        //[HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.DragAction))]
        //public static IEnumerable<CodeInstruction> DragActionTranspiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    var pop = 0;
        //    var firstPart = false;
        //    var secondPart = false;
        //    var getButton = 0;
        //    foreach (var code in instructions)
        //    {
        //        //if (pop != 2)
        //        //{
        //        //    if (code.opcode == OpCodes.Pop)
        //        //    {
        //        //        pop += 1;
        //        //        SensibleH.Logger.LogDebug($"DragActionTranspiler[{code.opcode} {code.operand}]");
        //        //    }
        //        //}
        //        if (!firstPart)
        //        {
        //            if (code.opcode == OpCodes.Callvirt
        //                && code.operand.ToString().Contains("set_useDOF"))
        //                firstPart = true;
        //            SensibleH.Logger.LogDebug($"DragActionTranspiler[firstPart] {code.opcode} {code.operand}]");
        //            yield return new CodeInstruction(OpCodes.Nop);
        //            continue;
        //        }
        //        else if (getButton != 2 && code.opcode == OpCodes.Call &&
        //            code.operand is MethodInfo methodInfo &&
        //            methodInfo.Name.Equals("GetMouseButton"))
        //        {
        //            SensibleH.Logger.LogDebug($"DragActionTranspiler[button]{code.opcode} {code.operand}]");
        //            getButton++;
        //            //if (getButton == 2)
        //            //    pop = 0;
        //        }
        //        else if (getButton == 2 && !secondPart)
        //        {
        //            SensibleH.Logger.LogDebug($"DragActionTranspiler[secondPart]{code.opcode} {code.operand}]");
        //            if (code.opcode == OpCodes.Callvirt
        //                && code.operand.ToString().Contains("set_useDOF"))
        //                secondPart = true;

        //            yield return new CodeInstruction(OpCodes.Nop);
        //            continue;
        //        }

        //        yield return code;
        //    }
        //}
    }
}
