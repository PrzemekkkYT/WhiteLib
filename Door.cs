using RedLoader;
using SonsSdk;
using UnityEngine;

namespace WhiteLib {
    public class Door : MonoBehaviour {
        public DoorController _doorController;
        public List<Transform> _hooks = new();
        public bool _isOpen = false;

        public void Start() {
            foreach (Transform hookParent in transform.root.FindAllDeepChildRegex(@$"\({transform.parent.name}\)$")) {
                foreach (Transform hook in hookParent.GetChildren()) {
                    _hooks.Add(hook);
                }
            }
        }

        public void Open() {
            _doorController.animator.SetTrigger($"{transform.parent.name}_Open");
            foreach (Transform hook in _hooks) {
                if (hook.childCount > 1) {
                    hook.FindDeepChild("InteractionTrigger").gameObject.SetActive(true);
                } else {
                    hook.gameObject.SetActive(true);
                }
            }
            _isOpen = true;
        }
        public void Close() {
            _doorController.animator.SetTrigger($"{transform.parent.name}_Close");
            foreach (Transform hook in _hooks) {
                if (hook.childCount > 1) {
                    hook.FindDeepChild("InteractionTrigger").gameObject.SetActive(false);
                } else {
                    hook.gameObject.SetActive(false);
                }
            }
            _isOpen = false;
        }
    }
}