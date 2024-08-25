using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using static KK_SensibleH.SensibleH;
using System.Collections;
using KK_SensibleH.Caress;
using Illusion.Game;
using KKAPI;
using KKAPI.MainGame;
#if KKS
using SaveData;
#endif

namespace KK_SensibleH
{
    /// <summary>
    /// Recently broken:
    /// </summary>
    internal class VoiceController : MonoBehaviour
    {
        private SaveData.Heroine _heroine;
        private GirlController _master;
        private ChaControl _chara;
        private HVoiceCtrl.Voice _voice;
        private int _main = 0;
        private bool _recentVoiceProc;
        private bool _nickAvailable;
        private int _lastVoice;
        //private string _lastVoiceId;
        internal bool IsVoiceActive => _voice.state == HVoiceCtrl.VoiceKind.voice;
        internal void Initialize(GirlController master, int main)
        {
            _main = main;
            _heroine = _hFlag.lstHeroine[main];
            _nickAvailable = _heroine.isNickNameEvent || _hFlag.isFreeH;
            _voice = _hVoiceCtrl.nowVoices[main];
            _chara = _chaControl[main];
            _master = master;
        }
        public void OnVoiceProc()
        {
            
            var id = _hFlag.voice.playVoices[_main];
            if (id == _lastVoice)
            {
                return;
            }
            else
            {
                SensibleH.Logger.LogDebug($"OnVoiceProc[{id}]");
                if (SensibleH.EyeNeckControl.Value)
                {
                    _master._neckController.LookAtCam();
                }
                if (Kiss.Instance != null)
                {
                    Kiss.Instance.OnVoiceProc();
                }
                _master._lastVoice = id;
                _lastVoice = id;
                //_lastVoiceId = _voice.voiceInfo.nameFile;
                _recentVoiceProc = true;
            }
        }
        internal void Proc()
        {
            // TODO Leave only "nickname instead of voice", only in aibu idle probably (and re-integrate it).
            if (_recentVoiceProc && !IsVoiceActive)
            {
                _recentVoiceProc = false;
                _lastVoice = -1;
            }
        }
        private IEnumerator PlayBeforeVoice(int pattern)
        {
            SensibleH.Logger.LogDebug($"PlayBeforeVoice[Start]");
            _voice.state = HVoiceCtrl.VoiceKind.voice;
            _voice.notOverWrite = true;
            PlayNickname(pattern);
            yield return new WaitForSeconds(0.5f);
            //yield return new WaitUntil(() => _chara.asVoice != null && _chara.asVoice.clip != null);

            var charaVoice = _chara.asVoice;
            var haltTime = charaVoice.clip.length * (0.5f + Random.value / 4f);

            yield return new WaitUntil(() => charaVoice.time > haltTime);

            _voice.state = HVoiceCtrl.VoiceKind.breath;
            _voice.notOverWrite = false;
            SensibleH.Logger.LogDebug($"PlayBeforeVoice[End]");
        }
        private IEnumerator PlayAfterVoice(int pattern)
        {
            SensibleH.Logger.LogDebug($"PlayAfterVoice[Start]");
            yield return new WaitForSeconds(1f);
            var charaVoice = _chara.asVoice;
            var haltTime = charaVoice.clip.length - (0.5f + Random.value / 2f);
            while (charaVoice.time < haltTime)
            {
                yield return new WaitForSeconds(0.2f);
            }
            PlayNickname(pattern);
            SensibleH.Logger.LogDebug($"PlayAfterVoice[End]");
        }
        private void PlayInsteadOfVoice(int pattern)
        {
            SensibleH.Logger.LogDebug($"PlayInsteadOfVoice");
            _hFlag.voice.playVoices[_main] = -1;
            PlayNickname(pattern);
            switch (_hFlag.mode)
            {
                case HFlag.EMode.aibu:
                    _hFlag.voice.timeAibu.timeIdle /=  4f;
                    break;
                case HFlag.EMode.sonyu:
                    _hFlag.voice.timeSonyu.timeIdle /=  4f;
                    break;
            }
        }
        private enum CallTypes
        {
            None,
            AfterVoice,
            BeforeVoice,
            InsteadOfVoice
        }
        private CallTypes PickNickname(out int voicePtn)
        {
            switch (_hFlag.mode)
            {
                case HFlag.EMode.aibu:
                    if (_hFlag.nowAnimStateName.EndsWith("_Idle"))
                    {
                        voicePtn = Random.value > 0.5f ? 0 : 2;
                        if (voicePtn == 0)
                            return (CallTypes)Random.Range(2, 4);
                        else
                            return (CallTypes)Random.Range(1, 4);
                    }
                    break;
                case HFlag.EMode.sonyu:
                    if (_hFlag.nowAnimStateName.EndsWith("Loop") && !_hFlag.nowAnimStateName.StartsWith("I") && !_hFlag.nowAnimationInfo.isFemaleInitiative && _hFlag.speedCalc < 0.5f)
                    {
                        if (_hFlag.nowAnimStateName.StartsWith("W"))
                            voicePtn = Random.value > 0.5f ? 0 : 2;
                        else
                            voicePtn = 2;
                        return CallTypes.BeforeVoice;
                    }
                    break;

            }
            voicePtn = 0;
            return CallTypes.None;
        }
        private Dictionary<int, List<int>> voices = new Dictionary<int, List<int>>()
        {
            // 0 - HUTSUU, /Meek and shy
            // 1 - AKARUKU, /Sober and willful
            // 2 - TUYA / Meek and eager voice
            {
                // Meek and shy voice.
                0, new List<int>()
                {
                    // Aibu Loops preVoice/insteadOfVoice
                    // Sonyu weakSlowLoop preVoice
                }
            },
            {
                // Sober and willful voice.
                1, new List<int>()
                {
                    // Switch from Aibu to Sonyu (5) preVoice
                }
            },
            {
                // Meek and eager voice.
                2, new List<int>()
                {
                    // Aibu Loops preVoice/postVoice/insteadOfVoice
                    310, // Sonyu weakSlowLoop preVoice
                    // Sonyu strongSlowLoop preVoice
                    328 // Sonyu after orgasm
                }
            }
            // Bad Picks
            // 100 / Aibu Idle
        };

