using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KK_SensibleH
{
    internal class BPHelper
    {
        private class ColliderInfo
        {
            internal ColliderInfo(string _name, float _m_Radius, float _m_Height, DynamicBoneCollider.Direction _m_Direction, Vector3 _m_Center)
            {
                name = _name;
                m_Radius = _m_Radius;
                m_Height = _m_Height;
                m_Direction = _m_Direction;
                m_Center = _m_Center;
            }
            internal readonly string name;
            internal readonly float m_Radius;
            internal readonly float m_Height;
            internal readonly DynamicBoneCollider.Direction m_Direction;
            internal readonly Vector3 m_Center;
        }

        private static readonly List<List<ColliderInfo>> _colliderInfoList =
        [
            [   // Hand
                new ColliderInfo("cf_j_index03", 0.005f, 0.03f, default, default),
                new ColliderInfo("cf_j_middle03", 0.006f, 0.03f, default, default),
                new ColliderInfo("cf_j_ring03", 0.006f, 0.02f, default, default)
            ],
            [   // Finger
                new ColliderInfo("cf_j_index02", 0.006f, 0.06f, default, default),
                new ColliderInfo("cf_j_middle02", 0.006f, 0.03f, default, new Vector3(-0.01f, 0f, 0f))
            ],
            [   // Tongue
                new ColliderInfo("cf_j_tang_04", 0.006f, 0.02f, DynamicBoneCollider.Direction.Z, new Vector3(0f, 0f, 0.015f))
            ],
            [   // Massager
                new ColliderInfo("J_massajiki_L_head_00", 0.032f, 0.04f, DynamicBoneCollider.Direction.Y, new Vector3(0f, 0.02f, 0f))
            ],
            [   // Vibe
                new ColliderInfo("J_vibe_01", 0.018f, 0.05f, DynamicBoneCollider.Direction.Y, default),
                new ColliderInfo("J_vibe_04", 0.018f, 0.05f, DynamicBoneCollider.Direction.Y, default),
                new ColliderInfo("J_vibe_05", 0.02f, 0f, default, new Vector3(0f, 0.015f, 0f))
            ],
        ];
        internal static BPHelper Instance => _instance;
        private static BPHelper _instance;
        private readonly Transform _dbRoot;
        private readonly HandCtrl _handCtrl;
        private readonly List<DynamicBoneCollider> _presentDbc = [];

        // New H Scene = new instance.
        internal BPHelper(ChaControl chara, HandCtrl handCtrl)
        {
            _instance = this;
            _dbRoot = chara.transform.Find("BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/cf_s_waist02/cf_J_Vagina_root");
            _handCtrl = handCtrl;
        }

        // Clean up added colliders.
        internal void OnPositionChange()
        {
            if (_dbRoot != null && _presentDbc.Count > 0)
            {
                foreach (var db in _dbRoot.GetComponents<DynamicBone>())
                {
                    foreach (var dbc in _presentDbc)
                    {
                        db.m_Colliders.Remove(dbc);
                    }
                }
            }
        }

        // Add colliders and link them to dynamic bones.
        internal void OnItemAttach(int area)
        {
            if (_dbRoot == null || _handCtrl == null) return;

            if (area == 2 || area == 3)
            {
                var children = _handCtrl.useAreaItems[area].obj.GetComponentsInChildren<Transform>();
                var itemIndex = _handCtrl.areaItem[area];
                var list = new List<DynamicBoneCollider>();

                // KKS not added yet.
                if (_colliderInfoList.Count > itemIndex)
                {
                    foreach (var entry in _colliderInfoList[itemIndex])
                    {
                        var gameObj = children.Where(t => t.name.StartsWith(entry.name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (gameObj == null) continue;

                        var dbc = gameObj.GetComponent<DynamicBoneCollider>();
                        if (dbc == null)
                        {
                            dbc = gameObj.gameObject.AddComponent<DynamicBoneCollider>();
                            dbc.m_Radius = entry.m_Radius;
                            dbc.m_Height = entry.m_Height;
                            dbc.m_Direction = entry.m_Direction;
                            dbc.m_Center = entry.m_Center;
                        }
                        list.Add(dbc);
                    }
                    AddToList(list);
                }
                
            }
        }

        private void AddToList(IEnumerable<DynamicBoneCollider> colliders)
        {

            foreach (var collider in colliders)
            {
                if (!_presentDbc.Contains(collider))
                {
                    _presentDbc.Add(collider);
                }
            }
            foreach (var db in _dbRoot.GetComponents<DynamicBone>())
            {
                foreach (var collider in colliders)
                {
                    if (!db.m_Colliders.Contains(collider))
                    {
                        db.m_Colliders.Add(collider);
                    }
                }
            }
        }
    }
}
