using ADV.Commands.Base;
using HarmonyLib;
using Illusion.Game.Elements.EasyLoader;
using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace KK_SensibleH.Patches.StaticPatches
{
    internal class TestGame
    {
        //[HarmonyTranspiler, HarmonyPatch(typeof(Illusion.Game.Elements.EasyLoader.Motion), nameof(Illusion.Game.Elements.EasyLoader.Motion.LoadAnimator),
        //    new Type[] {
        //        typeof(Animator),
        //        typeof(string),
        //        typeof(string)
        //    })]
        //public static IEnumerable<CodeInstruction> NoWayTrans(IEnumerable<CodeInstruction> instructions)
        //{
        //    SensibleH.Logger.LogDebug($"Trans:Motion:Start");
        //    foreach (var code in instructions)
        //    {
        //        if (code.opcode == OpCodes.Ldarg_2 || code.opcode == OpCodes.Ldarg_3)
        //        {
        //            SensibleH.Logger.LogDebug($"Trans:Motion:{code.opcode}:{code.operand}");
        //            yield return new CodeInstruction(OpCodes.Nop);
        //            continue;
        //        }
        //        if (code.opcode == OpCodes.Newobj)
        //        {
        //            SensibleH.Logger.LogDebug($"Trans:Motion:{code.opcode}:{code.operand}");
        //            yield return new CodeInstruction(OpCodes.Ldarg_0);
        //            continue;
        //        }
        //        yield return code;
        //    }
        //}
    }

}
