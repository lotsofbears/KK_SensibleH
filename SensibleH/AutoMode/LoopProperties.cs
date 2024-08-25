using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static KK_SensibleH.SensibleH;

namespace KK_SensibleH.AutoMode
{
    public static class LoopProperties
    {
        public static bool IsVoiceWait => _hFlag.voiceWait || _hFlag.isDenialvoiceWait;
        public static  bool IsIdleInside => _hFlag.nowAnimStateName.EndsWith("InsertIdle", StringComparison.Ordinal);
        public static bool IsIdleOutside => _hFlag.nowAnimStateName.Equals("Idle");
        public static bool IsEndInside => _hFlag.nowAnimStateName.EndsWith("IN_A", StringComparison.Ordinal);
        public static bool IsEndOutside => _hFlag.nowAnimStateName.EndsWith("OUT_A", StringComparison.Ordinal);
        public static bool IsEndInMouth => _hFlag.nowAnimStateName.StartsWith("Oral", StringComparison.Ordinal);
        public static bool IsInsert => _hFlag.nowAnimStateName.EndsWith("Insert", StringComparison.Ordinal);
        public static bool IsWeakLoop => _hFlag.nowAnimStateName.EndsWith("WLoop", StringComparison.Ordinal);
        public static bool IsStrongLoop => _hFlag.nowAnimStateName.EndsWith("SLoop", StringComparison.Ordinal);
        public static bool IsOrgasmLoop => _hFlag.nowAnimStateName.EndsWith("OLoop", StringComparison.Ordinal);
        public static bool IsTouch => _hFlag.nowAnimStateName.EndsWith("Touch", StringComparison.Ordinal);
        public static bool IsAibuItemIdlePos => _hFlag.nowAnimStateName.EndsWith("_Idle", StringComparison.Ordinal);
        public static bool IsPull => _hFlag.nowAnimStateName.EndsWith("Pull", StringComparison.Ordinal);
        public static bool IsFinishLoop => _hFlag.finish != HFlag.FinishKind.none && IsOrgasmLoop;
        public static bool IsActionLoop => _hFlag.nowAnimStateName.EndsWith("Loop", StringComparison.Ordinal);// IsWeakLoop || IsStrongLoop || IsOrgasmLoop;
        public static bool IsEndLoop => IsEndInside || IsEndOutside;
        public static bool IsSonyu => _hFlag.mode == HFlag.EMode.sonyu || _hFlag.mode == HFlag.EMode.sonyu3P;
        public static bool IsHoushi => _hFlag.mode == HFlag.EMode.houshi || _hFlag.mode == HFlag.EMode.houshi3P;
    }
}
