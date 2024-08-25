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
    /// We throw away camera/effects related things that cause Huge stutters on the kiss and present zero use in VR.
    /// KKS should work.
    /// </summary>
    class PatchMoMiVR
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
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.DragAction))]
        public static IEnumerable<CodeInstruction> DragActionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var pop = 0;
            var firstPart = false;
            var secondPart = false;
            var getButton = 0;
            foreach (var code in instructions)
            {
                if (pop != 2)
                {
                    if (code.opcode == OpCodes.Pop)
                    {
                        pop += 1;
                        //SensibleH.Logger.LogDebug($"DragActionTranspiler[{code.opcode} {code.operand}]");
                    }
                }
                else if (!firstPart)
                {
                    if (code.opcode == OpCodes.Callvirt
                        && code.operand.ToString().Contains("set_useDOF"))
                        firstPart = true;
                    //SensibleH.Logger.LogDebug($"DragActionTranspiler[firstPart] {code.opcode} {code.operand}]");
                    yield return new CodeInstruction(OpCodes.Nop);
                    continue;
                }
                else if (getButton != 2 && code.opcode == OpCodes.Call &&
                    code.operand is MethodInfo methodInfo &&
                    methodInfo.Name.Equals("GetMouseButton"))
                {
                    //SensibleH.Logger.LogDebug($"DragActionTranspiler[button]{code.opcode} {code.operand}]");
                    getButton++;
                    if (getButton == 2)
                        pop = 0;
                }
                else if (getButton == 2 && !secondPart)
                {
                    //SensibleH.Logger.LogDebug($"DragActionTranspiler[secondPart]{code.opcode} {code.operand}]");
                    if (code.opcode == OpCodes.Callvirt
                        && code.operand.ToString().Contains("set_useDOF"))
                        secondPart = true;

                    yield return new CodeInstruction(OpCodes.Nop);
                    continue;
                }
                
                yield return code;
            }
        }
    }
}