        private void PlayNickname(int pattern)
        {
            SensibleH.Logger.LogDebug($"PlayNickname[{pattern}]");
#if KK
            var callFileData = SaveData.FindCallFileData(_heroine.personality, _heroine.callMyID);
#else
            var callFileData = WorldData.FindCallFileData(_heroine.personality, _heroine.callMyID);
#endif
            
            var setting = new Utils.Voice.Setting
            {
                no = _heroine.voiceNo,
                assetBundleName = callFileData.bundle,
                assetName = callFileData.GetFileName(pattern),
                pitch = _heroine.voicePitch,
                voiceTrans = _hFlag.transVoiceMouth[_main]
            };

            _chara.ChangeMouthPtn(0, true);
#if KK
            _chara.SetVoiceTransform(Utils.Voice.OnecePlayChara(setting));
#else
            _chara.SetLipSync(Utils.Voice.OncePlayChara(setting));
#endif
        }
        public bool SayNickname()
        {
            if (!_nickAvailable || IsVoiceActive)
                return false;
            
            if (Random.value < 0.9f)
                return false;

            switch (PickNickname(out var voicePtn))
            {
                case CallTypes.AfterVoice:
                    StartCoroutine(PlayAfterVoice(voicePtn));
                    break;
                case CallTypes.BeforeVoice:
                    StartCoroutine(PlayBeforeVoice(voicePtn));
                    break;
                case CallTypes.InsteadOfVoice:
                    PlayInsteadOfVoice(voicePtn);
                    break;
                case CallTypes.None:
                    return false;
            }
            return true;

        }
    }
}
        