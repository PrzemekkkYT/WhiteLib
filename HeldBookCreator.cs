using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using RedLoader;
using Sons.Crafting;
using Sons.Crafting.Structures;
using static Sons.Crafting.HeldBookInteraction;
using Sons.Items.Core;
using Sons.Weapon;
using Endnight.Utilities;
using WhiteLib;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace WhiteLib {
    public class HeldBookCreator {
        public static Dictionary<string, GameObject> tabTemplates = new();
		public static HeldBookInteraction missingTabs_lastRoot;
        internal static BlueprintBookController heldBook;

        public static void Init() {
			ClassInjector.RegisterTypeInIl2Cpp<SubTabsController>();

            PopulateTabTemplates(GetHeldBook());
            CreateMissingTabs();
			RLog.Msg("HeldBookCreator Initiated");
        }
		

		internal static void CreateMissingTabs() {
			int PAGESBASENUM = 39;
			int pagesCount = heldBook._pages.Pages.Count;
			RLog.Debug(pagesCount);
			if (pagesCount > PAGESBASENUM) {
				GameObject temp_tab = null;
				int subTabs = 0;
				for (int i = 0; i < pagesCount-PAGESBASENUM; i++) {
					RLog.Warning(i);
					if (temp_tab == null || subTabs >= 7) {
						temp_tab = CreateTab($"Missing", Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), new Vector2(0.5f, 0.5f)), Color.magenta);
						missingTabs_lastRoot = temp_tab.GetComponent<HeldBookInteraction>();
						subTabs = 0;
					} else {
						CreateTab($"Missing", Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), new Vector2(0.5f, 0.5f)), Color.magenta, temp_tab.GetComponent<HeldBookInteraction>());
						subTabs++;
					}
				}
			}
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

		public static StringTable GetStringTable(string table, string locale) {
			return LocalizationSettings.StringDatabase.GetTable(table, LocalizationSettings.AvailableLocales.GetLocale(locale));
		}

		/// <summary>
		/// Przykładowa funkcja, która przyjmuje słownik argumentów.
		/// </summary>
		/// <param name="kwargs">Słownik zawierający argumenty nazwane.</param>
		/// <remarks>
		/// Możliwe opcje w słowniku `kwargs`:
		/// <list type="bullet">
		/// <item><description><c>chinese_Simplified</c>: Chinese Simplified (string)</description></item>
		/// <item><description><c>chinese_Traditional</c>: Chinese Traditional (string)</description></item>
		/// <item><description><c>czech</c>: Czech (string)</description></item>
		/// <item><description><c>english</c>: English (string)</description></item>
		/// <item><description><c>finnish</c>: Finnish (string)</description></item>
		/// <item><description><c>french</c>: French (string)</description></item>
		/// <item><description><c>german</c>: German (string)</description></item>
		/// <item><description><c>italian</c>: Italian (string)</description></item>
		/// <item><description><c>japanese</c>: Japanese (string)</description></item>
		/// <item><description><c>korean</c>: Korean (string)</description></item>
		/// <item><description><c>polish</c>: Polish (string)</description></item>
		/// <item><description><c>portuguese</c>: Portuguese (string)</description></item>
		/// <item><description><c>russian</c>: Russian (string)</description></item>
		/// <item><description><c>spanish</c>: Spanish (string)</description></item>
		/// <item><description><c>swedish</c>: Swedish (string)</description></item>
		/// <item><description><c>turkish</c>: Turkish (string)</description></item>
		/// </list>
		/// </remarks>
		public static void LocalizePageName(string pageName, Dictionary<string, string> kwargs) {
			Dictionary<string, string> locales = new Dictionary<string, string>() {
				{ "chinese_Simplified", "zh" },
				{ "chinese_Traditional", "zh-Hant" },
				{ "czech", "cs" },
				{ "english", "en" },
				{ "finnish", "fi" },
				{ "french", "fr" },
				{ "german", "de" },
				{ "italian", "it" },
				{ "japanese", "ja" },
				{ "korean", "ko" },
				{ "polish", "pl" },
				{ "portuguese", "pt" },
				{ "russian", "ru" },
				{ "spanish", "es" },
				{ "swedish", "sv" },
				{ "turkis", "tr" }
			};
			foreach (var locale in locales) {
				if (kwargs.ContainsKey(locale.Key)) {
					GetStringTable("Items", locale.Value).AddEntry(pageName, kwargs[locale.Key]);
				} else {
					GetStringTable("Items", locale.Value).AddEntry(pageName, pageName);
				}
			}
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
						newTab.transform.parent = subTabsCon.subTabs.Last().FindChild("AnimRoot");
					}

					RectTransform newTabRT = newTab.GetComponent<RectTransform>();

					newTabRT.anchoredPosition = new Vector2(0, -0.016f);
					newTabRT.anchoredPosition3D = new Vector3(0, -0.016f, -0.0011f);
					newTabRT.anchorMax = new Vector2(0.5f, 0.5f);
					newTabRT.anchorMin = new Vector2(0.5f, 0.5f);
					newTabRT.offsetMax = new Vector2(0.5f, 0.484f);
					newTabRT.offsetMin = new Vector2(-0.5f, -0.516f);

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
    }
}