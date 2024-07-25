using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static KK_SensibleH.GirlController;

namespace KK_SensibleH.Resources
{
    public struct EyeNeckDictionaries
    {

        public static Dictionary<int, DirectionNeck> NeckDirections = new Dictionary<int, DirectionNeck>()
        {
            {0, DirectionNeck.Pose},
            {17, DirectionNeck.Cam},
            {34, DirectionNeck.Away},
            {51, DirectionNeck.Cam},
            {68,  DirectionNeck.Away},
            {85,  DirectionNeck.Cam}, // used during sonyu _tag[2] / is it?
            {102, DirectionNeck.Away},
            {119, DirectionNeck.DownLeft},
            {136, DirectionNeck.UpRightFar},
            {153, DirectionNeck.UpMid},
            {170, DirectionNeck.UpRight},
            {187, DirectionNeck.MidRight},
            {204, DirectionNeck.DownRight},
            {221, DirectionNeck.DownMid},
            {238, DirectionNeck.DownDownLeft},
            {255, DirectionNeck.MidLeft},
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
        public static Dictionary<int, DirectionEye> NewEyeDirections = new Dictionary<int, DirectionEye>()
        {
            {0, DirectionEye.Cam},
            {5, DirectionEye.UpRight},
            {6, DirectionEye.MidRight},
            {7, DirectionEye.DownRight},
            {9, DirectionEye.DownLeft},
            {10, DirectionEye.MidLeft},
            {11, DirectionEye.UpLeft},
        };
        public static Dictionary<DirectionNeck, List<DirectionNeck>> AibuFrontIdleNeckDirections = new Dictionary<DirectionNeck, List<DirectionNeck>>()
        {
            {
                DirectionNeck.Mid,
                new List<DirectionNeck> {
                    DirectionNeck.MidRight,
                    DirectionNeck.MidLeft,
                    DirectionNeck.DownDownLeft,
                    DirectionNeck.DownRight,
                    DirectionNeck.Pose,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.MidRight,
                new List<DirectionNeck> {
                    DirectionNeck.Mid,
                    DirectionNeck.Pose,
                    DirectionNeck.DownRight,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.MidLeft,
                new List<DirectionNeck> {
                    DirectionNeck.Mid,
                    DirectionNeck.Pose,
                    DirectionNeck.DownDownLeft,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.DownMid,
                new List<DirectionNeck> {
                    DirectionNeck.Pose,
                    DirectionNeck.Mid,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.DownDownLeft,
                new List<DirectionNeck> {
                    DirectionNeck.Mid,
                    DirectionNeck.Pose,
                    DirectionNeck.MidLeft,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.DownRight,
                new List<DirectionNeck> {
                    DirectionNeck.Mid,
                    DirectionNeck.Pose,
                    DirectionNeck.MidRight,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.Pose,
                new List<DirectionNeck> {
                    DirectionNeck.MidRight,
                    DirectionNeck.MidLeft,
                    DirectionNeck.DownDownLeft,
                    DirectionNeck.DownRight,
                    DirectionNeck.Mid,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.Cam,
                new List<DirectionNeck> {
                    DirectionNeck.Pose,
                    DirectionNeck.Mid,
                    DirectionNeck.MidRight,
                    DirectionNeck.MidLeft
                }
            },
            {
                DirectionNeck.UpMid,
                new List<DirectionNeck> {
                    DirectionNeck.Pose,
                    DirectionNeck.Mid,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.UpRight,
                new List<DirectionNeck> {
                    DirectionNeck.Pose,
                    DirectionNeck.Mid,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.UpLeft,
                new List<DirectionNeck> {
                    DirectionNeck.Pose,
                    DirectionNeck.Mid,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.Away,
                new List<DirectionNeck> {
                    DirectionNeck.Pose,
                    DirectionNeck.Mid,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.UpRightFar,
                new List<DirectionNeck> {
                    DirectionNeck.Pose,
                    DirectionNeck.Mid,
                    DirectionNeck.Cam
                }
            }
        };
        public static Dictionary<DirectionNeck, List<DirectionNeck>> AibuFrontActionNeckDirections = new Dictionary<DirectionNeck, List<DirectionNeck>>()
        {
            {
                DirectionNeck.Mid,
                new List<DirectionNeck> {
                    DirectionNeck.Pose,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.MidRight,
                new List<DirectionNeck> {
                    DirectionNeck.Pose,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.MidLeft,
                new List<DirectionNeck> {
                    DirectionNeck.Pose,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.DownMid,
                new List<DirectionNeck> {
                    DirectionNeck.DownDownLeft,
                    DirectionNeck.DownRight,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.DownDownLeft,
                new List<DirectionNeck> {
                    DirectionNeck.DownMid,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.DownRight,
                new List<DirectionNeck> {
                    DirectionNeck.DownMid,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.Pose,
                new List<DirectionNeck> {
                    DirectionNeck.Pose,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.Cam,
                new List<DirectionNeck> {
                    DirectionNeck.DownMid,
                    DirectionNeck.DownDownLeft,
                    DirectionNeck.DownRight
                }
            },
            {
                DirectionNeck.UpMid,
                new List<DirectionNeck> {
                    DirectionNeck.Pose,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.UpRight,
                new List<DirectionNeck> {
                    DirectionNeck.Pose,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.UpLeft,
                new List<DirectionNeck> {
                    DirectionNeck.Pose,
                    DirectionNeck.Cam
                }
            },
            {
                DirectionNeck.Away,
                new List<DirectionNeck> {
                    DirectionNeck.Pose,
                    DirectionNeck.Mid,
                    DirectionNeck.Cam,
                    DirectionNeck.Mid,
                    DirectionNeck.MidLeft,
                    DirectionNeck.MidRight
                }
            },
            {
                DirectionNeck.UpRightFar,
                new List<DirectionNeck> {
                    DirectionNeck.Pose,
                    DirectionNeck.Mid,
                    DirectionNeck.Cam,
                    DirectionNeck.MidRight
                }
            }
        };
        public static Dictionary<DirectionNeck, List<DirectionNeck>> AibuBackNeckDirections = new Dictionary<DirectionNeck, List<DirectionNeck>>()
        {
            {
                DirectionNeck.Mid,
                new List<DirectionNeck> {
                    DirectionNeck.MidRight,
                    DirectionNeck.MidLeft,
                    DirectionNeck.DownDownLeft,
                    DirectionNeck.DownRight,
                    DirectionNeck.Pose,
                    DirectionNeck.UpMid,
                    DirectionNeck.UpRight,
                    DirectionNeck.UpLeft,
                }
            },
            {
                DirectionNeck.MidRight, // looks funny
                new List<DirectionNeck> {
                    DirectionNeck.Mid,
                    DirectionNeck.MidLeft,
                    DirectionNeck.DownDownLeft,
                    DirectionNeck.DownRight,
                    DirectionNeck.Pose,
                    DirectionNeck.UpMid,
                    DirectionNeck.UpRight,
                    DirectionNeck.UpLeft,
                }
            },
            {
                DirectionNeck.MidLeft,
                new List<DirectionNeck> {
                    DirectionNeck.MidRight,
                    DirectionNeck.Mid,
                    DirectionNeck.DownDownLeft,
                    DirectionNeck.DownRight,
                    DirectionNeck.Pose,
                    DirectionNeck.UpMid,
                    DirectionNeck.UpRight,
                    DirectionNeck.UpLeft,
                }
            },
            {
                DirectionNeck.DownMid,
                new List<DirectionNeck> {
                    DirectionNeck.MidRight,
                    DirectionNeck.MidLeft,
                    DirectionNeck.DownDownLeft,
                    DirectionNeck.DownRight,
                    DirectionNeck.Pose,
                    DirectionNeck.UpMid,
                    DirectionNeck.Mid,
                    DirectionNeck.UpRight,
                    DirectionNeck.UpLeft,
                }
            },
            {
                DirectionNeck.DownDownLeft,
                new List<DirectionNeck> {
                    DirectionNeck.MidRight,
                    DirectionNeck.MidLeft,
                    DirectionNeck.DownRight,
                    DirectionNeck.Pose,
                    DirectionNeck.UpMid,
                    DirectionNeck.Mid,
                    DirectionNeck.UpRight,
                    DirectionNeck.UpLeft,
                }
            },
            {
                DirectionNeck.DownRight,
                new List<DirectionNeck> {
                    DirectionNeck.MidRight,
                    DirectionNeck.MidLeft,
                    DirectionNeck.DownDownLeft,
                    DirectionNeck.Pose,
                    DirectionNeck.UpMid,
                    DirectionNeck.Mid,
                    DirectionNeck.UpRight,
                    DirectionNeck.UpLeft,
                }
            },
            {
                DirectionNeck.Pose,
                new List<DirectionNeck> {
                    DirectionNeck.MidRight,
                    DirectionNeck.MidLeft,
                    DirectionNeck.DownDownLeft,
                    DirectionNeck.DownRight,
                    DirectionNeck.UpMid,
                    DirectionNeck.Mid,
                    DirectionNeck.UpRight,
                    DirectionNeck.UpLeft,
                }
            },
            {
                DirectionNeck.Cam,
                new List<DirectionNeck> {
                    DirectionNeck.MidRight,
                    DirectionNeck.MidLeft,
                    DirectionNeck.DownDownLeft,
                    DirectionNeck.DownRight,
                    DirectionNeck.UpMid,
                    DirectionNeck.Mid,
                    DirectionNeck.UpRight,
                    DirectionNeck.UpLeft,
                    DirectionNeck.Pose
                }
            },
            {
                DirectionNeck.UpMid,
                new List<DirectionNeck> {
                    DirectionNeck.MidRight,
                    DirectionNeck.MidLeft,
                    DirectionNeck.DownDownLeft,
                    DirectionNeck.DownRight,
                    DirectionNeck.Mid,
                    DirectionNeck.Pose,
                    DirectionNeck.UpLeft,
                    DirectionNeck.UpRight
                }
            },
            {
                DirectionNeck.UpLeft,
                new List<DirectionNeck> {
                    DirectionNeck.MidRight,
                    DirectionNeck.MidLeft,
                    DirectionNeck.DownDownLeft,
                    DirectionNeck.DownRight,
                    DirectionNeck.UpMid,
                    DirectionNeck.Mid,
                    DirectionNeck.UpRight,
                    DirectionNeck.Pose
                }
            },
            {
                DirectionNeck.UpRight, // looks funny
                new List<DirectionNeck> {
                    DirectionNeck.MidRight,
                    DirectionNeck.MidLeft,
                    DirectionNeck.DownDownLeft,
                    DirectionNeck.DownRight,
                    DirectionNeck.UpMid,
                    DirectionNeck.Mid,
                    DirectionNeck.UpLeft,
                    DirectionNeck.Pose
                }
            },
            {
                DirectionNeck.UpRightFar,
                new List<DirectionNeck> {
                    DirectionNeck.MidRight,
                    DirectionNeck.DownRight,
                    DirectionNeck.UpMid,
                    DirectionNeck.Mid,
                    DirectionNeck.Pose
                }
            },
            {
                DirectionNeck.Away,
                new List<DirectionNeck> {
                    DirectionNeck.MidRight,
                    DirectionNeck.MidLeft,
                    DirectionNeck.DownDownLeft,
                    DirectionNeck.DownRight,
                    DirectionNeck.UpMid,
                    DirectionNeck.Mid,
                    DirectionNeck.UpLeft,
                    DirectionNeck.Pose
                }
            }
        };
        public static Dictionary<DirectionEye, List<DirectionNeck>> NeckFollowEyeDir = new Dictionary<DirectionEye, List<DirectionNeck>>()
        {
            {
                DirectionEye.MidLeft,
                new List<DirectionNeck> {
                    DirectionNeck.MidLeft
                }
            },
            {
                DirectionEye.MidRight,
                new List<DirectionNeck> {
                    DirectionNeck.MidRight
                }
            },
            {
                DirectionEye.DownLeft,
                new List<DirectionNeck> {
                    DirectionNeck.MidLeft
                }
            },
            {
                DirectionEye.DownRight,
                new List<DirectionNeck> {
                    DirectionNeck.MidRight
                }
            },
            {
                DirectionEye.UpLeft,
                new List<DirectionNeck> {
                    DirectionNeck.MidLeft
                }
            },
            {
                DirectionEye.UpRight,
                new List<DirectionNeck> {
                    DirectionNeck.MidRight
                }
            },
            {
                DirectionEye.Cam,
                new List<DirectionNeck> {
                    DirectionNeck.Cam
                }
            }
        };
        public static Dictionary<int, DirectionNeck> SpecialNeckDirections = new Dictionary<int, DirectionNeck>()
        {
            {34, DirectionNeck.Away},
            {68,  DirectionNeck.Away},
            {102, DirectionNeck.Away},
            {136, DirectionNeck.UpRightFar}, // Seems to be bugged.
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
                    "kha_f_08", // desk squat
                    "kha_f_07", // desk sitting
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
