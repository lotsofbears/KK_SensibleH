using HarmonyLib;
using Illusion.Game;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Reflection;
using KK_SensibleH.AutoMode;

namespace KK_SensibleH.Patches.StaticPatches
{
    internal class PatchLoop
    {
        struct CodeInfo
        {
            public OpCode firstOpcode;
            public string firstOperand;
            public OpCode secondOpcode;
            public string secondOperand;
        }

        public static bool FakeButtonUp;
        /// <summary>
        /// We get rid of pesky sound on button clicks.
        /// </summary>
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnPullClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnRelyClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertAnalNoVoiceClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertAnalClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertNoVoiceClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnAutoFinish))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnCondomClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsideClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnOutsideClick))]
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
#if KKS
                    codes[i + 1].opcode = OpCodes.Nop;
#endif
                    break;
                }
            }
            return codes.AsEnumerable();
        }
        /// <summary>
        /// Actions that we interpret as user input.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnAutoFinish))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertAnalClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnChangeMotionClick))] // Changes loop type. RMB.
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnSpeedUpClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnCondomClick))]
        [HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.Reaction))]
        public static void ManyActionsPostfix()
        {
            LoopController.Instance.OnUserInput();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSonyu), nameof(HSonyu.LoopProc))]
        public static void HSonyuLoopProcPostfix(HSonyu __instance)
        {
            if (__instance.flags.finish != HFlag.FinishKind.none)
            {
                LoopController.Instance.OnPreClimax();
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HSprite), nameof(HSprite.OnPullClick))]
        public static void HandleOnPullClick()
        {
            LoopController.Instance.OnUserInput();
            LoopController.Instance.OnSonyuClick(pullOut: true);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertNoVoiceClick))]
        public static void OnInsertClickPostfix()
        {
            LoopController.Instance.OnUserInput();
            LoopController.Instance.OnSonyuClick(pullOut: false);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertAnalNoVoiceClick))]
        public static void OnInsertAnalClickPostfix()
        {
            LoopController.Instance.OnUserInput();
            LoopController.Instance.DoAnalClick();
        }
        [HarmonyPrefix, HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonUp))]
        public static bool GetMouseButtonUpPrefix(int button, ref bool __result)
        {
            if (button == 0 && FakeButtonUp)
            {
                __result = true;
                return false;
            }
            return true;

        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddAibuOrg))]
        [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuOrg))]
        [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuAnalOrg))]
        public static void OrgasmFemalePostfix()
        {
            LoopController.Instance.OnOrgasmF();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddHoushiInside))]
        [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddHoushiOutside))]
        [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuInside))]
        [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuOutside))]
        [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuCondomInside))]
        [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuAnalInside))]
        [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuAnalOutside))]
        [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuAnalCondomInside))]
        public static void OrgasmMalePostfix()
        {
            LoopController.Instance.OnOrgasmM();
        }
        public static float FemaleCeiling = 100f;
        public static float FemaleUpThere = 70f;
        private static bool RandomBinary() => UnityEngine.Random.value < 0.5f;
        public static bool Play70Voice(HandCtrl handCtrl) => handCtrl.IsKissAction() || RandomBinary();
        /// <summary>
        /// We override 70+ voice lines with kiss lines, otherwise we play them with 50% chance instead of once per gauge fill. 
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HSonyu), nameof(HSonyu.LoopProc))]
        public static IEnumerable<CodeInstruction> HSonyuLoopProcTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var part = -1;
            var counter = 0;
            var done = false;
            var hand = AccessTools.Field(typeof(HSonyu), "hand");
            var isKiss = AccessTools.Method(typeof(PatchLoop), nameof(Play70Voice));
            var upThere = AccessTools.Field(typeof(PatchLoop), "FemaleUpThere");
            //SensibleH.Logger.LogDebug($"LoopProc[Start][{hand}]\n[{isKiss}]");
            foreach (var code in  instructions)
            {
                if (!done)
                {
                    if (part == -1)
                    {
                        if (counter == 0 && code.opcode == OpCodes.Call && code.operand is MethodInfo info
                            && info.Name.Equals("RangeEqualOn"))
                        {
                            counter++;
                        }
                        else if (counter == 1)
                        {
                            if (code.opcode == OpCodes.Ldc_R4)
                            {
                                yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PatchLoop), "FemaleCeiling"));
                                counter = 0;
                                part++;
                                continue;
                            }
                        }
                    }
                    else if (part < 2)
                    {
                        if (counter == 0 && code.opcode == OpCodes.Ldc_R4
                            && code.operand is float number
                            && number == 70f)
                        {
                            yield return new CodeInstruction(OpCodes.Ldsfld, upThere);
                            counter++;
                            continue;
                        }
                        else if (counter == 1)
                        {
                            if (code.opcode == OpCodes.Blt_Un)
                            {
                                //SensibleH.Logger.LogDebug($"LoopProc[Trans][{code.opcode}][{code.operand}]");
                                counter++;
                            }
                            else
                            {
                                //SensibleH.Logger.LogDebug($"LoopProc[Trans][Wrong][{code.opcode}][{code.operand}]");
                                counter = 0;
                            }
                        }
                        else if (counter == 2)
                        {
                            //SensibleH.Logger.LogDebug($"LoopProc[Trans][{code.opcode}][{code.operand}]");
                            if (code.opcode == OpCodes.Ldfld
                                && code.operand is FieldInfo field)
                            {
                                if (field.Name.Equals("flags"))
                                {
                                    yield return new CodeInstruction(OpCodes.Nop);
                                    continue;
                                }
                                else if (field.Name.Equals("voice"))
                                {
                                    yield return new CodeInstruction(OpCodes.Ldfld, hand);
                                    continue;
                                }
                                else if (field.Name.Equals("isFemale70PercentageVoicePlay") || field.Name.Equals("isMale70PercentageVoicePlay"))
                                {
                                    //SensibleH.Logger.LogDebug($"LoopProc[Trans][{part} is done]");
                                    yield return new CodeInstruction(OpCodes.Call, isKiss);
                                    counter = 0;
                                    part++;
                                    continue;
                                }
                            }
                        }
                    }
                    else if (part == 2)
                    {
                        if (counter == 0 && code.opcode == OpCodes.Callvirt
                            && code.operand is MethodInfo info && info.Name.Equals("SetSonyuIdleTime"))
                        {
                            //SensibleH.Logger.LogDebug($"LoopProc[Trans][Found][{code.opcode}][{code.operand}]");
                            counter++;
                        }
                        else if (counter == 1 && code.opcode == OpCodes.Br)
                        {
                            //SensibleH.Logger.LogDebug($"LoopProc[Trans][Found][{code.opcode}][{code.operand}]");
                            counter++;
                        }
                        else if (counter == 2)
                        {
                            //SensibleH.Logger.LogDebug($"LoopProc[Trans][{code.opcode}][{code.operand}]");
                            if (code.opcode == OpCodes.Ldarg_0)
                            {
                                code.opcode = OpCodes.Ldc_I4_0;
                            }
                            else if (code.opcode == OpCodes.Ldfld)
                            {
                                yield return new CodeInstruction(OpCodes.Nop);
                                continue;
                                //if (field.Name.Equals("flags"))
                                //{
                                //    yield return new CodeInstruction(OpCodes.Nop);
                                //    continue;
                                //}
                                //else if (field.Name.Equals("voice"))
                                //{
                                //    yield return new CodeInstruction(OpCodes.Ldfld, hand);
                                //    continue;
                                //}
                                //else if (field.Name.Equals("isFemale70PercentageVoicePlay") || field.Name.Equals("isMale70PercentageVoicePlay"))
                                //{
                                //    yield return new CodeInstruction(OpCodes.Call, isKiss);
                                //    continue;
                                //}
                            }
                            else if (code.opcode == OpCodes.Brtrue)
                            {
                                //yield return new CodeInstruction(OpCodes.Brfalse, code.operand);
                                counter++;
                                //SensibleH.Logger.LogDebug($"LoopProc[Trans][{part} is done]");
                                //continue;
                            }

                        }
                        else if (counter == 3)
                        {
                            //SensibleH.Logger.LogDebug($"LoopProc[Trans][Removing][{code.opcode}][{code.operand}]");
                            if (code.opcode == OpCodes.Brtrue)
                            {
                                done = true;
                            }
                            yield return new CodeInstruction(OpCodes.Nop);
                            continue;
                        }
                    }
                }
                yield return code;
            }

        }
        [HarmonyTranspiler, HarmonyPatch(typeof(HAibu), nameof(HAibu.Proc))]
        public static IEnumerable<CodeInstruction> HAibuProcTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var field = AccessTools.Field(typeof(PatchLoop), "FemaleCeiling");
            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldc_R4 
                    && code.operand is float number && number == 100f)
                {
                    SensibleH.Logger.LogDebug($"HAibuProc:{code.opcode}:{code.operand}");
                    yield return new CodeInstruction(OpCodes.Ldsfld, field);
                    continue;
                }
                yield return code;
            }
        }
    }
}
