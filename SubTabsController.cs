using RedLoader;
using UnityEngine;

namespace WhiteLib
{
    public class SubTabsController : MonoBehaviour
    {
        public List<Transform> subTabs = new();

        public List<Transform> GetSubTabs() {
            return subTabs;
        }
    }
}