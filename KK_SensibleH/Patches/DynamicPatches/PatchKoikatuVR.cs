using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using KoikatuVR;
using KoikatuVR.Caress;

namespace KK_SensibleH.Patches.DynamicPatches
{
    class PatchKoikatuVR
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(HandCtrlHooks), nameof(HandCtrlHooks.InjectMouseButtonUp))]
        public static IEnumerable<CodeInstruction> RemoveInjectMouseButtonUp(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Ret)
                    codes[i].opcode = OpCodes.Nop;
                
            }
            return codes.AsEnumerable();
        }
    }
}
