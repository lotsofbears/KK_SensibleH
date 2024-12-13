using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static KK_SensibleH.SensibleH;
using static KK_SensibleH.Caress.Kiss;
using VRGIN.Core;
using KKAPI;
using KKAPI.MainGame;

namespace KK_SensibleH.Caress
{
    /// <summary>
    /// CyuVR's ito (thread) part.
    /// </summary>
    internal class Thread : MonoBehaviour
    {
        public static Thread Instance;
        private bool itoOn;
        private bool IsItoBreaking;
        private bool _exited;
        private float itoTimer;
        private float siruAmount;
        private bool _timeToGo;
        private bool _tongueOut;
        private const float itoRemainTime = 10f;
        private const int itoMatIndex = 1;
        private const int siruMatIndex = 6;
        private const float distanceReduction = 100f;
        private const float itoDistance = 0.025f;
        private const float itoBreakDistance = 0.07f;
        private LineRenderer ito;
        private ParticleSystem particleSystem;
        private ParticleSystemRenderer particleSystemRenderer;
        private List<Material> orgMaterial = new List<Material>();
        private List<Vector3> posList = new List<Vector3>();
        private List<Vector3>[] posListDelays = new List<Vector3>[5];

        private Transform head;
        private Transform top;
        private Transform tail;

        private Transform _attachment;
        private HandCtrl.AibuColliderKind _actionType;
        //private Transform _intermediateParent;

        private ChaControl _chara;
        //public Transform tongueTip;
        //private Transform siruTarget;
        //public SkinnedMeshRenderer tangRenderer;

        internal bool IsItoActive => siruAmount != 0f;
        /// <summary>
        /// As different models of VR headsets use different ranges for focal point, there are some differences in visual interpretation.
        /// Thus chosen ranges might not always optimal. (They seemed fine for "Pico 4")
        /// </summary>
        private Vector3 GetHeadsetPosition => VR.Camera.transform.TransformPoint(new Vector3(0f, -0.05f, 0.05f));
        //private Vector3 GetIntermediateParentPosition => _intermediateParent.position;
        //private Vector3 GetAttachmentPosition => _attachment.position;
        private Vector3 GetAttachmentPosition => _actionType == HandCtrl.AibuColliderKind.mouth ? GetMouthAttachmentPoint : _attachment.position;
        private Vector3 GetMouthAttachmentPoint => _tongueOut ? _attachment.position : _attachment.TransformPoint(new Vector3(0f, 0.0f, 0.01f));
        private void Awake()
        {
            //SensibleH.Logger.LogDebug($"Thread[Awake]");
            Instance = this;
            _chara = _chaControl[0];
            //_mouthAcc = _female.objHeadBone.transform.Find("cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceLow_tz/a_n_mouth");
            //_attachment = _chara.objHeadBone.transform.Find("cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceLow_tz/cf_J_MouthBase_ty/cf_J_MouthBase_rx/cf_J_MouthCavity");
            particleSystem = new GameObject("BeroItoEffect").AddComponent<ParticleSystem>();
            particleSystemRenderer = particleSystem.GetOrAddComponent<ParticleSystemRenderer>();
            particleSystemRenderer.renderMode = ParticleSystemRenderMode.Billboard;
 
            ((IEnumerable<ParticleSystem>)FindObjectsOfType<ParticleSystem>())
                .Where<ParticleSystem>(x => x.name.IndexOf("LiquidSiru") >= 0)
                .FirstOrDefault<ParticleSystem>()
                .GetComponentsInChildren<ParticleSystemRenderer>()
                .ToList<ParticleSystemRenderer>()
                .ForEach(x => orgMaterial
                .Add(x.material));
            ((IEnumerable<ParticleSystem>)FindObjectsOfType<ParticleSystem>())
                .Where<ParticleSystem>(x => x.name.IndexOf("LiquidSio") >= 0)
                .FirstOrDefault<ParticleSystem>()
                .GetComponentsInChildren<ParticleSystemRenderer>()
                .ToList<ParticleSystemRenderer>()
                .ForEach(x => orgMaterial
                .Add(x.material));
            ((IEnumerable<ParticleSystem>)FindObjectsOfType<ParticleSystem>())
                .Where<ParticleSystem>(x => x.name.IndexOf("LiquidToilet") >= 0)
                .FirstOrDefault<ParticleSystem>()
                .GetComponentsInChildren<ParticleSystemRenderer>()
                .ToList<ParticleSystemRenderer>()
                .ForEach(x => orgMaterial
                .Add(x.material));

            var main = particleSystem.main;
            //main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.07f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.15f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 4f);
            main.startColor = new Color(1f, 1f, 1f, 1f);

            // How in.. this thing works with turned of emission.
            var em = particleSystem.emission;
            em.rateOverTime = 0f;// 2f * GetVoiceValue();

            top = new GameObject("ItoTop").transform;
            tail = new GameObject("ItoTail").transform;
            head = new GameObject("ItoHead").transform;
            top.gameObject.name = "ItoTop";
            tail.gameObject.name = "ItoTail";
            head.gameObject.name = "ItoHead";
            //tail.position = _female.objHead.transform.TransformPoint(0.0f, 0.0f, 1f);

            // TODO Play not only with amounth of threads but localScale too.
            head.transform.localScale = Vector3.one * 0.005f;
            tail.transform.localScale = Vector3.one * 0.03f;
            ito = new GameObject("BeroIto").AddComponent<LineRenderer>();
            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 10f;
            shape.radius = 0.5f;
            ito.numCapVertices = 40;
            ito.numCornerVertices = 32;
            ito.enabled = true;
            ito.useWorldSpace = true;
            ito.startWidth = 0.05f;//0.005f;
            ito.endWidth = 0.05f;//0.005f;

            ito.material = orgMaterial[itoMatIndex];
            particleSystemRenderer.material = orgMaterial[siruMatIndex];
        }

