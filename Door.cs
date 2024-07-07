using Sons.Gui.Input;
using Sons.Input;
using SonsSdk;
using UnityEngine;

namespace WhiteLib {
    public class Door : MonoBehaviour {
        public DoorController _doorController;
        public List<Transform> _hooks = new();
        public LinkUiElement linkUiElement;
        public bool _isOpen = false;

        public void Start() {
            foreach (Transform hookParent in transform.root.FindAllDeepChildRegex(@$"\({transform.parent.name}\)$")) {
                foreach (Transform hook in hookParent.GetChildren()) {
                    _hooks.Add(hook);
                }
            }
            linkUiElement = transform.FindChild("Interaction").GetComponent<LinkUiElement>() ?? transform.FindChild("Interaction").gameObject.AddComponent<LinkUiElement>();
            linkUiElement.SetId("screen.use", true);
            linkUiElement._maxDistance = 1.2f;
            linkUiElement._texture = _doorController.whileClose;
            linkUiElement._applyTexture = true;
            linkUiElement.OnEnable();
            linkUiElement.GetUiGameObject();
            Close();
        }

        private void Update() {
            if (InputSystem.InputMapping.@default.Use.triggered && linkUiElement.IsActive) {
                if (_isOpen) {
                    Close();
                } else {
                    Open();
                }
                linkUiElement.OnEnable();
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
            linkUiElement._texture = _doorController.whileOpen;
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
            linkUiElement._texture = _doorController.whileClose;
            _isOpen = false;
        }
    }
}