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
        internal static Func<int, bool> maleBreathDelegate;
        enum ClickType
        {
            Insert,
            Insert_novoice,
            InsertAnal,
            InsertAnal_novoice,
            Inside,
            Outside,
            Pull_novoice,
            Insert_female,
            Insert_novoice_female,
            InsertAnal_female,
            InsertAnal_novoice_female,
            Inside_female,
            Outside_female,
            Pull_novoice_female,
            InserDark,
            Insert_novoiceDark,
            InsertAnalDark,
            InsertAnal_novoiceDark,
            InsideDark,
            OutsideDark,
            Pull_novoiceDark
        }
        public static bool FakeButtonUp;
        /// <summary>
        /// Actions that we interpret as user input.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnAutoFinish))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnChangeMotionClick))] // Changes loop type. RMB.
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnSpeedUpClick))]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnCondomClick))]
        [HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.Reaction))]
        public static void ManyActionsPostfix()
        {
            LoopController.OnUserInput();
        }
        [HarmonyPrefix, HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertClick))]
        public static bool OnInsertPrefix()
        {
            if (maleBreathDelegate != null && maleBreathDelegate((int)ClickType.Insert))
            {
                return false;
            }
            //LoopController.OnUserInput();
            return true;
        }
        [HarmonyPrefix, HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertAnalClick))]
        public static bool OnInsertAnalPrefix()
        {
            if (maleBreathDelegate != null && maleBreathDelegate((int)ClickType.InsertAnal))
            {
                return false;
            }
            //LoopController.OnUserInput();
            return true;
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnPullClick))]
        public static void HandleOnPullClick()
        {
            maleBreathDelegate?.Invoke((int)ClickType.Pull_novoice);
            LoopController.Instance.OnSonyuClick(pullOut: true);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertNoVoiceClick))]
        public static void OnInsertNoVoiceClickPostfix()
        {
            maleBreathDelegate?.Invoke((int)ClickType.Insert_novoice);
            LoopController.Instance.OnSonyuClick(pullOut: false);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnInsertAnalNoVoiceClick))]
        public static void OnInsertAnalNoVoiceClickPostfix()
        {
            maleBreathDelegate?.Invoke((int)ClickType.InsertAnal_novoice);
            LoopController.Instance.DoAnalClick();
        }

        // Necessary to click a fake button that changes animations.
        // Invocation didn't work i think?
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonUp))]
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
        /// <summary>
        /// The value when orgasm animation plays, shuffled up by the plugin.
        /// </summary>
        public static float FemaleCeiling = 100f;
        /// <summary>
        /// The value when pre-orgasm voices start to play, shuffled by the plugin.
        /// </summary>
        public static float FemaleUpThere = 70f;
        private static bool RandomBinary() => UnityEngine.Random.value < 0.5f;
        public static bool Play70Voice(HandCtrl handCtrl)
        {
            handCtrl.flags.voice.SetSonyuWaitTime(true);
            return handCtrl.IsKissAction() || RandomBinary();
        }

        /// <summary>
        /// We override 70+ voice lines with kiss lines if kissing, otherwise we play them with 50% chance instead of once per gauge fill. 
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HSonyu), nameof(HSonyu.LoopProc))]
        public static IEnumerable<CodeInstruction> HSonyuLoopProcTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var part = -1;
            var counter = 0;
            var done = false;
            var hand = AccessTools.Field(typeof(HSonyu), "hand");
            var isKiss = AccessTools.Method(typeof(PatchLoop), nameof(Play70Voice));
            var upThere = AccessTools.Field(typeof(PatchLoop), nameof(FemaleUpThere));
            foreach (var code in instructions)
            {
                if (!done)
                {
                    if (part == -1)
                    {
                        if (code.opcode == OpCodes.Ldc_R4 && code.operand is float number)
                        {
                            if (number == 100f)
                            {
                                yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PatchLoop), nameof(FemaleCeiling)));
                                continue;
                            }
                            else if (number == 70f)
                            {
                                counter++;
                                if (counter == 2)
                                {
                                    // Second 70 number is male, we..
                                    code.operand = 80f;
                                    counter = 0;
                                    part++;
                                }
                                else
                                {
                                    yield return new CodeInstruction(OpCodes.Ldsfld, upThere);
                                    continue;
                                }
                            }
                        }
                    }
                    else if (part < 2)
                    {
                        if (counter == 0)
                        {
                            if (code.opcode == OpCodes.Ldc_R4 && code.operand is float number
                            && number == 70f)
                            {
                                counter++;
                                if (part == 0)
                                {
                                    yield return new CodeInstruction(OpCodes.Ldsfld, upThere);
                                    continue;
                                }
                                else
                                {
                                    code.operand = 80f;
                                }
                            }
                        }
                        else if (counter == 1)
                        {
                            counter++;
                        }
                        else if (counter == 2)
                        {
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
                            counter++;
                        }
                        else if (counter == 1 && code.opcode == OpCodes.Br)
                        {
                            counter++;
                        }
                        else if (counter == 2)
                        {
                            if (code.opcode == OpCodes.Ldarg_0)
                            {
                                code.opcode = OpCodes.Ldc_I4_0;
                            }
                            else if (code.opcode == OpCodes.Ldfld)
                            {
                                yield return new CodeInstruction(OpCodes.Nop);
                                continue;
                            }
                            else if (code.opcode == OpCodes.Brtrue)
                            {
                                counter++;
                            }

                        }
                        else if (counter == 3)
                        {
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
        /// <summary>
        /// Patch auto put-a-condom away.
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HSonyu), nameof(HSonyu.Proc))]
        public static IEnumerable<CodeInstruction> HSonyuLoopTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var counter = 0;
            var done = false;
            foreach (var code in instructions)
            {
                if (!done)
                {
                    if (counter == 0)
                    {
                        if (code.opcode == OpCodes.Ldfld && code.operand is FieldInfo info
                            && info.Name.Equals("isCondom"))
                        {
                            counter++;
                        }
                    }
                    else
                    {
                        if (code.opcode == OpCodes.Ldc_I4_1)
                        {
                            code.opcode = OpCodes.Ldc_I4_0;
                        }
                        else if (code.opcode == OpCodes.Callvirt && code.operand is MethodInfo info
                            && info.Name.Equals("SetCondom"))
                        {
                            done = true;
                        }
                    }
                }
                yield return code;
            }
        }

        /// <summary>
        /// We introduce custom borders for voice play.
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HAibu), nameof(HAibu.Proc))]
        public static IEnumerable<CodeInstruction> HAibuProcTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var ceiling = AccessTools.Field(typeof(PatchLoop), "FemaleCeiling");
            var upThere = AccessTools.Field(typeof(PatchLoop), nameof(FemaleUpThere));
            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldc_R4 
                    && code.operand is float number)
                {
                    if (number == 100f)
                    {
                        yield return new CodeInstruction(OpCodes.Ldsfld, ceiling);
                        continue;
                    }
                    else if (number == 70f)
                    {
                        yield return new CodeInstruction(OpCodes.Ldsfld, upThere);
                        continue;
                    }
                }
                yield return code;
            }
        }
    }
}
