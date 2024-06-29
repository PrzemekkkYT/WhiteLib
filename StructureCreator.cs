using System.Text.RegularExpressions;
using Sons.Items.Core;
using Sons.Weapon;
using Sons.Crafting.Structures;
using UnityEngine;
using Object = UnityEngine.Object;
using SonsSdk;
using Sons.Crafting;
using static Sons.Crafting.HeldBookInteraction;
using Endnight.Utilities;
using UnityEngine.UI;
using Il2CppInterop.Runtime.Injection;
using RedLoader;
using System.Reflection;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using TheForest.World;
using Construction;
using HarmonyLib;

namespace WhiteLib {
    public class StructureCreator {
		public static Dictionary<string, GameObject> tabTemplates = new();
		public static StructureCraftingNode craftingNodeTemplate = new();
		public static Dictionary<string, Material> materialList = new();

        internal static BlueprintBookController heldBook;

		internal static Dictionary<string, GameObject> GetTabTemplates() {
			return tabTemplates;
		}

		public static void Init() {
			RLog.Msg("TestOnInit");
			PopulateTabTemplates(GetHeldBook());
			FindCraftingNodeTemplate();
		}

		internal static void PopulateTabTemplates(BlueprintBookController book) {
			GameObject tempRootTab = Object.Instantiate<GameObject>(book.transform.FindDeepChild("Tab_Storage").gameObject);

			GameObject tempTab = Object.Instantiate<GameObject>(tempRootTab);
			Object.DestroyImmediate(tempTab.transform.Find("AnimRoot/Tab1").gameObject);
			tempTab.name = tempTab.name.Replace("(Clone)", "");
			tabTemplates.Add("RootTab", tempTab);

			for (int i = 1; i < 8; i++) {
				tempTab = Object.Instantiate<GameObject>(tempRootTab.transform.FindDeepChild(string.Format("Tab{0}", i)).gameObject);
				if (i<7) {
					Object.DestroyImmediate(tempTab.transform.Find(string.Format("AnimRoot/Tab{0}", i+1)).gameObject);
				}
				tempTab.name = tempTab.name.Replace("(Clone)", "");
				tabTemplates.Add(tempTab.name, tempTab);
			}
		}

        internal static BlueprintBookController GetHeldBook() {
			heldBook = ItemDatabaseManager.ItemById(552)._heldPrefab.GetComponent<BlueprintBookController>();
            return heldBook;
        }

		internal static StructureCraftingNode FindCraftingNodeTemplate() {
			GameObject structureNodePrefab = CommonExtensions.Instantiate(ConstructionTools.GetRecipe(26)._structureNodePrefab, false);
			StructureCraftingNode component = structureNodePrefab.GetComponent<StructureCraftingNode>();
			CommonExtensions.TryDestroy(structureNodePrefab.transform.Find("LeanTo").gameObject);
			CommonExtensions.HideAndDontSave(CommonExtensions.DontDestroyOnLoad(structureNodePrefab)).SetActive(false);
			foreach (StructureElement structureElement in structureNodePrefab.GetComponentsInChildren<StructureElement>(true)) {
				Object.Destroy(structureElement.gameObject);
			}
			craftingNodeTemplate._ingredientUiTemplate = null;
			craftingNodeTemplate = component;
			return craftingNodeTemplate;
		}

