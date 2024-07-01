using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using RedLoader;
using UnityEngine;

namespace WhiteLib {
    public class Shutter : MonoBehaviour {
        public Il2CppReferenceArray<Transform> hooks;
        public Il2CppStringArray triggers;
        public Il2CppValueField<bool> isOpen;

        void Awake() {
            RLog.DebugBig(hooks.Length);
            // foreach (var hook in hooks) {
            //     hook.Cache();
            // }
        }

        public bool IsOpen() {
            return isOpen.Value;
        }

        public void Triggers() {
            RLog.Msg("===============");
            RLog.Msg(triggers.GetType());
            foreach (var trigger in triggers) {
                RLog.Msg(trigger.GetType());
                RLog.Msg(trigger.ToString());
            }
            RLog.Msg("===============");
        }
        public void Hooks() {
            RLog.Msg("===============");
            RLog.Msg(hooks.GetType());
            foreach (var hook in hooks) {
                RLog.Msg(hook.GetType());
                RLog.Msg(hook.ToString());
            }
            RLog.Msg("===============");
        }
    }
    
}