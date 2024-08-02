using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using VRGIN.Core;
using Random = UnityEngine.Random;
using static KK_SensibleH.SensibleH;
using KK_SensibleH.Patches.DynamicPatches;
using static KK_SensibleH.MoMiController;

namespace KK_SensibleH.Caress
{
    /// <summary>
    /// Stripped down version of CyuVR, ported to MainGameVR, severely decreased overhead at the price of minor aesthetics.
    /// Reasonably optimized.
    /// </summary>
    public class Kiss : MonoBehaviour
    {
        public static Kiss Instance;
        internal static float _eyesOpenness;
        //private const float ExitKissDistance = 0.16f;
        //private float tangSpeed = 35f;
        private float curMouthValue = 0f;//100f;
        private float toMouthValue = 95f;
        private float toEyeValue = KissEyesLimit.Value;
        private float toKissValue = 75f;
        private float curEyeValue = 100f;
        private float curKissValue;
        private float eyesOpenValue = 100f;
        private float mouthSpeed = 1f;
        private float npWeight = 0.33f;
        private float npWeightTo = 0.33f;
        private float npWeight2 = 0.33f;
        private float npWeight2To = 0.33f;
        private float npWeight3 = 0.33f;
        private float npWeight3To = 0.33f;
        private float nnKutiOpenWeight = 0.5f;
        private float nnKutiOpenWeightTo = 0.5f;
        private float eyeOpenSpeed = 1f;
        private float eyeOpenTime = 1f;
        private float npWeightTime = 1f;
        private float npWeightTime2 = 1f;
        private float npWeightTime3 = 1f;
        private float nnKutiOpenWeightTime = 1f;
        private float tangTime = 1f;
        private float mouthOpenTime = 1f;
        private float npWeightSpeed;
        private float npWeightSpeed2;
        private float npWeightSpeed3;
        internal float dragSpeed = 0.001f;

        private List<BlendValue> bvs = new List<BlendValue>();
        private List<BlendValue> bvsShow = new List<BlendValue>();
        private Vector3 tangBonePos = Vector3.zero;
        private Quaternion tangBoneRot = Quaternion.identity;
        private Vector3 tangBonePosTarget = Vector3.zero;
        private Quaternion tangBoneRotTarget = Quaternion.identity;
        private Vector3 tangBoneRotSpeed = Vector3.zero;
        private Vector3 tangBoneTime = Vector3.zero;
        private Vector3 tangBonePosSpeed = Vector3.one;
        private ChaControl _female;
        public bool kissAction;
        internal static Phase _kissPhase;
        //public GameObject maleTang;
        //public GameObject camera;
        //public GameObject kissNeckTarget;
        //private GameObject tang;
        private SkinnedMeshRenderer tangRenderer;
        private Thread _thread;
        private Transform _headset;
        private Vector3 initTangBonePos;
        private Quaternion initTangBoneRot;
        private Harmony _activePatch;
        private Coroutine _beroKissCo;
        internal static bool _frenchKiss;
        private class BlendValue
        {
            public float weight = 1f;
            public int index;
            public string name;
            public float value;
            public SkinnedMeshRenderer renderer;
            public bool active;
        }
        private readonly List<string> _blendValuesEndsWith = new List<string>()
        {
            "name02_op",
            "pero_cl",
            "name_op"
        };
        private readonly List<string> _blendValuesEquals = new List<string>()
        {
            "kuti_face.f00_name02_op",
            "kuti_face.f00_name_op",
            "kuti_ha.ha00_name02_op",
            "kuti_ha.ha00_name_op",
        };
        private readonly string _blendValueStartsWith = "kuti";
        public enum FrenchType
        {
            Disabled,
            Auto,
            Always
        }

        internal enum Phase
        {
            None,
            Engaging,
            InAction,
            Disengaging
        }
        //public bool IsKiss { get; private set; }


