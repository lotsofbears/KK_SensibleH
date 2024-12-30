using ADV.Commands.Chara;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static KK_SensibleH.HeadManipulator;
using static KK_SensibleH.EyeNeckControl.NewNeckController;
using Random = UnityEngine.Random;
using KKAPI;
using KKAPI.MainGame;

namespace KK_SensibleH.EyeNeckControl
{
    public struct EyeNeckDictionaries
    {
        public enum DirectionEye
        {
            Cam,
            Away,
            PoiUp,
            PoiDown,
            PoiRollAway,
            UpMid,
            UpRight,
            UpLeft,
            Mid,
            MidRight,
            MidLeft,
            DownMid,
            DownDownMid,
            DownRight,
            DownLeft,
            Pose
        }
        public enum DirectionNeck
        {
            Pose,
            Cam,
            Away,
            UpMid,
            UpRight,
            UpRightFar,
            UpLeft,
            Mid,
            MidRight,
            MidLeft,
            DownMid,
            DownRight,
            DownLeft,
            DownDownLeft
        }

        public static int GetNeckDirection(DirectionNeck direction)
        {
            var id = 0;
            id = NeckDirections
                .Where(kv => kv.Value == direction)
                .FirstOrDefault().Key;
            return id;
        }
        public static DirectionNeck GetNeckDirection(int id)
        {
            var direction = DirectionNeck.Pose;
            if (NeckDirections.ContainsKey(id))
            {
                direction = NeckDirections[id];
            }
            return direction;
        }
        private static Dictionary<int, DirectionNeck> NeckDirections = new Dictionary<int, DirectionNeck>()
        {
            {0, DirectionNeck.Pose},
            {17, DirectionNeck.Cam},
            {34, DirectionNeck.Away},
            {51, DirectionNeck.Cam}, 
            {68,  DirectionNeck.Away},
            {85,  DirectionNeck.Cam}, 
            {102, DirectionNeck.Away},
            {119, DirectionNeck.DownLeft},
            {136, DirectionNeck.UpRightFar},
            {153, DirectionNeck.UpMid},
            {170, DirectionNeck.UpRight},
            {187, DirectionNeck.MidRight}, // _tag 6
            {204, DirectionNeck.DownRight},
            {221, DirectionNeck.DownMid},
            {238, DirectionNeck.DownDownLeft},
            {255, DirectionNeck.MidLeft}, // _tag 10
            {272, DirectionNeck.UpLeft},
            {289, DirectionNeck.Mid}
        };
        public static Dictionary<int, DirectionEye> EyeDirections = new Dictionary<int, DirectionEye>()
        {
            {0, DirectionEye.Cam},
            {1, DirectionEye.Pose},
            {2, DirectionEye.DownDownMid},
            {3, DirectionEye.DownDownMid},
            {4, DirectionEye.UpMid},
            {5, DirectionEye.UpRight},
            {6, DirectionEye.MidRight},
            {7, DirectionEye.DownRight},
            {8, DirectionEye.DownMid},
            {9, DirectionEye.DownLeft},
            {10, DirectionEye.MidLeft},
            {11, DirectionEye.UpLeft},
            {12, DirectionEye.Mid},
            {13, DirectionEye.Away},
            {14, DirectionEye.PoiDown},
            {15, DirectionEye.PoiUp},
            {16, DirectionEye.PoiRollAway}
        };