        internal void UpdateAttachmentPoint(HandCtrl.AibuColliderKind colliderKind, bool tongueOut)
        {
            if (AttachmentPoints.ContainsKey(colliderKind) && tongueOut)
            {
                //SensibleH.Logger.LogDebug($"UpdateAttachmentPoint[{colliderKind}]");
                var path = AttachmentPoints[colliderKind];
                _attachment = _chara.objBodyBone.transform.Find(path);
                //SensibleH.Logger.LogDebug($"UpdateAttachmentPoint[pathFound]");
                _actionType = colliderKind;
                _tongueOut = tongueOut;
                //if (colliderKind != HandCtrl.AibuColliderKind.mouth)
                //{
                //    _intermediateParent = _handCtrl.useItems[2].obj.transform.Find("cf_j_tangroot");
                //}
                //else
                //    _intermediateParent = null;
            }

        }
        private void StartIto()
        {
            itoOn = true;
            ito.enabled = true;
        }

        private void BreakIto()
        {
            if (IsItoBreaking)
                return;
            IsItoBreaking = true;
            itoTimer = itoRemainTime;
        }

        private float GetVoiceValue()
        {
            // Was broken in original, is useless now. Poor thing.
            return _chara.asVoice.time;
        }

        private void UpdateWidthCurve()
        {
            var sqrDistance = Vector3.SqrMagnitude(head.transform.position - tail.transform.position);
            var distance = sqrDistance * 0.5f;
            var animationCurve = new AnimationCurve();
            var num3 = 0f;
            while (num3 < 1f)
            {
                var num4 = Mathf.Lerp(0f, sqrDistance, num3);
                var num5 = Mathf.Abs(distance - num4);
                if (IsItoBreaking && distance - num4 < 0f)
                {
                    num5 = 0f;
                }
                animationCurve.AddKey(num3, siruAmount * num5 / (sqrDistance * distanceReduction));
                num3 += 0.01f; //0.01f;
            }
            ito.widthCurve = animationCurve;
        }
        
        private void ChangeColor()
        {
            var main = particleSystem.main;
            if (hFlag.gaugeFemale < 70f)
            {
                main.startColor = new Color(1f, 1f, 1f, 1f);
                _exited = false;
            }
            else
            {
                main.startColor = (ParticleSystem.MinMaxGradient)new Color(1f, 0.62f, 0.85f);
                _exited = true;
            }
        }
        //private void ChangeEmission()
        //{
        //    var em = particleSystem.emission;
        //    em.rateOverTime = 2f * GetVoiceValue();
        //    if (_hVoiceCtrl.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice)
        //        _wasVoice = true;
        //    else
        //        _wasVoice = false;
        //}
        //private bool _wasVoice;

