﻿#if KKS
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using KKAPI;

namespace KK_SensibleH.Patches
{
    public static class PatchObi
    {
        #region Allows render of fluids on all animations.

        [HarmonyPrefix, HarmonyPatch(typeof(HitCollisionEnableCtrl), nameof(HitCollisionEnableCtrl.SetPlayObi))]
        public static void StopResettingObi(string _animation, HitCollisionEnableCtrl __instance)
        {
            var dic = __instance.dicInfo;
            if (dic.Count != 0 && !dic.ContainsKey(_animation))
            {
                AddToDic(__instance);
            }
        }
        [HarmonyPrefix, HarmonyPatch(typeof(ParentObjectCtrl), nameof(ParentObjectCtrl.ProcObi))]
        public static void StopResettingObiParentObjectCtrl(string _nameNextAnimation, ParentObjectCtrl __instance)
        {
            var dic = __instance.dicInfo;
            if (dic.Count != 0 && !dic.ContainsKey(_nameNextAnimation))
            {
                AddToDic(__instance);
            }
        }

        public static void AddToDic(ParentObjectCtrl ctrl)
        {
            var dic = ctrl.dicInfo;
            var donor = dic.ElementAt(0).Value;
            foreach (var anim in ListOfAnimations)
            {
                if (!dic.ContainsKey(anim))
                {
                    dic.Add(anim, donor);
                }
            }
        }

        public static void AddToDic(HitCollisionEnableCtrl ctrl)
        {
            var dic = ctrl.dicInfo;
            var donor = dic.ElementAt(0).Value;
            foreach (var anim in ListOfAnimations)
            {
                if (!dic.ContainsKey(anim))
                {
                    dic.Add(anim, donor);
                }
            }
        }

        public static readonly List<string> ListOfAnimations = new List<string>
        {
            // Hopefully all.
            "Idle",
            "Pull",
            "IN_A",
            "M_IN_Start",
            "M_IN_Loop",
            "IN_A",
            "WF_IN_Start",
            "WF_IN_Loop",
            "WS_IN_A",
            "Insert",
            "InsertIdle",
            "WLoop",
            "SLoop",
            "OLoop",
            "IN_Start",
            "IN_Loop",
            "Oral_Idle_IN",
            "Oral_Idle",
            "Drink_IN",
            "Drink",
            "Drink_A",
            "Vomit_IN"



        };
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(HSonyu), nameof(HSonyu.Proc))]
        [HarmonyPatch(typeof(H3PSonyu), nameof(H3PSonyu.Proc))]
        [HarmonyPatch(typeof(HHoushi), nameof(HHoushi.AfterProc))]
        [HarmonyPatch(typeof(H3PHoushi), nameof(H3PHoushi.AfterProc))]
        public static IEnumerable<CodeInstruction> HSonyuProcRemovalOfObiTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            var counter = 0;
            for (var i = 0; i < code.Count; i++)
            {
                if (counter == 0 && code[i].opcode == OpCodes.Ldarg_0)
                {
                    counter++;
                }
                else if (counter == 1 && code[i].opcode == OpCodes.Ldfld
                    && code[i].operand is FieldInfo field
                    && field.Name.Equals("obi"))
                {
                    counter++;
                }
                else if (counter == 2 && code[i].opcode == OpCodes.Callvirt
                    && code[i].operand is MethodInfo method
                    && method.Name.Equals("Clear"))
                {
                    counter = 0;
                    code[i].opcode = OpCodes.Nop;
                    code[i].operand = null;
                    code[i - 1].opcode = OpCodes.Nop;
                    code[i - 1].operand = null;
                    code[i - 2].opcode = OpCodes.Nop;
                    code[i - 2].operand = null;
                }
                else
                {
                    counter = 0;
                }
            }
            return code.AsEnumerable();
        }

        #endregion
    }
}
#endif