        public static Vector3 GetAuxCamDic(DirectionEye direction)
        {
            switch (direction)
            {
                case DirectionEye.Pose:
                    return new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f);
                case DirectionEye.DownDownMid:
                    return new Vector3(-0.15f + Random.value * 0.3f, -0.1f - Random.value * 0.2f);
                case DirectionEye.UpMid:
                    return new Vector3(-0.15f + Random.value * 0.3f, 0.1f + Random.value * 0.2f);
                case DirectionEye.UpRight:
                    return new Vector3(-0.1f - Random.value * 0.2f, 0.1f + Random.value * 0.2f);
                case DirectionEye.MidRight:
                    return new Vector3(-0.1f - Random.value * 0.2f, -0.15f + Random.value * 0.3f);
                case DirectionEye.DownRight:
                    return new Vector3(-0.1f - Random.value * 0.2f, -0.1f - Random.value * 0.2f);
                case DirectionEye.DownMid:
                    return new Vector3(-0.15f + Random.value * 0.3f, -0.1f - Random.value * 0.2f);
                case DirectionEye.DownLeft:
                    return new Vector3(0.1f + Random.value * 0.2f, -0.1f - Random.value * 0.2f);
                case DirectionEye.MidLeft:
                    return new Vector3(0.1f + Random.value * 0.2f, -0.15f + Random.value * 0.3f);
                case DirectionEye.UpLeft:
                    return new Vector3(0.1f + Random.value * 0.2f, 0.1f + Random.value * 0.2f);
                case DirectionEye.Mid:
                    return new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f);
                case DirectionEye.Away:
                    return new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f);
                case DirectionEye.PoiDown:
                    return new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f);
                case DirectionEye.PoiUp:
                    return new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f);
                case DirectionEye.PoiRollAway:
                    return new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f);
                default:
                    return Vector3.zero;
            }
        }
        public static Vector3 GetAuxPoiDic(DirectionEye direction)
        {
            // Right side 
            //     Up = -x
            //     Right = +y
            // Left side 
            //     Up = +x
            //     Right = +y
            switch (direction)
            {
                case DirectionEye.Cam:
                    return new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f);
                case DirectionEye.Pose:
                    return new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f);
                case DirectionEye.DownDownMid:
                    return new Vector3(-0.1f - Random.value * 0.2f, -0.15f + Random.value * 0.3f);
                case DirectionEye.UpMid:
                    return new Vector3(0.1f + Random.value * 0.2f, -0.15f + Random.value * 0.3f);
                case DirectionEye.UpRight:
                    return new Vector3(0.1f + Random.value * 0.2f, -0.15f + Random.value * 0.3f);
                case DirectionEye.MidLeft:
                case DirectionEye.MidRight:
                    return new Vector3(-0.15f + Random.value * 0.3f, 0.1f + Random.value * 0.2f);// done
                case DirectionEye.DownRight:
                    return new Vector3(0.1f + Random.value * 0.2f, 0.1f + Random.value * 0.2f); // DONE
                case DirectionEye.DownMid:
                    return new Vector3(-0.1f - Random.value * 0.2f, -0.15f + Random.value * 0.3f);
                case DirectionEye.DownLeft:
                    return new Vector3(-0.1f - Random.value * 0.2f, -0.1f - Random.value * 0.2f);
                case DirectionEye.UpLeft:
                    return new Vector3(0.1f + Random.value * 0.2f, 0.1f + Random.value * 0.2f);
                case DirectionEye.Mid:
                    return new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f);
                case DirectionEye.Away:
                    return new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f);
                case DirectionEye.PoiDown:
                    return new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f);
                case DirectionEye.PoiUp:
                    return new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f);
                case DirectionEye.PoiRollAway:
                    return new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f);
                default:
                    return Vector3.zero;
            }
        }
        public static Dictionary<DirectionEye, Vector3> AuxCamDic = new Dictionary<DirectionEye, Vector3>()
        {
            // They are created at runtime and kept that way.. not exactly pretty.
        // -x = right   
        // +y = up
            {DirectionEye.Pose, new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.DownDownMid, new Vector3(-0.15f + Random.value * 0.3f, -0.1f - Random.value * 0.2f)},
            {DirectionEye.UpMid, new Vector3(-0.15f + Random.value * 0.3f, 0.1f + Random.value * 0.2f)},
            {DirectionEye.UpRight, new Vector3(-0.1f - Random.value * 0.2f, 0.1f + Random.value * 0.2f)},
            {DirectionEye.MidRight, new Vector3(-0.1f - Random.value * 0.2f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.DownRight, new Vector3(-0.1f - Random.value * 0.2f, -0.1f - Random.value * 0.2f)},
            {DirectionEye.DownMid, new Vector3(-0.15f + Random.value * 0.3f, -0.1f - Random.value * 0.2f)},
            {DirectionEye.DownLeft, new Vector3(0.1f + Random.value * 0.2f, -0.1f - Random.value * 0.2f)},
            {DirectionEye.MidLeft, new Vector3(0.1f + Random.value * 0.2f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.UpLeft, new Vector3(0.1f + Random.value * 0.2f, 0.1f + Random.value * 0.2f)},
            {DirectionEye.Mid, new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.Away, new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.PoiDown, new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.PoiUp, new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.PoiRollAway, new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f)}

        };
        public static Dictionary<DirectionEye, Vector3> AuxPoiDic = new Dictionary<DirectionEye, Vector3>()
        {
        // +x = up
        // -y = right
            {DirectionEye.Cam, new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.Pose, new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.DownDownMid, new Vector3(-0.1f - Random.value * 0.2f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.UpMid, new Vector3(0.1f + Random.value * 0.2f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.UpRight, new Vector3(0.1f + Random.value * 0.2f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.MidRight, new Vector3(-0.15f + Random.value * 0.3f, -0.1f - Random.value * 0.2f)},
            {DirectionEye.DownRight, new Vector3(-0.1f - Random.value * 0.2f, -0.1f - Random.value * 0.2f)},
            {DirectionEye.DownMid, new Vector3(-0.1f - Random.value * 0.2f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.DownLeft, new Vector3(-0.1f - Random.value * 0.2f, -0.1f - Random.value * 0.2f)},
            {DirectionEye.MidLeft, new Vector3(-0.15f + Random.value * 0.3f, 0.1f + Random.value * 0.2f)},
            {DirectionEye.UpLeft, new Vector3(0.1f + Random.value * 0.2f, 0.1f + Random.value * 0.2f)},
            {DirectionEye.Mid, new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.Away, new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.PoiDown, new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.PoiUp, new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f)},
            {DirectionEye.PoiRollAway, new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f)}
        };
        public static List<Vector3> AuxCamDodgeList = new List<Vector3>()
        {
            {new Vector3(-0.2f - Random.value * 0.2f, -0.15f + Random.value * 0.3f)}, // Right
            {new Vector3(0.2f + Random.value * 0.2f, -0.15f + Random.value * 0.3f)}, // Left
            {new Vector3(-0.15f + Random.value * 0.3f, 0.2f + Random.value * 0.2f)}, // Up
            {new Vector3(-0.15f + Random.value * 0.3f, -0.2f - Random.value * 0.2f)} // Down
        };
        public static Dictionary<int, DirectionEye> EyeDirForNeckFollow = new Dictionary<int, DirectionEye>()
        {
            {0, DirectionEye.Cam},
            {5, DirectionEye.UpRight},
            {6, DirectionEye.MidRight},
            {7, DirectionEye.DownRight},
            {9, DirectionEye.DownLeft},
            {10, DirectionEye.MidLeft},
            {11, DirectionEye.UpLeft},
        };
        /// <summary>
        /// Current neck goes in, list of available choices goes out.
        /// </summary>
        public static List<DirectionNeck> GetAibuIdleNeckDir(DirectionNeck direction)
        {
            var newList = new List<DirectionNeck>();
            foreach (var dir in AibuIdleDirections)
            {
                if (dir != direction)
                    newList.Add(dir);
            }
            return newList;
        }
        /// <summary>
        /// Current neck goes in, list of available choices goes out.
        /// </summary>
        public static List<DirectionNeck> GetAibuActionDir(DirectionNeck direction)
        {
            var newList = new List<DirectionNeck>();
            foreach (var dir in AibuActionDirections)
            {
                if (dir != direction)
                    newList.Add(dir);
            }
            return newList;
        }
        /// <summary>
        /// Current neck goes in, list of available choices goes out.
        /// </summary>
        public static List<DirectionNeck> GetAibuBackDir(DirectionNeck direction)
        {

            var newList = new List<DirectionNeck>();
            foreach (var dir in AibuBackDirections)
            {
                if (dir != direction)
                    newList.Add(dir);
            }
            return newList;
        }
        private static readonly List<DirectionNeck> AibuIdleDirections = new List<DirectionNeck>
        {
            DirectionNeck.Mid,
            DirectionNeck.MidRight,
            DirectionNeck.MidLeft,
            //DirectionNeck.Pose
        };
        private static readonly List<DirectionNeck> AibuActionDirections = new List<DirectionNeck>
        {
            DirectionNeck.DownDownLeft,
            DirectionNeck.DownRight,
            DirectionNeck.DownMid,
            DirectionNeck.MidLeft,
            DirectionNeck.MidRight,
            DirectionNeck.Mid,
            //DirectionNeck.Pose
        };
        private static readonly List<DirectionNeck> AibuBackDirections = new List<DirectionNeck>
        {
            DirectionNeck.Mid,
            DirectionNeck.MidRight,
            DirectionNeck.MidLeft,
            DirectionNeck.DownRight,
            DirectionNeck.DownDownLeft,
            DirectionNeck.UpMid,
            DirectionNeck.UpRight,
            DirectionNeck.UpLeft,
            //DirectionNeck.Pose
        };
        //public static Dictionary<DirectionNeck, List<DirectionNeck>> AibuFrontIdleNeckDirections = new Dictionary<DirectionNeck, List<DirectionNeck>>()
        //{
        //    // Keep it simple, mess almost always is a detractor.
        //    {
        //        DirectionNeck.Mid,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft
        //        }
        //    },
        //    {
        //        DirectionNeck.MidRight,
        //        new List<DirectionNeck> {
        //            DirectionNeck.Mid,
        //            DirectionNeck.MidRight
        //        }
        //    },
        //    {
        //        DirectionNeck.MidLeft,
        //        new List<DirectionNeck> {
        //            //DirectionNeck.Pose,
        //            DirectionNeck.MidRight,
        //            DirectionNeck.Mid
        //        }
        //    },
        //    {
        //        DirectionNeck.DownMid,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.Mid
        //        }
        //    },
        //    {
        //        DirectionNeck.DownDownLeft,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.Mid
        //        }
        //    },
        //    {
        //        DirectionNeck.DownRight,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.Mid
        //        }
        //    },
        //    {
        //        DirectionNeck.Pose,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft
        //        }
        //    },
        //    {
        //        DirectionNeck.Cam,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.Mid
        //        }
        //    },
        //    {
        //        DirectionNeck.UpMid,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.Mid
        //        }
        //    },
        //    {
        //        DirectionNeck.UpRight,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.Mid
        //        }
        //    },
        //    {
        //        DirectionNeck.UpLeft,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.Mid
        //        }
        //    },
        //    {
        //        DirectionNeck.Away,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.Mid
        //        }
        //    },
        //    {
        //        DirectionNeck.UpRightFar,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.Mid
        //        }
        //    }
        //};
        //public static Dictionary<DirectionNeck, List<DirectionNeck>> AibuFrontActionNeckDirections = new Dictionary<DirectionNeck, List<DirectionNeck>>()
        //{
        //    {
        //        DirectionNeck.Mid,
        //        new List<DirectionNeck> {
        //            DirectionNeck.Cam,
        //            DirectionNeck.Pose,
        //            DirectionNeck.DownLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.DownMid,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.MidRight
        //        }
        //    },
        //    {
        //        DirectionNeck.MidRight,
        //        new List<DirectionNeck> {
        //            DirectionNeck.Cam,
        //            DirectionNeck.Pose,
        //            DirectionNeck.DownLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.DownMid,
        //            DirectionNeck.MidLeft
        //        }
        //    },
        //    {
        //        DirectionNeck.MidLeft,
        //        new List<DirectionNeck> {
        //            DirectionNeck.Cam,
        //            DirectionNeck.Pose,
        //            DirectionNeck.DownLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.DownMid,
        //            DirectionNeck.MidRight
        //        }
        //    },
        //    {
        //        DirectionNeck.DownMid,
        //        new List<DirectionNeck> {
        //            DirectionNeck.Cam,
        //            DirectionNeck.Pose,
        //            DirectionNeck.DownLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft
        //        }
        //    },
        //    {
        //        DirectionNeck.DownDownLeft,
        //        new List<DirectionNeck> {
        //            DirectionNeck.Cam,
        //            DirectionNeck.Pose,
        //            DirectionNeck.DownLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.DownMid,
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft
        //        }
        //    },
        //    {
        //        DirectionNeck.DownRight,
        //        new List<DirectionNeck> {
        //            DirectionNeck.Cam,
        //            DirectionNeck.Pose,
        //            DirectionNeck.DownLeft,
        //            DirectionNeck.DownMid,
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft
        //        }
        //    },
        //    {
        //        DirectionNeck.Pose,
        //        new List<DirectionNeck> {
        //            DirectionNeck.Cam,
        //            DirectionNeck.DownLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.DownMid,
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft
        //        }
        //    },
        //    {
        //        DirectionNeck.Cam,
        //        new List<DirectionNeck> {
        //            DirectionNeck.Pose,
        //            DirectionNeck.DownLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.DownMid,
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft
        //        }
        //    },
        //    {
        //        DirectionNeck.UpMid,
        //        new List<DirectionNeck> {
        //            DirectionNeck.Pose,
        //            DirectionNeck.Cam
        //        }
        //    },
        //    {
        //        DirectionNeck.UpRight,
        //        new List<DirectionNeck> {
        //            DirectionNeck.Pose,
        //            DirectionNeck.Cam
        //        }
        //    },
        //    {
        //        DirectionNeck.UpLeft,
        //        new List<DirectionNeck> {
        //            DirectionNeck.Pose,
        //            DirectionNeck.Cam
        //        }
        //    },
        //    {
        //        DirectionNeck.Away,
        //        new List<DirectionNeck> {
        //            DirectionNeck.Pose,
        //            DirectionNeck.Cam
        //        }
        //    },
        //    {
        //        DirectionNeck.UpRightFar,
        //        new List<DirectionNeck> {
        //            DirectionNeck.Pose,
        //            DirectionNeck.Cam
        //        }
        //    }
        //};
        //public static Dictionary<DirectionNeck, List<DirectionNeck>> AibuBackNeckDirections = new Dictionary<DirectionNeck, List<DirectionNeck>>()
        //{
        //    {
        //        DirectionNeck.Mid,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.DownDownLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.Pose,
        //            DirectionNeck.UpMid,
        //            DirectionNeck.UpRight,
        //            DirectionNeck.UpLeft,
        //        }
        //    },
        //    {
        //        DirectionNeck.MidRight, // looks off
        //        new List<DirectionNeck> {
        //            //DirectionNeck.Mid,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.DownDownLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.Pose,
        //            DirectionNeck.UpMid,
        //            DirectionNeck.UpRight,
        //            DirectionNeck.UpLeft,
        //        }
        //    },
        //    {
        //        DirectionNeck.MidLeft,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            //DirectionNeck.Mid,
        //            DirectionNeck.DownDownLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.Pose,
        //            DirectionNeck.UpMid,
        //            DirectionNeck.UpRight,
        //            DirectionNeck.UpLeft,
        //        }
        //    },
        //    {
        //        DirectionNeck.DownMid,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.DownDownLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.Pose,
        //            DirectionNeck.UpMid,
        //            //DirectionNeck.Mid,
        //            DirectionNeck.UpRight,
        //            DirectionNeck.UpLeft,
        //        }
        //    },
        //    {
        //        DirectionNeck.DownDownLeft,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.Pose,
        //            DirectionNeck.UpMid,
        //            //DirectionNeck.Mid,
        //            DirectionNeck.UpRight,
        //            DirectionNeck.UpLeft,
        //        }
        //    },
        //    {
        //        DirectionNeck.DownRight,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.DownDownLeft,
        //            DirectionNeck.Pose,
        //            DirectionNeck.UpMid,
        //            //DirectionNeck.Mid,
        //            DirectionNeck.UpRight,
        //            DirectionNeck.UpLeft,
        //        }
        //    },
        //    {
        //        DirectionNeck.Pose,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.DownDownLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.UpMid,
        //            //DirectionNeck.Mid,
        //            DirectionNeck.UpRight,
        //            DirectionNeck.UpLeft,
        //        }
        //    },
        //    {
        //        DirectionNeck.Cam,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.DownDownLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.UpMid,
        //            //DirectionNeck.Mid,
        //            DirectionNeck.UpRight,
        //            DirectionNeck.UpLeft,
        //            DirectionNeck.Pose
        //        }
        //    },
        //    {
        //        DirectionNeck.UpMid,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.DownDownLeft,
        //            DirectionNeck.DownRight,
        //            //DirectionNeck.Mid,
        //            DirectionNeck.Pose,
        //            DirectionNeck.UpLeft,
        //            DirectionNeck.UpRight
        //        }
        //    },
        //    {
        //        DirectionNeck.UpLeft,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.DownDownLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.UpMid,
        //            //DirectionNeck.Mid,
        //            DirectionNeck.UpRight,
        //            DirectionNeck.Pose
        //        }
        //    },
        //    {
        //        DirectionNeck.UpRight, // looks funny
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.DownDownLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.UpMid,
        //            //DirectionNeck.Mid,
        //            DirectionNeck.UpLeft,
        //            DirectionNeck.Pose
        //        }
        //    },
        //    {
        //        DirectionNeck.UpRightFar,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.UpMid,
        //            //DirectionNeck.Mid,
        //            DirectionNeck.Pose
        //        }
        //    },
        //    {
        //        DirectionNeck.Away,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight,
        //            DirectionNeck.MidLeft,
        //            DirectionNeck.DownDownLeft,
        //            DirectionNeck.DownRight,
        //            DirectionNeck.UpMid,
        //            //DirectionNeck.Mid,
        //            DirectionNeck.UpLeft,
        //            DirectionNeck.Pose
        //        }
        //    }
        //};
        //public static Dictionary<DirectionEye, List<DirectionNeck>> NeckFollowEyeDir = new Dictionary<DirectionEye, List<DirectionNeck>>()
        //{
        //    {
        //        DirectionEye.MidLeft,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidLeft
        //        }
        //    },
        //    {
        //        DirectionEye.MidRight,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight
        //        }
        //    },
        //    {
        //        DirectionEye.DownLeft,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidLeft
        //        }
        //    },
        //    {
        //        DirectionEye.DownRight,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight
        //        }
        //    },
        //    {
        //        DirectionEye.UpLeft,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidLeft
        //        }
        //    },
        //    {
        //        DirectionEye.UpRight,
        //        new List<DirectionNeck> {
        //            DirectionNeck.MidRight
        //        }
        //    },
        //    {
        //        DirectionEye.Cam,
        //        new List<DirectionNeck> {
        //            DirectionNeck.Cam
        //        }
        //    }
        //};
        public static Dictionary<int, DirectionNeck> SpecialNeckDirections = new Dictionary<int, DirectionNeck>()
        {
            {34, DirectionNeck.Away},
            {68,  DirectionNeck.Away},
            {102, DirectionNeck.Away},
            {136, DirectionNeck.UpRightFar},
            {153, DirectionNeck.UpMid},
        };
        public static Dictionary<HFlag.EMode, List<string>> FrontAnimations = new Dictionary<HFlag.EMode, List<string>>()
        {
            {
                HFlag.EMode.sonyu,
                new List<string> {
                    "khs_f_n04", // cowgirl
                    "khs_f_92", // cowgirl hands on knees
                    "khs_f_99", // cowgirl holding hands
                    "khs_f_111", // cowgirl holding hands 2
                    "khs_f_126", // cowgirl nip play
                    "khs_f_72", // cowgirl restrain
                    "khs_f_74", // cowgirl restrain 2
                    "khs_f_129", // cowgirl side ride   
                    "khs_f_73", // femdom piledriver
                    "khs_f_107", // flexion
                    "khs_f_00", // missionary
                    "khs_f_85", // missionary 2
                    "khs_f_128", // missionary 3    
                    "khs_f_n00", // spread legs
                    "khs_f_120", // spread legs alternative
                    "khs_f_95", // spread eagle
                    "khs_f_62", // missionary lifted up 1
                    "khs_f_67", // missionary lifted up 2
                    "khs_f_110", // knee hug
                    "khs_f_117", // legs on shoulders
                    "khs_f_130", // crossed legs on shoulders
                    "khs_f_105", // straddle legs on shoulders
                    "khs_f_n23", // mating press    
                    "khs_f_n08", // standing, carrying
                    "khs_f_n24", // sitting, facing the girl
                    "khs_f_78", // sitting, flexion
                    "khs_f_n09", // sitting, riding
                    "khs_f_96", // sitting, straddle
                    "khs_f_n27", // sitting, cowgirl
                    "khs_f_122", // desk, against the table
                    "khs_f_121", // desk, facing the girl
                    "khs_f_75", // desk, flexion
                    "khs_f_131", // desk, legs on shoulder
                    "khs_f_n13", // desk, missionary
                    "khs_f_87", // desk, missionary 2
                    "khs_f_64", // wall, on the floor
                    "khs_f_89", // wall, splits

                }
            },
            {
                HFlag.EMode.aibu,
                new List<string> {
                    "kha_f_00", // standing
                    "kha_f_01", // lying
                    "kha_f_03", // sitting
                    "kha_f_07", // desk sitting
                    "kha_f_08", // desk squat
                    "kha_f_09", // standing bookshelf
                }
            }
        };

        public static Dictionary<HFlag.EMode, List<string>> BackAnimations = new Dictionary<HFlag.EMode, List<string>>()
        {
            {
                HFlag.EMode.sonyu,
                new List<string> {
                    "khs_f_100", // bridge
                    "khs_f_02", // doggy
                    "khs_f_n02", // doggy arm pull
                    "khs_f_93", // doggy forced
                    "khs_f_77", // doggy straddle
                    "khs_f_86", // femdom reverse piledriver
                    "khs_f_n06", // spooning 
                    "khs_f_65", // spooning 2
                    "khs_f_119", // spooning 3
                    "khs_f_60", // reverse cowgirl
                    "khs_f_71", // reverse cowgirl 2
                    "khs_f_112", // reverse cowgirl 3
                    "khs_f_113", // reverse cowgirl 4
                    "khs_f_81", // reverse cowgirl splits
                    "khs_f_97", // nelson
                    "khs_f_n26", // lying behind
                    "khs_f_106", // straddle back
                    "khs_f_132", // straddle back 2
                    "khs_f_108", // seiza rear
                    "khs_f_79", // trust back cuddle
                    "khs_f_118", // trust back arm grab
                    "khs_f_94", // trust back arm lock
                    "khs_f_109", // standing bridge
                    "khs_f_61", // standing behind
                    "khs_f_66", // standing behind arm lock
                    "khs_f_115", // standing behind arm grab
                    "khs_f_63", // standing back carrying
                    "khs_f_98", // standing nelson
                    "khs_f_90", // standing doggy
                    "khs_f_124", // standing doggy lifted
                    "khs_f_70", // sitting, nehind four legs
                    "khs_f_n10", // sitting, behind
                    "khs_f_83", // sitting, behind squat
                    "khs_f_11", // sitting, doggy
                    "khs_f_n11", // stitting, doggy arm grab
                    "khs_f_91", // sitting, froggy
                    "khs_f_n10", // sitting, reverse cowgirl
                    "khs_f_n16", // desk, spooning
                    "khs_f_14", // desk, doggy
                    "khs_f_n14", // desk, doggy arm grab
                    "khs_f_125", // desk on table
                    "khs_f_84", // wall, doggy
                    "khs_f_18", // wall, doggy
                    "khs_f_n18", // wall, doggy arm grab
                    "khs_f_n21", // wall, fence behind 
                    "khs_f_n20", // pool, behind 
                }
            },
            {
                HFlag.EMode.aibu,
                new List<string> {
                    "kha_f_02", // four legged
                    "kha_f_04", // chair doggy
                    "kha_f_05", // desk doggy
                    "kha_f_06", // wall doggy
                }
            }

        };
        public static Dictionary<HFlag.EMode, List<string>> DontMoveAnimations = new Dictionary<HFlag.EMode, List<string>>()
        {
            {
                HFlag.EMode.sonyu,
                new List<string> {
                    "khs_f_76", // cowgirl hug
                    "khs_f_104", // doggy face down
                    "khs_f_116", // missionary holding hands
                    "khs_f_102", // missionary interlock
                    "khs_f_127", // missionary lifted up 3
                    "khs_f_80", // lotus stacking
                    "khs_f_114", // stacking
                    "khs_f_123", // princess hug
                    "khs_f_68", // piledriver
                    "khs_f_88", // piledriver 2
                    "khs_f_n07", // standing
                    "khs_f_69", // standing arm hold
                    "khs_f_82", // desk, lotus stacking
                    "khs_f_103", // desk, doggy face down
                    "khs_f_59", // wall, carrying forced
                    "khs_f_n17", // wall, carrying
                    "khs_f_101", // wall, riding 
                    "khs_f_n28", // wall, subway behind
                    "khs_f_n22", // wall, fence carrying
                    "khs_f_n25", // vault, doggy

                }
            },
            {
                HFlag.EMode.aibu,
                new List<string> {
                    "kha_f_10", // studying
                }
            }
        };
        public static Dictionary<string, List<string>> DontMoveNeckSpecialCases = new Dictionary<string, List<string>>()
        {
            // Key - controller in question
            // Value - First letter of animation
            //     A for A_Touch/A_Loop, and so on. 
            {
                "kha_f_02",
                new List<string> {
                    "A",
                    "S"
                }
            }
        };
    }
}