        private void Awake()
        {
            Instance = this;
            _female = _chaControl[0];
            ReloadBlendValues();
            var tongue = _female.transform.GetComponentsInChildren<Transform>().ToList()
                .Where(t => t.name == "o_tang")
                .Select(t => t.gameObject)
                .FirstOrDefault();
            _thread = this.gameObject.AddComponent<Thread>();
            tangRenderer = tongue.GetComponent<SkinnedMeshRenderer>(); // = (siru.tangRenderer
            initTangBonePos = tangRenderer.bones[0].localPosition;
            initTangBoneRot = tangRenderer.bones[0].localRotation;
        }
        private void ReloadBlendValues()
        {
            var componentsInChildren = _female.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            bvs.Clear();
            bvsShow.Clear();
            foreach (var skinnedMeshRenderer in componentsInChildren)
            {
                //SensibleH.Logger.LogDebug($"Cyu[ReloadBlendValues][AllMeshes] {skinnedMeshRenderer.name}");
                var blendShapeCount = skinnedMeshRenderer.sharedMesh.blendShapeCount;
                for (int j = 0; j < blendShapeCount; j++)
                {
                    var name = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(j);
                    if (_blendValuesEndsWith.Any(s => name.EndsWith(s, StringComparison.Ordinal))
                        || name.StartsWith(_blendValueStartsWith, StringComparison.Ordinal)) // _blendValuesEquals.Contains(name)

                    {
                        //SensibleH.Logger.LogDebug($"Cyu[ReloadBlendValues][ChosenMesh] {skinnedMeshRenderer.sharedMesh.GetBlendShapeName(j)}");
                        var blendValue = new BlendValue();
                        blendValue.index = j;
                        blendValue.name = name;
                        blendValue.value = skinnedMeshRenderer.GetBlendShapeWeight(j);
                        blendValue.renderer = skinnedMeshRenderer;
                        if (_blendValuesEndsWith.Any(s => name.EndsWith(s, StringComparison.Ordinal)))
                        {
                            blendValue.active = true;
                            blendValue.weight = 0.5f;
                        }
                        bvs.Add(blendValue);
                    }
                }
            }
        }
        public void Cyu()
        {
            if (kissAction && _kissPhase == Phase.Disengaging)
            {
                StopCoroutine(_beroKissCo);
                kissAction = false;
            }
            if (!kissAction)
            {
                if (FrenchKiss.Value == FrenchType.Always || (FrenchKiss.Value == FrenchType.Auto
                    && Random.value < 0.7f && (_hFlag.gaugeFemale > 70f || _hFlag.lstHeroine[0].HExperience > SaveData.Heroine.HExperienceKind.不慣れ)))
                {
                    _frenchKiss = true;
                    SuppressVoice = true;
                }
                else
                    _frenchKiss = false;

                _beroKissCo =  StartCoroutine(BeroKiss());
            }
                
        }
        public void RandomMoveFloat(ref float cur, ref float to, float speed, float min, float max, ref float time, float timeMin = 1f, float timeMax = 5f)
        {
            float num = 0.01f;
            if (cur < to)
            {
                cur = Mathf.SmoothDamp(cur, to, ref speed, time);
                if (cur + num >= to)
                {
                    to = Random.Range(min, max);
                    time = Random.Range(timeMin, timeMax);
                }
                return;
            }
            speed = 0f - speed;
            cur = Mathf.SmoothDamp(cur, to, ref speed, time);
            if (cur - num <= to)
            {
                to = Random.Range(min, max);
                time = Random.Range(timeMin, timeMax);
            }
        }

        public void RandomMoveFloatTest(ref float cur, ref float to, ref float speed, float min, float max, ref float time, float timeMin = 1f, float timeMax = 5f)
        {
            float num = 0.05f;
            if (cur < to)
            {
                cur = Mathf.SmoothDamp(cur, to, ref speed, time, 100f);
                if (cur + num >= to)
                {
                    to = Random.Range(min, max);
                    time = Random.Range(timeMin, timeMax);
                    return;
                }
            }
            else
            {
                cur = Mathf.SmoothDamp(cur, to, ref speed, time, 100f);
                if (cur - num <= to)
                {
                    to = Random.Range(min, max);
                    time = Random.Range(timeMin, timeMax);
                }
            }
        }

