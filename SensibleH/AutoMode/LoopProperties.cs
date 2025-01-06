using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static KK_SensibleH.SensibleH;

namespace KK_SensibleH.AutoMode
{
    public static class LoopProperties
    {
        public static bool IsVoiceWait => hFlag.voiceWait || hFlag.isDenialvoiceWait;
        public static  bool IsIdleInside => hFlag.nowAnimStateName.EndsWith("InsertIdle", StringComparison.Ordinal);
        public static bool IsIdleOutside => hFlag.nowAnimStateName.Equals("Idle");
        public static bool IsEndInside => hFlag.nowAnimStateName.EndsWith("IN_A", StringComparison.Ordinal);
        public static bool IsEndOutside => hFlag.nowAnimStateName.EndsWith("OUT_A", StringComparison.Ordinal);
        public static bool IsEndInMouth => hFlag.nowAnimStateName.StartsWith("Oral", StringComparison.Ordinal);
        public static bool IsHoushiOutside => hFlag.nowAnimStateName.StartsWith("Drink", StringComparison.Ordinal);
        public static bool IsInsert => hFlag.nowAnimStateName.EndsWith("Insert", StringComparison.Ordinal);
        public static bool IsWeakLoop => hFlag.nowAnimStateName.EndsWith("WLoop", StringComparison.Ordinal);
        public static bool IsStrongLoop => hFlag.nowAnimStateName.EndsWith("SLoop", StringComparison.Ordinal);
        public static bool IsOrgasmLoop => hFlag.nowAnimStateName.EndsWith("OLoop", StringComparison.Ordinal);
        public static bool IsTouch => hFlag.nowAnimStateName.EndsWith("Touch", StringComparison.Ordinal);
        public static bool IsAibuItemIdlePos => hFlag.nowAnimStateName.EndsWith("_Idle", StringComparison.Ordinal);
        public static bool IsPull => hFlag.nowAnimStateName.EndsWith("Pull", StringComparison.Ordinal);
        public static bool IsFinishLoop => hFlag.finish != HFlag.FinishKind.none && IsOrgasmLoop;
        public static bool IsActionLoop => hFlag.nowAnimStateName.EndsWith("Loop", StringComparison.Ordinal);// IsWeakLoop || IsStrongLoop || IsOrgasmLoop;
        public static bool IsEndLoop => IsEndInside || IsEndOutside;

        // Modes are trimmed at animController change.
        public static bool IsSonyu => mode == HFlag.EMode.sonyu;
        public static bool IsHoushi => mode == HFlag.EMode.houshi;
        public static bool IsKissLoop
        {
            get
            {
                if (hFlag.mode == HFlag.EMode.aibu)
                {
                    return hFlag.nowAnimStateName.StartsWith("K_", StringComparison.Ordinal);
                }
                else
                {
                    return _handCtrl.IsKissAction();
                }
            }
        }
    }
}
