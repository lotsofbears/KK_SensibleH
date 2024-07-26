using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace KK_SensibleH.Patches.DynamicPatches
{
    class PatchKiss
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.DragAction))]
        public static IEnumerable<CodeInstruction> DragActionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            for (var i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldflda &&
                    code[i].operand.ToString().Contains("calcDragLength"))
                {
                    code[i].opcode = OpCodes.Ldsfld;
                    code[i].operand = AccessTools.Field(typeof(MoMiController), name: "FakeDragLength"); ;
                    code[i + 1].opcode = OpCodes.Ldc_R4;
                    code[i + 1].operand = 3f;
                    code[i + 2].opcode = OpCodes.Call;
                    code[i + 2].operand = AccessTools.FirstMethod(typeof(Vector2), method => method.Name.Equals("op_Multiply"));
                    code[i + 3].opcode = OpCodes.Stfld;
                    code[i + 3].operand = AccessTools.Field(typeof(HandCtrl), name: "calcDragLength");
                    code[i + 4].opcode = OpCodes.Nop;
                    code[i + 4].operand = null;
                    code[i + 5].opcode = OpCodes.Nop;
                    code[i + 5].operand = null;
                    break;
                }
            }
            return code.AsEnumerable();
        }
    }
}
