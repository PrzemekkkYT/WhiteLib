using RedLoader;
using Sons.Input;
using TheForest.Utils;
using UnityEngine;

namespace WhiteLib
{
    public class DoorController : MonoBehaviour
    {
        public List<Door> doors = new();
        public Animator animator;
        public Texture2D whileOpen = Assets.ArrowLeft;
        public Texture2D whileClose = Assets.ArrowRight;

        void Start() {
            animator = GetComponent<Animator>();
            foreach (Transform handle in transform.FindAllDeepChild("Handle")) {
                Door door = handle.GetComponent<Door>() ?? handle.gameObject.AddComponent<Door>();
                door._doorController = this;
                doors.Add(door);
            }
        }

        // private void Update() {
        //     if (InputSystem.InputMapping.@default.Use.WasPerformedThisFrame()) {
        //         Transform mainCamTr = LocalPlayer._instance._mainCamTr;
        //         if (Physics.SphereCast(mainCamTr.position, 0.3f, mainCamTr.forward, out RaycastHit raycastHit, 10f, LayerMask.GetMask(new string[]
		// 		{
		// 			"Default"
		// 		}))) {
        //             if (raycastHit.collider == null) {
        //                 return;
        //             }
        //             RLog.Msg($"Door | {raycastHit.collider.transform.parent.name} | {raycastHit.collider.name}");
        //             Door door = raycastHit.collider.gameObject.GetComponent<Door>();
        //             if (door != null) {
        //                 if (door._isOpen) {
        //                     door.Close();
        //                 } else {
        //                     door.Open();
        //                 }
        //             }
        //         };
        //     }
        // }
    }
}