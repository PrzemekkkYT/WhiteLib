using Il2CppInterop.Runtime.InteropTypes.Arrays;
using RedLoader;
using Sons.Crafting.Structures;
using SonsSdk;
using UnityEngine;

namespace WhiteLib
{
    public class GhostFix : MonoBehaviour
    {
        public StructureCraftingNode structureCraftingNode;

        public void OnEnable()
		{
			foreach (Transform itemGroup in structureCraftingNode.transform.FindAllDeepChildRegex(@"\((\d+)\)")) {
				foreach (Transform item in itemGroup.GetChildren()) {
					try {
						Renderer renderer = item.GetComponent<Renderer>();
						StructureGhostSwapper structureGhostSwapper;
						if (item.gameObject.GetComponent<StructureGhostSwapper>()) {
							structureGhostSwapper = item.gameObject.GetComponent<StructureGhostSwapper>();
						} else {
							structureGhostSwapper = item.gameObject.AddComponent<StructureGhostSwapper>();
						}
						structureGhostSwapper._originalMaterialList = new Il2CppReferenceArray<Material>(1);
						structureGhostSwapper._originalMaterialList.Append(StructureCreator.materialList[item.name]);
						renderer.materials = new Il2CppReferenceArray<Material>(1);
						renderer.materials.Append(StructureCreator.materialList[item.name]);
						renderer.material = StructureCreator.materialList[item.name];
					} catch (Exception e) {
						RLog.Error($"GhostFix - {e.Message}");
					}
				}
			}
		}
    }
}