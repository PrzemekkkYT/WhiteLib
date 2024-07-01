using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace WhiteLib
{
    public class ShutterController : MonoBehaviour
    {
        public Il2CppReferenceArray<Transform> handleDict;
        public Il2CppReferenceField<Animator> animator;
        public Il2CppReferenceField<Texture2D> whileOpen;
        public Il2CppReferenceField<Texture2D> whileClose;

        void Awake() {
            // foreach (var handle in handleDict) {
            //     handle.Cache();
            // }
            animator.Cache();
            whileOpen.Cache();
            whileClose.Cache();
        }

        public Animator GetAnimator() {
            return animator.CachedValue;
        }
    }
}