        public static StructureRecipe CreateRecipe(int id, string structureName, Texture2D structureImage, GameObject blueprintModel, GameObject builtPrefab, ValueTuple<int, int>[] ingredients, StructureRecipe.CategoryType category, bool canRotate = true, bool alignToSurface = true)
		{
			StructureRecipe structureRecipe = ScriptableObject.CreateInstance<StructureRecipe>();
			structureRecipe._id = id;
			structureImage.name = structureName;
			structureRecipe._displayName = structureName;
			structureRecipe._recipeImage = structureImage;
			structureRecipe._builtPrefab = builtPrefab;
			structureRecipe._ingredients = GetStructureIngredients(ingredients);
			structureRecipe._anchor = StructureRecipe.AnchorType.Bottom;
			structureRecipe._alignToSurface = alignToSurface;
			structureRecipe._canBeRotated = canRotate;
			structureRecipe._forceUp = true;
			structureRecipe._forceUpAngleThreshold = 35f;
			structureRecipe._category = category;
			structureRecipe._maxDistanceMultiplier = 1.5f;
			structureRecipe._placeMode = StructureRecipe.PlaceModeType.Single;
			structureRecipe._isDiscovered = true;
			structureRecipe._structureNodePrefab = CommonExtensions.DontDestroyOnLoad(CreateBlueprintPrefab(blueprintModel, structureRecipe).gameObject);
			return structureRecipe;
		}

		public static void CreatePage(StructureRecipe topRecipe, StructureRecipe bottomRecipe, string pageTitle, Texture2D pageImage)
		{
            BlueprintBookPageData blueprintBookPageData = new BlueprintBookPageData
            {
                _topRecipe = topRecipe,
                _bottomRecipe = bottomRecipe,
                _pageImage = pageImage,
                _pageTitleLocalizationId = pageTitle
            };
            heldBook._pages._pages.Add(blueprintBookPageData);
		}

// dodać możliwość dodania ikonki dla taba i spróbować dodać nazwę (możliwe, że wymagany będzie harmonypatch)
		public static GameObject CreateTab(string tabName, Sprite tabIcon, Color tabColor, HeldBookInteraction rootTab = null) {
			Transform tabsObject = heldBook.transform.FindDeepChild("Tabs");

			GameObject newTab;
			HeldBookInteraction newTabInteraction;
			if (rootTab == null) {
				if (tabsObject.childCount < 14) {
					newTab = Object.Instantiate<GameObject>(tabTemplates["RootTab"].gameObject);
					newTab.transform.parent = tabsObject.transform;
					newTabInteraction = newTab.GetComponent<HeldBookInteraction>();

					newTab.transform.parent = tabsObject.transform;
					newTab.name = string.Format("Tab_{0}", tabName);

					RectTransform newTabRT = newTab.GetComponent<RectTransform>();

					newTabRT.anchoredPosition = new Vector2(0, 0);
					newTabRT.anchoredPosition3D = new Vector3(0, 0, -0.0029f);
					newTabRT.anchorMax = new Vector2(0, 0);
					newTabRT.anchorMin = new Vector2(0, 0);
					newTabRT.offsetMax = new Vector2(0.5f, 0.5f);
					newTabRT.offsetMin = new Vector2(-0.5f, -0.5f);

					Transform newTabIcon = newTab.transform.Find("AnimRoot/Visuals/Icon");
					newTabIcon.GetComponent<Image>().sprite = tabIcon;
					newTabIcon.rotation = Quaternion.Euler(0,0,0);
					newTabIcon.gameObject.SetActive(true);

					float possiblePos = -0.02f*(tabsObject.childCount-12);
					if (possiblePos > -0.04) {
						newTab.transform.position = new Vector3(possiblePos, 0.12f, 0);
						newTab.transform.rotation = Quaternion.Euler(0, 0, 270);
					}
					
					newTab.AddComponent<LayoutElement>().ignoreLayout = true;
					ClassInjector.RegisterTypeInIl2Cpp<SubTabsController>();
					newTab.AddComponent<SubTabsController>();
				} else {
					return null;
				}
			} else {
				SubTabsController subTabsCon = rootTab.GetComponent<SubTabsController>();
				if (subTabsCon.subTabs.Count < 7) {
					newTab = Object.Instantiate<GameObject>(tabTemplates.Values.ElementAt(subTabsCon.subTabs.Count+1).gameObject);
					newTabInteraction = newTab.GetComponent<HeldBookInteraction>();

					newTabInteraction._rootTab = rootTab;
					if (subTabsCon.subTabs.Count == 0) {
						newTab.transform.parent = rootTab.transform.FindChild("AnimRoot");
					} else {
						// newTab.transform.parent = rootTab.transform.FindDeepChild(string.Format("Tab{0}", subTabsCon.subTabs.Count)).FindChild("AnimRoot");
						newTab.transform.parent = subTabsCon.subTabs.Last().FindChild("AnimRoot");
					}

					RectTransform newTabRT = newTab.GetComponent<RectTransform>();

					newTabRT.anchoredPosition = new Vector2(0, -0.016f);
					newTabRT.anchoredPosition3D = new Vector3(0, -0.016f, -0.0011f);
					newTabRT.anchorMax = new Vector2(0.5f, 0.5f);
					newTabRT.anchorMin = new Vector2(0.5f, 0.5f);
					newTabRT.offsetMax = new Vector2(0.5f, 0.484f);
					newTabRT.offsetMin = new Vector2(-0.5f, -0.516f);

					// newTab.transform.position = new Vector3(0, 0, -0.0011f);
					newTab.transform.localPosition = new Vector3(0, 0.0005f, -0.0011f);
					newTab.transform.rotation = Quaternion.Euler(0, 0, -90);

					BoxCollider collider = newTab.GetComponent<BoxCollider>();
					collider.center = new Vector3(-0.01f, 0, 0);
					collider.extents = new Vector3(0.025f, 0.01f, 0.0006f);
					collider.size = new Vector3(0.05f, 0.02f, 0.0012f);

					BoxCollider parentCollider = newTab.transform.parent.parent.GetComponent<BoxCollider>();
					parentCollider.center = new Vector3(-0.0025f, 0, 0);
					parentCollider.extents = new Vector3(0.0113f, 0.01f, 0.0006f);
					parentCollider.size = new Vector3(0.0225f, 0.02f, 0.0012f);

					subTabsCon.subTabs.Add(newTab.transform);
				} else {
					return null;
				}
			}

			newTab.name = newTab.name.Replace("(Clone)", "");

			MeshRenderer tabMesh = newTab.transform.Find("AnimRoot/TactiPadTab").GetComponent<MeshRenderer>();
			tabMesh.material.color = tabColor;

			Object.DestroyImmediate(newTab.GetComponent<MouseEventsProxy>());
			newTabInteraction._animator = newTab.GetComponent<Animator>();
			newTabInteraction._layoutType = LayoutType.Tab;
			newTabInteraction._tabPageIndex = heldBook._tabs.Count;
			newTabInteraction._mouseEventsProxy = null;
			heldBook._tabs.Add(newTabInteraction);
			if (rootTab == null) {
				heldBook._categoryStartingPage.Add(newTabInteraction._tabPageIndex);
			}
			return newTab;
		}

