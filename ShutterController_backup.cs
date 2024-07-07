// using Sons.Gui.Input;
// using UnityEngine;
// using Sons.Input;
// using TheForest.Utils;
// using RedLoader;

// namespace WhiteLib
// {
//     public class ShutterController_backup : MonoBehaviour
//     {
//         public Dictionary<Transform, List<Transform>> hooksPerHandle;
//         public Dictionary<Transform, (string openTrigger, string closeTrigger)> triggersPerHandle;
//         public Dictionary<Transform, bool> isOpen;
//         public List<Transform> handles;
//         public Animator animator;
//         public Texture2D whileOpen;
//         public Texture2D whileClose;
        
//         public Transform FindIt(string it) {
//             return transform.Find(it);
//         }

//         public void OnEnable() {
//             Init(handles);
//         }

//         public void Init(List<Transform> _handles) {
//             try {
//                 RLog.Msg($"ShutterInit - handles count: {_handles.Count}");
//                 foreach (Transform handle in _handles) {
//                     RLog.Warning(handle.parent.name);
//                     isOpen.Add(handle, false);
//                     LinkUiElement linkUiElement = handle.gameObject.GetComponent<LinkUiElement>() ?? handle.gameObject.AddComponent<LinkUiElement>();
//                     linkUiElement.SetId("screen.resetTrap", true);
//                     linkUiElement._maxDistance = 2.5f;
//                     RLog.Error($"ShutterInit - t1");
//                     linkUiElement._texture = whileClose;
//                     RLog.Error($"ShutterInit - t2");
//                     linkUiElement._applyTexture = true;
//                     linkUiElement.OnEnable();
//                     linkUiElement.GetUiGameObject();

//                     Close(handle);
//                 }
//             } catch (Exception e) {
//                 RLog.Error($"ShutterInit - {e.Message}");
//             }
//         }
//         public void Open(Transform handle) {
//             foreach (Transform hook in hooksPerHandle[handle]) {
//                 if (hook.childCount > 1) {
//                     hook.FindDeepChild("InteractionTrigger").gameObject.SetActive(true);
//                 } else {
//                     hook.gameObject.SetActive(true);
//                 }
//             }
//             animator.SetTrigger(triggersPerHandle[handle].openTrigger);
//         }
//         public void Close(Transform handle) {
//             foreach (Transform hook in hooksPerHandle[handle]) {
//                 if (hook.childCount > 1) {
//                     hook.FindDeepChild("InteractionTrigger").gameObject.SetActive(false);
//                 } else {
//                     hook.gameObject.SetActive(false);
//                 }
//             }
//             animator.SetTrigger(triggersPerHandle[handle].closeTrigger);
//             // animator.
//         }

//         private void Update() {
//             if (InputSystem.InputMapping.@default.Use.WasPerformedThisFrame()) {
//                 Transform mainCamTr = LocalPlayer._instance._mainCamTr;
//                 if (Physics.SphereCast(mainCamTr.position, 0.3f, mainCamTr.forward, out RaycastHit raycastHit, 10f, LayerMask.GetMask(new string[]
// 				{
// 					"Default"
// 				}))) {
//                     if (raycastHit.collider == null) {
//                         return;
//                     }
//                     if (isOpen.ContainsKey(raycastHit.collider.transform)) {
//                         if (isOpen[raycastHit.collider.transform]) {
//                             Close(raycastHit.collider.transform);
//                         } else {
//                             Open(raycastHit.collider.transform);
//                         }
//                     }
//                     RLog.Msg($"ShutterController | {raycastHit.collider.transform.parent.name} | {raycastHit.collider.name}");
//                 };
//             }
//         }
//     }
// }