        private IEnumerator BeroKiss()
        {
            SensibleH.Logger.LogDebug($"BeroKiss[Start] French[{FrenchKiss.Value}]");
            if (_activePatch == null)
            {
                // Extra check if we don't go through the "Disengage" phase, and start the new kiss immediately.
                _activePatch = Harmony.CreateAndPatchAll(typeof(PatchEyes));
                //SensibleH.Logger.LogDebug($"BeroKiss[Patch]{_activePatch}");
            }
            _female.ChangeEyesBlinkFlag(false);
            //_female.ChangeEyesPtn(0);
            //_female.ChangeEyebrowPtn(0);
            //_female.eyesCtrl.OpenMin = 0f;
            //_female.eyebrowCtrl.OpenMin = 0f;
            kissAction = true;
            _kissPhase = Phase.Engaging;
            
            var changeRate = Random.Range(10f, 20f);
            while (MoMiController._kissCo)
            {
                var frameChange = Time.deltaTime * changeRate;
                curMouthValue = Mathf.Clamp(curMouthValue + (frameChange * 10f), 0f, 100f);
                curKissValue = Mathf.Clamp(curKissValue + (frameChange * 5f), 0f, 100f);
                curEyeValue = Mathf.Clamp(curEyeValue - (frameChange * 3f), 0f, 100f);
                _eyesOpenness = curEyeValue * 0.01f;
                AnimateEyes();
                //SensibleH.Logger.LogDebug($"BeroKiss[Engage]{curMouthValue} - {curKissValue} - {curEyeValue}");
                if (curEyeValue == 0f)
                {
                    break;
                }
                yield return null;
            }
            _kissPhase = Phase.InAction;
            //SensibleH.Logger.LogDebug($"BeroKiss[InAction]{_kissCo}");
            while (MoMiController._kissCo)
            {
                RandomMoveFloatTest(ref npWeight, ref npWeightTo, ref npWeightSpeed, 0f, 1f, ref npWeightTime, 0.1f, 0.5f);
                RandomMoveFloatTest(ref npWeight2, ref npWeight2To, ref npWeightSpeed2, 0f, 1f, ref npWeightTime2, 0.1f, 0.5f);
                RandomMoveFloatTest(ref npWeight3, ref npWeight3To, ref npWeightSpeed3, 0f, 1f, ref npWeightTime3, 0.1f, 0.5f);
                RandomMoveFloat(ref nnKutiOpenWeight, ref nnKutiOpenWeightTo, 0.1f, 0f, 1f, ref nnKutiOpenWeightTime, 1f, 5f);
                RandomMoveFloatTest(ref tangBonePos.x, ref tangBonePosTarget.x, ref tangBonePosSpeed.x, -0.002f, 0.002f, ref tangBoneTime.x, 0.1f, 2f);
                RandomMoveFloatTest(ref tangBonePos.y, ref tangBonePosTarget.y, ref tangBonePosSpeed.y, -0.001f, 0.001f, ref tangBoneTime.y, 0.1f, 2f);
                RandomMoveFloatTest(ref tangBonePos.z, ref tangBonePosTarget.z, ref tangBonePosSpeed.z, -0.002f, 0.002f, ref tangBoneTime.z, 0.1f, 2f);
                RandomMoveFloatTest(ref tangBoneRot.y, ref tangBoneRotTarget.y, ref tangBoneRotSpeed.y, -5f, 5f, ref tangBoneTime.y, 0.1f, 2f);
                RandomMoveFloatTest(ref tangBoneRot.x, ref tangBoneRotTarget.x, ref tangBoneRotSpeed.x, -5f, 2.5f, ref tangBoneTime.x, 0.1f, 2f);
                RandomMoveFloatTest(ref tangBoneRot.z, ref tangBoneRotTarget.z, ref tangBoneRotSpeed.z, -3.5f, 3.5f, ref tangBoneTime.z, 0.1f, 2f);
                RandomMoveFloatTest(ref curMouthValue, ref toMouthValue, ref mouthSpeed, 97f, 100f, ref mouthOpenTime, 10f, 12f);
                RandomMoveFloatTest(ref curEyeValue, ref toEyeValue, ref eyeOpenSpeed, 0f, KissEyesLimit.Value, ref eyeOpenTime, 0.01f, 1.2f);
                _eyesOpenness = curEyeValue * 0.01f;
                RandomMoveFloatTest(ref curKissValue, ref toKissValue, ref changeRate, 25f, 100f, ref tangTime, 0.01f, 0.1f);
                if (KissEyesLimit.Value > 0f)
                {
                    AnimateEyes();
                }
                yield return null;
            }
            _kissPhase = Phase.Disengaging;
            SensibleH.Logger.LogDebug($"BeroKiss[Disengaging]{MoMiController._kissCo}");
            if (!_frenchKiss)
            {
                SuppressVoice = true;
                if (_hVoiceCtrl.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice)
                    Manager.Voice.Instance.Stop(_hFlag.transVoiceMouth[0]);
                _female.ChangeMouthPtn(0);
                // 23 - kiss
            }

            changeRate = Random.Range(15f, 30f); //Mathf.Max(25f, Mathf.Abs(changeRate));
            while (true)
            {
                var frameChange = Time.deltaTime * changeRate;
                curMouthValue = Mathf.Clamp(curMouthValue - (frameChange * 1.5f), 0f, 100f);
                curKissValue = Mathf.Clamp(curKissValue - (frameChange * 1.25f), 0f, 100f);
                curEyeValue = Mathf.Clamp(curEyeValue + (frameChange * (curMouthValue < 25f ? 1f + _eyesOpenness : 1f)), 0f, 100f);
                _eyesOpenness = curEyeValue * 0.01f;
                AnimateEyes();
                if (curEyeValue == 100f)
                {
                    break;
                }
                if (MoMiController._kissCo)
                {
                    // In case there is already next one on the way.
                    kissAction = false;
                    yield break;
                }
                //SensibleH.Logger.LogDebug($"BeroKiss[Disengage]{curMouthValue} - {curKissValue} - {curEyeValue}");
                yield return null;
            }
            if (_frenchKiss)
            {
                _activePatch.UnpatchSelf();
                _activePatch = null;
                _frenchKiss = false;
            }
            SuppressVoice = false;
            curEyeValue = 100f;
            _eyesOpenness = 1f; 
            curMouthValue = 0f;
            //female.eyesCtrl.OpenMax = 1f;
            //female.eyebrowCtrl.OpenMax = 1f;
            _female.ChangeEyesOpenMax(1f);
            _female.ChangeEyebrowOpenMax(1f);

            tangBonePos = Vector3.zero;
            tangBoneRot = Quaternion.identity;
            _kissPhase = Phase.None;
            _hFlag.voice.playVoices[0] = 102;

            _female.ChangeEyesBlinkFlag(true);
            kissAction = false;
            SensibleH.Logger.LogDebug("BeroKiss[End]");
        }
        private void OnDestroy()
        {
            _activePatch?.UnpatchSelf();
        }
        public void SetBlendShapeWeight()
        {
            foreach (var blendValue in bvs)
            {
                if (blendValue.active)
                {
                    float num = 1f;
                    float num2 = 1.5f;
                    if (blendValue.name.EndsWith("name02_op", StringComparison.Ordinal))
                    {
                        num = num2 * npWeight / (npWeight + npWeight2 + npWeight3);
                    }
                    else if (blendValue.name.EndsWith("pero_cl", StringComparison.Ordinal))
                    {
                        num = num2 * npWeight2 / (npWeight + npWeight2 + npWeight3);
                    }
                    else if (blendValue.name.EndsWith("name_op", StringComparison.Ordinal))
                    {
                        num = num2 * npWeight3 / (npWeight + npWeight2 + npWeight3);
                    }
                    blendValue.value = curKissValue * num;
                    if (kissAction && blendValue.name.Equals("kuti_face.f00_name02_op"))
                    {
                        blendValue.value = curMouthValue * (1f - nnKutiOpenWeight);
                    }
                    else if (kissAction && blendValue.name.Equals("kuti_face.f00_name_op"))
                    {
                        blendValue.value = curMouthValue * nnKutiOpenWeight;
                    }
                    else if (kissAction && blendValue.name.Equals("kuti_ha.ha00_name02_op"))
                    {
                        blendValue.value = curMouthValue * (1f - nnKutiOpenWeight);
                    }
                    else if (kissAction && blendValue.name.Equals("kuti_ha.ha00_name_op"))
                    {
                        blendValue.value = curMouthValue * nnKutiOpenWeight;
                    }
                    blendValue.renderer.SetBlendShapeWeight(blendValue.index, blendValue.value);
                }
                else if (blendValue.name.StartsWith("kuti", StringComparison.Ordinal))
                {
                    blendValue.renderer.SetBlendShapeWeight(blendValue.index, 0f);
                }
            }
        }

        public void LateUpdateHook()
        {
            if (kissAction && _frenchKiss)
            {
                // It does indeed wants to be after the late update.
                SetBlendShapeWeight();

                var localPosition = tangRenderer.bones[0].transform.localPosition;
                tangRenderer.bones[0].transform.localPosition = new Vector3(localPosition.x, tangBonePos.y + initTangBonePos.y, localPosition.z);
                tangRenderer.bones[0].transform.localRotation = initTangBoneRot * Quaternion.Euler(tangBoneRot.x, tangBoneRot.y, tangBoneRot.z);
            }
        }
        private void AnimateEyes()
        {
            // We patch "ChangeEye..()", here we only call it (because nobody else wants to).
            _female.ChangeEyesOpenMax(1);
            _female.ChangeEyebrowOpenMax(1);
        }
    }
}
