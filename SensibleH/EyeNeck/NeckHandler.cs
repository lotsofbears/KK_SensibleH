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
        // BackBackBack again.
        // We duplicate native neck object pair, move them in cohesive fashion and aim at it.
        // Because my previous attempt to do it by native means is a mess and not up to my standards,
        // and now we have lookAtIK to handle too, which really shines when whatever moves the head knows what it does.
        internal static Dictionary<ChaControl, Transform> Targets => _targets;

        // Those will probably be aim objects, we pass them in harmony-prefix for each chara.
        // If there is no object - we don't change neck -> neck script uses default target.
        private static readonly Dictionary<ChaControl, Transform> _targets = [];

        private Transform _target;

        // Root object at ~neck, the only movable part.
        private Transform _root;

        // Aim object at vector.forward of root object, complete slave.
        private Transform _aim;

        // Parent of root object, used as it's default orientation.
        private Transform _shoulders;

        // Desirable rotation, will slowly(depends on method used) assume it.
        private Quaternion _rootTargetRot;

        // Coefficient to move between rotations.
        private float _lerp;

        private ChaControl _chara;

        // Easy switch between many states on update.
        private State _state;
        enum State
        {
            Stay,
            Move,
            Drift,
            Aim
        }

        // Everything is governed by one single timing (minus hooks), that dictates frequency happenings.
        private float _waitTimestamp;

        // Coefficient to scale timings.
        private float _waitCoef;

        private bool IsWait() => _waitTimestamp > Time.time;
        private void SetWait(int seconds) => _waitTimestamp = Time.time + (seconds * _waitCoef);

        private SmoothDamp _smoothDamp;
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


        private void Awake()
        {
            _chara = GetComponentInParent<ChaControl>();
            _root = new GameObject(_chara.fileParam.firstname + "'s_NeckTargetP").transform;
            _root.SetParent(_chara.objBodyBone.transform.Find("cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_s_spine03"), false);
            _aim = new GameObject("Point").transform;
            _aim.SetParent(_root, false);
            _aim.localPosition = Vector3.forward;
            _shoulders = _root.parent;
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



        private void StartAim()
        {
            _smoothDamp = new(1f);
        }


        private void Aim()
        {
            var lookRot = Quaternion.LookRotation(_target.position - _root.position);
            //if (Vector3.Angle(_target.position - _root.position, _root.parent.forward) < _ceiling)

            // Only if current deviation is less then ceiling.
            // We rotateTowards lookRotation with step from SmoothDamp.
            if (Quaternion.Angle(_root.rotation, _shoulders.rotation) < _ceiling)
            //if (Quaternion.Angle(lookRot, _shoulders.rotation) < _ceiling)
            {
                _root.rotation = Quaternion.RotateTowards(
                    _root.rotation,
                    lookRot,
                    Time.deltaTime * _smoothDamp.Damp(Vector3.Angle(_target.position - _root.position, _aim.position - _root.position) > 10f));
            }
            else
            {
                _root.rotation = Quaternion.RotateTowards(
                    _root.rotation,
                    lookRot,
                    _ceiling);
            }
        }

        /// <summary>
        /// Picks ~different rotation offset.
        /// </summary>
        private void SetRootTarget()
        {
            _rootTargetRot =  Quaternion.Euler(
                RepeatEx(Mathf.DeltaAngle(0f, _root.localEulerAngles.x) + Random.Range(_defRange * 0.3f, _defRange * 0.6f)) * _vLimit,
                RepeatEx(Mathf.DeltaAngle(0f, _root.localEulerAngles.y) + Random.Range(_defRange * 0.3f, _defRange * 0.6f)) * _hLimit,
                0f);
        }

        // Like repeat but floor isn't 0.
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
            _state = State.Move;
            _lerp = 0f;
            SensibleH.Logger.LogDebug($"StartMove:{_moveSpeed}");
        }
        /// <summary>
        /// Move from rotation a to rotation b.
        /// </summary>
        private void Move()
        {
            _lerp += Time.deltaTime * _moveSpeed;
            _root.localRotation = Quaternion.Lerp(_root.localRotation, _rootTargetRot, Mathf.SmoothStep(0f, 1f, _lerp));
            if (_lerp >= 1f)
            {
                Stay();
            }
        }
        private void Stay()
        {
            _state = State.Stay;
            SetWait(Random.Range(2, 5));
        }
        private void StartDrift()
        {
            _state = State.Drift;
            _lerp = 0f;
            _driftSpeed = Mathf.Max(0.2f, _moveSpeed * Random.Range(0.25f, 0.5f));
            _rootTargetRot = _root.localRotation * Quaternion.Euler(Random.Range(-4f, 4f), Random.Range(-4f, 4f), 0f);
            SensibleH.Logger.LogDebug($"StartDrift:{_driftSpeed}");
        }
        /// <summary>
        /// Move considerably slower between rotations. Possibly in consecutive fashion.
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
    }
}
