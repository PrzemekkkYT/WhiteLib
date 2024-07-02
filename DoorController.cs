using UnityEngine;

namespace WhiteLib
{
    public class DoorController : MonoBehaviour
    {
        public List<Door> doors = new();
        public Animator animator;
        public Texture2D whileOpen;
        public Texture2D whileClose;

        void Start() {
            animator = GetComponent<Animator>();
            foreach (Transform handle in transform.FindAllDeepChild("Handle")) {
                Door door = handle.GetComponent<Door>() ?? handle.gameObject.AddComponent<Door>();
                door._doorController = this;
                doors.Add(door);
            }
        }
    }
}