        internal void Cull()
        {
            _timeToGo = true;
        }
        private void OnDestroy()
        {
            //SensibleH.Logger.LogDebug($"Thread:OnDestroy");
            if (particleSystem != null) Destroy(particleSystem);
            if (ito != null) Destroy(ito);

            if (top != null) Destroy(top.gameObject);
            if (tail != null) Destroy(tail.gameObject);
            if (head != null) Destroy(head.gameObject);
        }
        private void Update()
        {
            if (_attachment == null || (siruAmount == 0f && (_kissPhase < Phase.InAction || _timeToGo)))
            {
                if (_timeToGo)
                {
                    Kiss.Instance.EndHelper(this);
                }
                return;
            }
            // We can not change parent along the way, will break everything.
            tail.position = GetHeadsetPosition;
            head.position = GetAttachmentPosition;

            if (hFlag.gaugeFemale > 70f)
            {
                if (!_exited)
                    ChangeColor();
            }
            else
            {
                if (_exited)
                    ChangeColor();
            }

            if (!IsItoBreaking)
            {
                var sqrDist = Vector3.SqrMagnitude(tail.transform.position - head.transform.position);
                if (sqrDist < itoDistance)
                {
                    siruAmount += Time.deltaTime * 0.2f;
                    //CyuLoaderVR.Logger.LogDebug($"SiruUpdate[itoGrowing] {siruAmount}");
                }
                else if (sqrDist > itoBreakDistance)
                {
                    //CyuLoaderVR.Logger.LogDebug($"SiruUpdate[timeToBreak]");
                    BreakIto();
                }
                else
                {
                    //CyuLoaderVR.Logger.LogDebug($"SiruUpdate[itoDwindling] {siruAmount}");
                    siruAmount -= Time.deltaTime * 0.2f;
                }

                top.position = Vector3.Lerp(head.position, tail.position, 10f * Time.deltaTime) - new Vector3(0.0f, 0.5f * sqrDist, 0.0f);
                siruAmount = Mathf.Clamp(siruAmount, 0f, 1f);
            }
            else// if (itoBreaking)
            {
                //CyuLoaderVR.Logger.LogDebug($"SiruUpdate[itoBreaking] {siruAmount}");
                siruAmount -= Time.deltaTime * 0.5f;

                itoTimer -= Time.deltaTime;
                int index = posList.Count - 2;
                if (index > 0)
                {
                    tail.position = posList[index];
                    if (itoTimer < 0f || siruAmount <= 0f)
                    {
                        IsItoBreaking = false;
                        siruAmount = 0f;
                        _attachment = null;
                    }
                }
                float y = top.position.y;
                top.position = Vector3.Lerp(head.position, tail.position, 10f * Time.deltaTime);
                top.position = new Vector3(top.position.x, y - Time.deltaTime * 0.3f, top.position.z);

            }
            OnLineDraw();
        }
        private static Vector3 BezierCurve(Vector3 pt1, Vector3 pt2, Vector3 ctrlPt, float t)
        {
            if (t > 1f)
            {
                t = 1f;
            }
            Vector3 result = default(Vector3);
            float num = 1f - t;
            result.x = num * num * pt1.x + 2f * num * t * ctrlPt.x + t * t * pt2.x;
            result.y = num * num * pt1.y + 2f * num * t * ctrlPt.y + t * t * pt2.y;
            result.z = num * num * pt1.z + 2f * num * t * ctrlPt.z + t * t * pt2.z;
            return result;
        }
        
        private void OnLineDraw()
        {
            var frameCount = Time.frameCount % 5;
            if (posListDelays[frameCount] == null)
                posListDelays[frameCount] = new List<Vector3>();

            posList.Clear();
            posListDelays[frameCount].Clear();
            posList.Add(head.position);
            posListDelays[frameCount].Add(head.position);
            float t = 0.0f;
            int index = 1;
            while (t < 1.0)
            {
                t += 0.03f;//0.03f;
                Vector3 a = BezierCurve(head.position, tail.position, top.position, t);
                if (posListDelays.Length == 5 && posListDelays[(Time.frameCount + 1) % 5] != null && posListDelays[(Time.frameCount + 1) % 5].Count > index)
                {
                    Vector3 vector3 = Vector3.Lerp(a, posListDelays[(Time.frameCount + 1) % 5][index], Mathf.Abs(0.5f - t));
                    posList.Add(vector3);
                    posListDelays[frameCount].Add(vector3);
                }
                else
                {
                    posList.Add(a);
                    posListDelays[frameCount].Add(a);
                }
                ++index;
            }
            ito.positionCount = posList.Count;
            ito.SetPositions(posList.ToArray());
            UpdateWidthCurve();
        }
        private readonly Dictionary<HandCtrl.AibuColliderKind, string> AttachmentPoints = new Dictionary<HandCtrl.AibuColliderKind, string>
        {
            {
                HandCtrl.AibuColliderKind.mouth,"cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck/cf_j_head/cf_s_head/" +
                "p_cf_head_bone/cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceLow_tz/cf_J_MouthBase_ty/cf_J_MouthBase_rx/cf_J_MouthCavity"
            },
            {
                 HandCtrl.AibuColliderKind.muneL,"cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_d_bust00/cf_s_bust00_L/cf_d_bust01_L/" +
                "cf_j_bust01_L/cf_d_bust02_L/cf_j_bust02_L/cf_d_bust03_L/cf_j_bust03_L/cf_d_bnip01_L/cf_j_bnip02root_L/cf_s_bnip02_L/cf_j_bnip02_L/cf_s_bnipacc_L"
            },
            {
                HandCtrl.AibuColliderKind.muneR,"cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_d_bust00/cf_s_bust00_R/cf_d_bust01_R/" +
                "cf_j_bust01_R/cf_d_bust02_R/cf_j_bust02_R/cf_d_bust03_R/cf_j_bust03_R/cf_d_bnip01_R/cf_j_bnip02root_R/cf_s_bnip02_R/cf_j_bnip02_R/cf_s_bnipacc_R"
            },
            {
                HandCtrl.AibuColliderKind.kokan,"cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/cf_d_kokan/cf_j_kokan/cf_n_pee"
            },
            {
                HandCtrl.AibuColliderKind.anal,"cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/cf_d_ana/cf_j_ana"
            }
        };
    }
}