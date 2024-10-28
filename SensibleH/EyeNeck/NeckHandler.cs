using ADV.Commands.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KK_SensibleH
{
    internal class NeckHandler : MonoBehaviour
    {
        internal static Dictionary<ChaControl, Transform> Targets => _targets;

        private static readonly Dictionary<ChaControl, Transform> _targets = [];

        private Transform _target;
        private Transform _root;
        private Transform _aim;
        private Quaternion _rootTargetRot;
        private float _moveStep;
        private float _lerp;
        private float _waitTimestamp;

        /// <summary>
        /// 1 for one second, 0.5 for two, etc.
        /// </summary>
        private float _moveSpeed;
        private float _driftSpeed;

        public float _defRange;
        public float _floor;
        public float _ceiling;
        public float _hLimit;
        public float _vLimit;

        private State _state;
        private ChaControl _chara;

        private float _waitCoef;
        private bool IsWait() => Time.time > _waitTimestamp;
        private float GetWait(int seconds) => Time.time + seconds * _waitCoef;
        enum State
        {
            Stay,
            Move,
            Drift,
            Aim
        }

        private void Awake()
        {
            _chara = GetComponentInParent<ChaControl>();
            _root = new GameObject(_chara.fileParam.firstname + "'s_NeckTargetP").transform;
            _root.SetParent(_chara.objBodyBone.transform.Find("cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_s_spine03"), false);
            _aim = new GameObject("Point").transform;
            _aim.SetParent(_root, false);
            _aim.localPosition = Vector3.forward;
            _moveSpeed = 0.5f;
        }


        private void Update()
        {
            switch (_state)
            {
                case State.Stay:
                    if (!IsWait())
                    {
                        if (Random.value < 0.5f)
                        {
                            StartMove();
                        }
                        else
                        {
                            StartDrift();
                        }
                    }
                    break;
                case State.Drift:
                    Drift();
                    break;
                case State.Move:
                    Move();
                    break;
                case State.Aim:
                    Aim();
                    break;

            }

        }

        internal void UpdateLimits()
        {

            _defRange = 60f;
            _floor = 0f - _defRange * 0.5f;
            _ceiling = _defRange * 0.5f;
        }

        /// <summary>
        /// Follow transform.
        /// </summary>
        private void Follow()
        {

        }

        // At aim start measure angle to the head, and place ceiling here. Remove afterwards.

        private class SmoothDamp
        {
            internal SmoothDamp(float smoothTime)
            {
                _smoothTime = smoothTime;
            }
            private float _current;
            private float _currentVelocity;
            private readonly float _smoothTime;
            
            internal float Damp(bool increase)
            {
                _current = Mathf.SmoothDamp(_current, increase ? 1f : 0f, ref _currentVelocity, _smoothTime);
                return _current;
            }
        }

        private SmoothDamp _smoothDamp;

        private void StartAim()
        {
            _smoothDamp = new(1f);
        }


        private void Aim()
        {
            var lookRot = Quaternion.LookRotation(_target.position - _root.position);
            //if (Vector3.Angle(_target.position - _root.position, _root.parent.forward) < _ceiling)
            if (Quaternion.Angle(lookRot, _root.parent.rotation) < _ceiling)
            {
                _root.rotation = Quaternion.RotateTowards(
                    _root.rotation, 
                    lookRot, 
                    Time.deltaTime * _smoothDamp.Damp(Vector3.Angle(_target.position - _root.position, _aim.position - _root.position) > 10f));
            }
            else
            {
                var rotLimit = 30f - Quaternion.Angle(_root.rotation, _root.parent.rotation);
                if (rotLimit > 0.1f)
                {
                    _rootTargetRot = Quaternion.RotateTowards(_root.rotation, lookRot, rotLimit);
                }
                else
                {
                    // Timestamp and remove aim after.
                }
            }
        }

        /// <summary>
        /// Picks ~different rotation offset.
        /// </summary>
        private void SetRootRotationOffset()
        {
            _rootTargetRot =  Quaternion.Euler(
                RepeatEx(Mathf.DeltaAngle(0f, _root.localEulerAngles.x) + Random.Range(_defRange * 0.3f, _defRange * 0.6f)) * _vLimit,
                RepeatEx(Mathf.DeltaAngle(0f, _root.localEulerAngles.y) + Random.Range(_defRange * 0.3f, _defRange * 0.6f)) * _hLimit,
                0f);
        }

        private float RepeatEx(float number)
        {
            // If we pass the limit, we roll random to negate further progress.
            if (number > _ceiling)
            {
                Debug.Log($"{number} -> {_floor + (number - _ceiling)}");
                return _floor + (number - _ceiling) * Random.value;
            }
            if (number < _floor)
            {
                Debug.Log($"{number} -> {_ceiling + (number - _floor)}");
                return _ceiling + (number - _floor) * Random.value;
            }
            return number;
        }
        private void StartMove()
        {
            SensibleH.Logger.LogDebug($"StartMove:{_moveSpeed}");
            _lerp = 0f;
        }
        /// <summary>
        /// Move from rotation a to rotation b.
        /// </summary>
        private void Move()
        {
            _lerp += Time.deltaTime * _moveSpeed;
            _root.localRotation = Quaternion.Lerp(_root.localRotation, _rootTargetRot, Mathf.SmoothStep(0f, 1f, _lerp));
            if (_lerp > 1f)
            {
                Stay();
            }
        }
        private void Stay()
        {

            _state = State.Stay;
            _waitTimestamp = GetWait(Random.Range(2, 5));
        }
        private void StartDrift()
        {
            _state = State.Drift;
            _lerp = 0f;
            _driftSpeed = Mathf.Max(0.2f, _moveSpeed * Random.Range(0.25f, 0.5f));
            SensibleH.Logger.LogDebug($"StartDrift:{_driftSpeed}");
            _rootTargetRot = _root.localRotation * Quaternion.Euler(Random.Range(-4f, 4f), Random.Range(-4f, 4f), 0f);
        }
        /// <summary>
        /// Move considerably slower between a and b rotations. Multiple iterations.
        /// </summary>
        private void Drift()
        {
            _lerp += Time.deltaTime * _driftSpeed;
            _root.localRotation = Quaternion.Lerp(_root.localRotation, _rootTargetRot, Mathf.SmoothStep(0f, 1f, _lerp));
            if (_lerp > 1f)
            {
                if (Random.value > 0.5f)
                {
                    Stay();
                }
                else
                {
                    StartDrift();
                }
            }
        }


        /*
         * Apply all the new knowledge about orientations, and make simple and pretty eye/neck controllers(different)
         * And synergize them with LookAtIK.
         */
    }
}
