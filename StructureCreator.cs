using System.Text.RegularExpressions;
using Sons.Crafting.Structures;
using UnityEngine;
using Object = UnityEngine.Object;
using SonsSdk;
using Il2CppInterop.Runtime.Injection;
using RedLoader;
using Construction;
using Bolt;
using HarmonyLib;
using Sons.Inventory;
using Sons.Gameplay;
using Sons.Gameplay.GrabBag;
using Endnight.Rendering;
using Il2Generic = Il2CppSystem.Collections.Generic;
using Whitelib;

namespace WhiteLib {
    public class StructureCreator {
		public static StructureCraftingNode craftingNodeTemplate = new();
		public static Dictionary<string, Material> materialList = new();

		public static void Init() {
			HeldBookCreator.Init();
			FindCraftingNodeTemplate();
			RLog.Msg("StructureCreator Initiated");
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
			try {
				builtPrefab.GetComponent<ScrewStructure>()._recipe = structureRecipe;
			} catch (Exception e) {
				RLog.Error($"CreateRecipe - {e.Message}");
				try {
				ScrewStructureWithStorage screwStorageStructure = builtPrefab.GetComponent<ScrewStructureWithStorage>();
				screwStorageStructure._recipe = structureRecipe;
				Il2CppSystem.Collections.Generic.List<StructureStorage> _storageSlots = new();
				foreach (Transform hook in structureRecipe._builtPrefab.transform.FindAllDeepChild("Hook")) {
					_storageSlots.Add(hook.GetComponent<StructureStorage>());
				}
				screwStorageStructure._storageSlots = _storageSlots;


				} catch (Exception e2) {
					RLog.Error($"CreateRecipe - {e2.Message}");
				}
			}

			RegisterBoltPrefab(structureRecipe._builtPrefab);
			RegisterBoltPrefab(structureRecipe._structureNodePrefab);

			return structureRecipe;
		}

		public static void RegisterBoltPrefab(GameObject go)
		{
			if (!BoltNetwork.isRunning)
			{
				return;
			}
            if (go.TryGetComponent<BoltEntity>(out BoltEntity boltEntity))
            {
                if (PrefabDatabase.Instance.Prefabs.Contains(go))
                {
                    return;
                }
                PrefabDatabase.Instance.Prefabs = PrefabDatabase.Instance.Prefabs.AddItem(go).ToArray<GameObject>();
                PrefabDatabase._lookup[boltEntity.prefabId] = go;
            }
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
                Il2Generic.List<StructureCraftingNodeIngredient> ingredientList;
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

        internal static Il2Generic.List<StructureCraftingRecipeIngredient> GetStructureIngredients(ValueTuple<int, int>[] ingredients)
		{
            Il2Generic.List<StructureCraftingRecipeIngredient> list = new();
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
    
		public static void ConfigureAsStructureStorage(GameObject go) {
			Transform shelfPrefab = ConstructionTools.GetRecipe(49)._builtPrefab.transform;
        
			GameObject itemLayoutGroups = Object.Instantiate<GameObject>(shelfPrefab.FindChild("ItemLayoutGroups").gameObject, go.transform);

			Transform grassRemover = go.transform.FindChild("GrassRemover");
			grassRemover.gameObject.AddComponent<ExcludeRenderableFrom>()._excludedFrom = 
				ExcludeRenderableFrom.Type.Collision & 
				ExcludeRenderableFrom.Type.ExcludeFromHideArms & 
				ExcludeRenderableFrom.Type.GreyOut & 
				ExcludeRenderableFrom.Type.MaterialSwap & 
				ExcludeRenderableFrom.Type.Outline & 
				ExcludeRenderableFrom.Type.Sheen;

			GameObject structureEnvironmentCleaner = Object.Instantiate<GameObject>(shelfPrefab.FindChild("StructureEnvironmentCleaner").gameObject, go.transform);
			Il2Generic.List<Transform> grassRemoversList = new();
			grassRemoversList.Add(grassRemover);
			structureEnvironmentCleaner.GetComponent<ScrewStructureEnvironmentCleaner>()._grassRemovers = grassRemoversList;

			// do skonfigurowania
			BoltEntity boltEntity = go.AddComponent<BoltEntity>();
			go.AddComponent<FreeFormStructureBuiltLinker>();
			go.AddComponent<ScrewStructureBuiltLinker>()._entity = boltEntity;
			go.AddComponent<ScrewStructureWithStorage>()._entity = boltEntity;

			ItemStorageController itemStorageController = go.AddComponent<ItemStorageController>();
			Il2Generic.List<ItemStorageHookPoint> hookPoints = new();
			Il2Generic.List<GrabBagCategory> grabBagCategories = new();
			grabBagCategories.Add(GrabBagCategory.Food);
			grabBagCategories.Add(GrabBagCategory.Cooking);
			grabBagCategories.Add(GrabBagCategory.Medicine);
			grabBagCategories.Add(GrabBagCategory.Ammo);
			grabBagCategories.Add(GrabBagCategory.ForStorage);
			grabBagCategories.Add(GrabBagCategory.Throwables);
			grabBagCategories.Add(GrabBagCategory.Tinker);
			grabBagCategories.Add(GrabBagCategory.Armour);
			grabBagCategories.Add(GrabBagCategory.Plants);
			grabBagCategories.Add(GrabBagCategory.Seeds);
			itemStorageController._grabBagCategories = grabBagCategories;

			foreach (Transform hook in go.transform.FindAllDeepChild("Hook")) {
				hook.gameObject.layer = LayerMask.NameToLayer("PickUp");

				ItemInstanceManager instanceManager = hook.gameObject.AddComponent<ItemInstanceManager>();
				
				StructureStorage structureStorage = hook.gameObject.AddComponent<StructureStorage>();
				structureStorage._boltEntity = boltEntity;
				structureStorage._itemInstanceManager = instanceManager;

				ItemStorageHookPoint hookPoint = hook.gameObject.AddComponent<ItemStorageHookPoint>();
				hookPoint._interactionCollider = hook.GetComponent<Collider>();
				Il2Generic.List<InWorldLayoutItemGroup> _itemlayoutGroups = new();
				foreach (Transform layoutGroup in itemLayoutGroups.transform.GetChildren()) {
					_itemlayoutGroups.Add(layoutGroup.GetComponent<InWorldLayoutItemGroup>());
				}
				hookPoint._itemLayoutGroups = _itemlayoutGroups;
				itemStorageController._itemLayoutGroups = _itemlayoutGroups;
				hookPoint._itemStorageController = itemStorageController;
				hookPoints.Add(hookPoint);
				hookPoint._storage = structureStorage;
				hookPoint._itemAnchorLocalOffsetData = shelfPrefab.transform.FindDeepChild("Hook").GetComponent<ItemStorageHookPoint>()._itemAnchorLocalOffsetData;

				GameObject _interElement = Object.Instantiate<GameObject>(shelfPrefab.FindDeepChild("InteractionElement").gameObject, hook);
				_interElement.name = _interElement.name.Replace("(Clone)", "");

				hookPoint._interactionElement = _interElement;

				
			}
			itemStorageController._hookPoints = hookPoints;
		}
	}
}