        internal static StructureCraftingNode CreateBlueprintPrefab(GameObject blueprintModel, StructureRecipe recipe)
		{
			StructureCraftingNode structureCraftingNode = Object.Instantiate<StructureCraftingNode>(craftingNodeTemplate);
			GameObject modelObject = CommonExtensions.Instantiate(blueprintModel, false);

			structureCraftingNode.gameObject.name = recipe._displayName;
			
			modelObject.SetParent(structureCraftingNode.transform, false);
			modelObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

			Shader shader = Shader.Find("Sons/HDRPLit");
			foreach (MeshRenderer meshRenderer in structureCraftingNode.gameObject.GetComponentsInChildren<MeshRenderer>())
			{
				foreach (Material material in meshRenderer.materials)
				{
					material.shader = shader;
				}
			}
			
			Dictionary<int, Il2CppSystem.Collections.Generic.List<StructureCraftingNodeIngredient>> dictionary = new();
			foreach (Transform itemGroup in modelObject.transform.GetChildren()) {
				MatchCollection matchCollection = Regex.Matches(itemGroup.name, @"\((\d+)\)");
				string itemId = String.Join("", matchCollection.Cast<Match>().Select(m => m.Groups[1].Value));
				try {
					foreach (Transform item in itemGroup.GetChildren()) {
						Renderer renderer = item.GetComponent<Renderer>();
						if (renderer != null) {
							// if (!renderer.GetComponent<StructureGhostSwapper>()) {
							// 	renderer.gameObject.AddComponent<StructureGhostSwapper>();
							// }
							if (!materialList.ContainsKey(item.name)) {
								materialList.Add(item.name, renderer.material);
							}
						}

						StructureCraftingNodeIngredient ingredient = item.gameObject.AddComponent<StructureCraftingNodeIngredient>();
						ingredient._itemId = int.Parse(itemId);
						ingredient._requiresAllPreviousIngredients = false;
						if (!dictionary.ContainsKey(ingredient.ItemId)) {
							dictionary.Add(ingredient._itemId, new Il2CppSystem.Collections.Generic.List<StructureCraftingNodeIngredient>());
						}
						dictionary[ingredient._itemId].Add(ingredient);
					}
				} catch (Exception e) {
					RLog.Error($"1 {e.Message}");
				}
			}
			structureCraftingNode._craftingIngredientLinks.Clear();
			foreach (KeyValuePair<int, Il2CppSystem.Collections.Generic.List<StructureCraftingNodeIngredient>> keyValuePair in dictionary)
			{
				int itemId;
				Il2CppSystem.Collections.Generic.List<StructureCraftingNodeIngredient> ingredientList;
				keyValuePair.Deconstruct(out itemId, out ingredientList);
				structureCraftingNode._craftingIngredientLinks.Add(new StructureCraftingNode.CraftingIngredientLink{
					Ingredient = new StructureCraftingRecipeIngredient{
						Count = ingredientList.Count,
						ItemId = itemId
					},
					_ingredients = ingredientList
				});
			}

			structureCraftingNode._recipe = recipe;
			
			InitObjectInteraction(structureCraftingNode);

			ClassInjector.RegisterTypeInIl2Cpp<GhostFix>();
			structureCraftingNode.gameObject.AddComponent<GhostFix>().structureCraftingNode = structureCraftingNode;
			
			structureCraftingNode.gameObject.SetActive(true);
			structureCraftingNode.ShowGhost(true);
			return structureCraftingNode;
		}

