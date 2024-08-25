using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;
using static KK_SensibleH.SensibleH;
using KK_SensibleH.Patches.DynamicPatches;
using static KK_SensibleH.Caress.MoMiController;
using VRGIN.Core;
using KKAPI;
using KKAPI.MainGame;

namespace KK_SensibleH.Caress
{
    /// <summary>
    /// Stripped down version of CyuVR, ported to MainGameVR, severely decreased overhead at the price of minor aesthetics.
    /// Reasonably optimized.
    /// </summary>
    public class Kiss : MonoBehaviour
    {
        // TODO Bring back mesh baking? aesthetics are quite good.

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

        private bool _proximity;
        public bool kissAction;
        private HandCtrl.AibuColliderKind _actionType;
        private float _lastVoice;

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
        internal static Phase _kissPhase;
        //public GameObject maleTang;
        //public GameObject camera;
        //public GameObject kissNeckTarget;
        //private GameObject tang;
        private SkinnedMeshRenderer tangRenderer;
        private Thread _thread;
        //private Transform _headset;
        private Transform _eyes;
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


        private void Awake()
        {
            SensibleH.Logger.LogDebug($"Kiss[Awake]");
            Instance = this;
            _female = _chaControl[0];
            _eyes = _female.objHeadBone.transform.Find("cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_tz");
            ReloadBlendValues();
            var tongue = _female.transform.GetComponentsInChildren<Transform>().ToList()
                .Where(t => t.name == "o_tang")
                .Select(t => t.gameObject)
                .FirstOrDefault();
            _thread = this.gameObject.AddComponent<Thread>();
            tangRenderer = tongue.GetComponent<SkinnedMeshRenderer>();
            initTangBonePos = tangRenderer.bones[0].localPosition;
            initTangBoneRot = tangRenderer.bones[0].localRotation;
        }
        internal void EndHelper(MonoBehaviour behavior)
        {
            SensibleH.Logger.LogDebug($"Kiss:EndHelper[{behavior}]");
            Destroy(behavior);
        }
        private void CullIto()
        {
            if (_thread.IsItoActive)
            {
                _thread.Cull();
                _thread = this.gameObject.AddComponent<Thread>();
            }
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
                        //SensibleH.Logger.LogDebug($"Cyu[ReloadBlendValues][ChosenMesh] {skinnedMeshRenderer.sharedMesh.GetBlendShapeName(j)} {skinnedMeshRenderer.GetBlendShapeWeight(j)}");
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
        public void Cyu(HandCtrl.AibuColliderKind colliderKind)
        {
            if (kissAction && _kissPhase == Phase.Disengaging)
            {
                StopCoroutine(_beroKissCo);
                kissAction = false;
            }
            if (!kissAction)
            {
                _actionType = colliderKind;
                if (colliderKind == HandCtrl.AibuColliderKind.mouth)
                {
                    _proximity = false;
                    if (FrenchKiss.Value == FrenchType.Always || (FrenchKiss.Value == FrenchType.Auto
                        && Random.value < 0.7f && (_hFlag.gaugeFemale > 70f || _hFlag.lstHeroine[0].HExperience > SaveData.Heroine.HExperienceKind.不慣れ)))
                    {
                        _frenchKiss = true;
                        SuppressVoice = true;
                    }
                    else
                    {
                        _frenchKiss = false;
                    }
                    CullIto();
                    _beroKissCo = StartCoroutine(BeroKiss());

                }
                else
                {
                    CullIto();
                    _proximity = true;
                    //_frenchKiss = true;
                    _beroKissCo = StartCoroutine(BeroLick());
                }
                _thread.UpdateAttachmentPoint(colliderKind, _frenchKiss);
            }
                
        }
        private void RandomMoveFloat(ref float cur, ref float to, float speed, float min, float max, ref float time, float timeMin = 1f, float timeMax = 5f)
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
        public void OnVoiceProc()
        {
            if (_actionType != HandCtrl.AibuColliderKind.mouth && Random.value < 0.75f)
            {
                toEyeValue = 25f + Random.value * 50f;
            }
            _lastVoice = Time.time + 4f + Random.value * 2f;
        }
        private void RandomMoveFloatTest(ref float cur, ref float to, ref float speed, float min, float max, ref float time, float timeMin = 1f, float timeMax = 5f)
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
        private void Update()
        {
            // Because we want coroutine after update but proximity check in update or end of frame.
            // And spawning an extra coroutine just to check proximity looks a bit too ugly.
            if (kissAction && !_proximity && Vector3.Distance(VR.Camera.Head.position, _eyes.position) < 0.12f)
            {
                _proximity = true;
            }

        }
        private IEnumerator BeroKiss()
        {
            SensibleH.Logger.LogDebug($"Kiss:BeroKiss[Start:French = {FrenchKiss.Value}]");
            if (_activePatch == null)
            {
                // Extra check if we don't go through the "Disengage" phase, and start the new kiss immediately.
                _activePatch = Harmony.CreateAndPatchAll(typeof(PatchEyes));
            }
            _female.ChangeEyesBlinkFlag(false);
            //_female.ChangeEyesPtn(0);
            //_female.ChangeEyebrowPtn(0);
            //_female.eyesCtrl.OpenMin = 0f;
            //_female.eyebrowCtrl.OpenMin = 0f;
            kissAction = true;
            _kissPhase = Phase.Engaging;
            
            var changeRate = Random.Range(10f, 20f);
            var initMouthOpenness = 25f + Random.value * 25f;
            while (_handCtrl.IsKissAction())
            {
                var frameChange = Time.deltaTime * changeRate * 3f;
                if (!_proximity)
                {
                    var mouthChange = curMouthValue < initMouthOpenness ? frameChange * 2f : frameChange * 0.5f;
                    curMouthValue = Mathf.Clamp(curMouthValue + mouthChange, 0f, 100f);

                    var kissChange = curKissValue < initMouthOpenness ? frameChange : frameChange * 0.3f;
                    curKissValue = Mathf.Clamp(curKissValue + (kissChange), 0f, 100f);
                }
                else
                {
                    curMouthValue = Mathf.Clamp(curMouthValue + (frameChange * 2f), 0f, 100f);
                    curKissValue = Mathf.Clamp(curKissValue + (frameChange), 0f, 100f);
                }
                curEyeValue = Mathf.Clamp(curEyeValue - (frameChange), 0f, 100f);
                _eyesOpenness = curEyeValue * 0.01f;
                AnimateEyes();
                //SensibleH.Logger.LogDebug($"BeroKiss[Engage]{curMouthValue} - {curKissValue} - {curEyeValue} - {_proximity}");
                if (curKissValue == 100f && curEyeValue == 0f)
                {
                    // Change distance to the setting + extra once migrated to VR.
                    break;
                }
                yield return null;
            }
            _kissPhase = Phase.InAction;
            //SensibleH.Logger.LogDebug($"BeroKiss[InAction]{_kissCo}");
            while (_handCtrl.IsKissAction())
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
            SensibleH.Logger.LogDebug($"BeroKiss[Disengaging]{_handCtrl.IsKissAction()}");
            if (!_frenchKiss)
            {
                SuppressVoice = true;
                if (_hVoiceCtrl.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice)
                {
#if KK
                    Manager.Voice.Instance.Stop(_hFlag.transVoiceMouth[0]);
#else
                    Manager.Voice.Stop(_hFlag.transVoiceMouth[0]);
#endif

                }
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
                if (_handCtrl.IsKissAction())
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
        private IEnumerator BeroLick()
        {
            SensibleH.Logger.LogDebug($"BeroLick[Start]");
            if (_activePatch == null)
            {
                // Extra check if we don't go through the "Disengage" phase, and start the new kiss immediately.
                _activePatch = Harmony.CreateAndPatchAll(typeof(PatchEyes));
                //SensibleH.Logger.LogDebug($"BeroKiss[Patch]{_activePatch}");
            }
            //_female.ChangeEyesBlinkFlag(false);
            //_female.ChangeEyesPtn(0);
            //_female.ChangeEyebrowPtn(0);
            //_female.eyesCtrl.OpenMin = 0f;
            //_female.eyebrowCtrl.OpenMin = 0f;
            kissAction = true;
            _kissPhase = Phase.Engaging;

            var changeRate = Random.Range(10f, 20f);
            var targetOpenness = Random.value * 70f;
            while (_handCtrl.useItems[2] != null)
            {
                var frameChange = Time.deltaTime * changeRate;
                //curMouthValue = Mathf.Clamp(curMouthValue + (frameChange * 10f), 0f, 100f);
                //curKissValue = Mathf.Clamp(curKissValue + (frameChange * 5f), 0f, 100f);
                curEyeValue = Mathf.Clamp(curEyeValue - (frameChange * 3f), targetOpenness, 100f);
                _eyesOpenness = curEyeValue * 0.01f;
                AnimateEyes();
                //SensibleH.Logger.LogDebug($"BeroKiss[Engage]{curMouthValue} - {curKissValue} - {curEyeValue}");
                if (curEyeValue == targetOpenness)
                {
                    break;
                }
                yield return null;
            }
            _kissPhase = Phase.InAction;
            SensibleH.Logger.LogDebug($"BeroKiss[InAction]{_handCtrl.useItems[2] != null}");
            var minValue = Mathf.Abs(-25 + UnityEngine.Random.value * 50f);
            while (_handCtrl.useItems[2] != null)
            {
                //RandomMoveFloatTest(ref npWeight, ref npWeightTo, ref npWeightSpeed, 0f, 1f, ref npWeightTime, 0.1f, 0.5f);
                //RandomMoveFloatTest(ref npWeight2, ref npWeight2To, ref npWeightSpeed2, 0f, 1f, ref npWeightTime2, 0.1f, 0.5f);
                //RandomMoveFloatTest(ref npWeight3, ref npWeight3To, ref npWeightSpeed3, 0f, 1f, ref npWeightTime3, 0.1f, 0.5f);
                //RandomMoveFloat(ref nnKutiOpenWeight, ref nnKutiOpenWeightTo, 0.1f, 0f, 1f, ref nnKutiOpenWeightTime, 1f, 5f);
                //RandomMoveFloatTest(ref tangBonePos.x, ref tangBonePosTarget.x, ref tangBonePosSpeed.x, -0.002f, 0.002f, ref tangBoneTime.x, 0.1f, 2f);
                //RandomMoveFloatTest(ref tangBonePos.y, ref tangBonePosTarget.y, ref tangBonePosSpeed.y, -0.001f, 0.001f, ref tangBoneTime.y, 0.1f, 2f);
                //RandomMoveFloatTest(ref tangBonePos.z, ref tangBonePosTarget.z, ref tangBonePosSpeed.z, -0.002f, 0.002f, ref tangBoneTime.z, 0.1f, 2f);
                //RandomMoveFloatTest(ref tangBoneRot.y, ref tangBoneRotTarget.y, ref tangBoneRotSpeed.y, -5f, 5f, ref tangBoneTime.y, 0.1f, 2f);
                //RandomMoveFloatTest(ref tangBoneRot.x, ref tangBoneRotTarget.x, ref tangBoneRotSpeed.x, -5f, 2.5f, ref tangBoneTime.x, 0.1f, 2f);
                //RandomMoveFloatTest(ref tangBoneRot.z, ref tangBoneRotTarget.z, ref tangBoneRotSpeed.z, -3.5f, 3.5f, ref tangBoneTime.z, 0.1f, 2f);
                if (_lastVoice < Time.time)
                {
                    RandomMoveFloatTest(ref curMouthValue, ref toMouthValue, ref mouthSpeed, 0f, 100f, ref mouthOpenTime, 10f, 12f);
                }
                RandomMoveFloatTest(ref curEyeValue, ref toEyeValue, ref eyeOpenSpeed, minValue, 75f, ref eyeOpenTime, 0.01f, 1.2f);
                _eyesOpenness = curEyeValue * 0.01f;
                //RandomMoveFloatTest(ref curKissValue, ref toKissValue, ref changeRate, 25f, 100f, ref tangTime, 0.01f, 0.1f);
                AnimateEyes();
                
                yield return null;
            }
            _kissPhase = Phase.Disengaging;
            SensibleH.Logger.LogDebug($"BeroKiss[Disengaging]{_handCtrl.IsKissAction()}");
            //if (!_frenchKiss)
            //{
            //    SuppressVoice = true;
            //    if (_hVoiceCtrl.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice)
            //        Manager.Voice.Stop(_hFlag.transVoiceMouth[0]);
            //    _female.ChangeMouthPtn(0);
            //    // 23 - kiss
            //}

            changeRate = Random.Range(15f, 30f); //Mathf.Max(25f, Mathf.Abs(changeRate));
            while (true)
            {
                var frameChange = Time.deltaTime * changeRate;
                //curMouthValue = Mathf.Clamp(curMouthValue - (frameChange * 1.5f), 0f, 100f);
                //curKissValue = Mathf.Clamp(curKissValue - (frameChange * 1.25f), 0f, 100f);
                curEyeValue = Mathf.Clamp(curEyeValue + (frameChange * (1f + _eyesOpenness)), 0f, 100f);
                _eyesOpenness = curEyeValue * 0.01f;
                AnimateEyes();
                if (curEyeValue == 100f)
                {
                    break;
                }
                if (_handCtrl.useItems[2] != null)
                {
                    // In case there is already next one on the way.
                    kissAction = false;
                    yield break;
                }
                //SensibleH.Logger.LogDebug($"BeroKiss[Disengage]{curMouthValue} - {curKissValue} - {curEyeValue}");
                yield return null;
            }
            //if (_frenchKiss)
            //{
            //    _activePatch.UnpatchSelf();
            //    _activePatch = null;
            //    _frenchKiss = false;
            //}
            //SuppressVoice = false;
            curEyeValue = 100f;
            _eyesOpenness = 1f;
            curMouthValue = 0f;
            //female.eyesCtrl.OpenMax = 1f;
            //female.eyebrowCtrl.OpenMax = 1f;
            _female.ChangeEyesOpenMax(1f);
            _female.ChangeEyebrowOpenMax(1f);

            //tangBonePos = Vector3.zero;
            //tangBoneRot = Quaternion.identity;
            _kissPhase = Phase.None;
            //_hFlag.voice.playVoices[0] = 102;

            //_female.ChangeEyesBlinkFlag(true);
            kissAction = false;
            SensibleH.Logger.LogDebug("BeroBero[End]");
        }
        private void OnDestroy()
        {
            _activePatch?.UnpatchSelf();
        }
        private void SetBlendShapeWeight()
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
