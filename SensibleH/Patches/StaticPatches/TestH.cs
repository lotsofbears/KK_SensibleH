using HarmonyLib;
using KK_SensibleH.AutoMode;
using KK_VR;
using Manager;
using NodeCanvas.Tasks.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using VRGIN.Core;
using static Illusion.Utils;

namespace KK_SensibleH.Patches.StaticPatches
{
    internal class TestH
    {
        public static float size = 1f;
        /// <summary>
        /// Adjustments for non-standard(small) dick diameters in houshi.
        /// </summary>
        [HarmonyPrefix, HarmonyPatch(typeof(FaceListCtrl), nameof(FaceListCtrl.SetFace))]
        public static void SetFacePrefix(int _idFace, int _voiceKind, int _action, FaceListCtrl __instance)
        {
            var dic = __instance.facelib[_voiceKind][_action][_idFace];
            if (SensibleH.hFlag != null && SensibleH.hFlag.mode == HFlag.EMode.houshi && dic.openMinMouth == 1f && (dic.mouth == 22 || dic.mouth == 21))
            {
                dic.openMinMouth = size;
            }
        }

        public static int GetRandomBinary() => UnityEngine.Random.value > 0.5f ? 1 : 0;
        /// <summary>
        /// We substitute rigid set of targets to play voices with random one.
        /// </summary>
        [HarmonyTranspiler, HarmonyPatch(typeof(HLesbian), nameof(HLesbian.Proc))]
        public static IEnumerable<CodeInstruction> HLesbianProcTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var counter = 0;
            var operand = AccessTools.FirstMethod(typeof(TestH), m => m.Name.Equals(nameof(GetRandomBinary)));
            foreach (var code in instructions)
            {
                if (counter == 0)
                {
                    if (code.opcode == OpCodes.Ldfld
                    && code.operand is FieldInfo info && info.Name.Equals("playVoices"))
                    {
                        counter++;
                    }
                }
                else
                {
                    counter = 0;
                    if (code.opcode == OpCodes.Ldc_I4_1 || code.opcode == OpCodes.Ldc_I4_0)
                    {
                        yield return new CodeInstruction(OpCodes.Call, operand);
                        continue;
                    }
                }
                yield return code;
            }
        }
        [HarmonyTranspiler, HarmonyPatch(typeof(HLesbian), nameof(HLesbian.MotionChange))]
        public static IEnumerable<CodeInstruction> HLesbianMotionChangeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var found = false;
            var done = false;
            foreach (var code in instructions)
            {
                if (!done)
                {
                    if (!found)
                    {
                        if (code.opcode == OpCodes.Callvirt
                        && code.operand is MethodInfo info && info.Name.Equals("set_enabled"))
                        {
                            found = true;
                        }
                    }
                    else
                    {
                        if (code.opcode == OpCodes.Ldc_I4_0 || code.opcode == OpCodes.Ldc_I4_3)
                        {
                            yield return new CodeInstruction(OpCodes.Nop);
                            continue;
                        }
                        if (code.opcode == OpCodes.Call)
                        {
                            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                            done = true;
                            continue;
                        }
                    }
                }
                yield return code;
            }
        }
        /// <summary>
        /// Used as trigger to look at particular item.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HandCtrl), nameof(HandCtrl.SetItem))]
        public static void HandCtrlSetItemPostfix(int _arrayArea)
        {
            if (_arrayArea < 3 && SensibleH.EyeNeckControl.Value && UnityEngine.Random.value < 0.67f)
            {
                SensibleHController.Instance.OnTouch(_arrayArea);
            }
        }
#if KK
        /// <summary>
        /// A clutch to synchronize cloth states between action/talk scenes after H. Ugly but can't seem to find a culprit otherwise.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TalkScene), nameof(TalkScene.Start), MethodType.Enumerator)]
        public static void TalkSceneStartPrefix() //(TalkScene __instance)
        {
            // Coroutine yields few times before cloning a chara, we grab the original and wait for a clone.
            // For some reason the clone comes with cloth states from a previous H Scene. We overwrite them with original states.
            // No clue where it hides those states, all the SaveData and ChaFiles are actual. 

            TalkSceneClothesState();
        }
        public static ChaControl _originalChara;
        public static TalkScene _talkScene;
        public static void TalkSceneClothesState()
        {
            if (_talkScene == null)
            {
                _talkScene = Component.FindObjectOfType<TalkScene>();
                _originalChara = null;
            }
            if (_talkScene != null && _talkScene.targetHeroine != null && _talkScene.targetHeroine.chaCtrl != null)
            {
                if (_originalChara == null)
                {
                    _originalChara = _talkScene.targetHeroine.chaCtrl;
                }
                else if (_originalChara != _talkScene.targetHeroine.chaCtrl)
                {
                    var target = _talkScene.targetHeroine.chaCtrl.fileStatus.clothesState;
                    var precursor = _originalChara.fileStatus.clothesState;
                    for (var i = 0; i < precursor.Length; i++)
                    {
                        target[i] = precursor[i];
                    }
                    _originalChara = null;
                }
            }
        }
#endif

        
    }
}