		internal static void InitObjectInteraction(StructureCraftingNode node) {
			if (node._ingredientUiTemplate)
			{
				return;
			}
			try {
				Transform transform = CommonExtensions.Instantiate(craftingNodeTemplate.transform.Find("StructureInteractionObjects").gameObject, false).transform;
				transform.SetParent(node.transform, false);
				transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
				node._ingredientUiTemplate = transform.Find("Canvas/UiRoot/Ingredients/IngredientUiTemplate").gameObject;
				node._cancelStructureInteractionElement = transform.Find("Canvas/UiRoot/CancelStructureInteractionElement").gameObject;
				node._ingredientUi = new Il2CppSystem.Collections.Generic.List<StructureCraftingNodeIngredientUi>();
				node._ingredientUi.Add(transform.Find("Canvas/UiRoot/Ingredients/IngredientUi").GetComponent<StructureCraftingNodeIngredientUi>());
				transform.Find("UiLocator").localPosition = node.GetComponent<BoxCollider>().center;
			} catch (Exception e) {
				RLog.Error(e.Message);
			}
		}

        internal static Il2CppSystem.Collections.Generic.List<StructureCraftingRecipeIngredient> GetStructureIngredients(ValueTuple<int, int>[] ingredients)
		{
			Il2CppSystem.Collections.Generic.List<StructureCraftingRecipeIngredient> list = new();
			foreach (ValueTuple<int, int> valueTuple in ingredients)
			{
				list.Add(new StructureCraftingRecipeIngredient
				{
					ItemId = valueTuple.Item1,
					Count = valueTuple.Item2
				});
			}
			return list;
		}
    }
}