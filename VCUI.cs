using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VesCat
{
	enum uiMode
	{
		normal,
		editCategories,
		addCategory
	}

	public class VCUI
	{
		// Windows
		private Rect mainWindowRect = new Rect(100, 100, 250, 250);

		// hashed Guids for unique identifiers
		private int mainGUIWindowGuid;

		// Gui visible at all?
		private bool GUIVisible = true;
		// Main list of stuff
		private Vector2 vesselListScroll;

		// a dictionary showing if a given parent node is visible.
		private Dictionary<Guid, bool> visibleNodes = new Dictionary<Guid, bool> (); 

		// new category string storage
		string newCatString = "";

		// Set category parent
		Guid modifyingCategory;

		uiMode currentMode = uiMode.normal;

		// scheduled actions
		List<Action> scheduled = new List<Action> ();

		private CommonTools Tools = CommonTools.Instance;
		private DataStorage Data = DataStorage.Instance;
		private String statusString = "";

		public VCUI ()
		{
			mainGUIWindowGuid = Guid.NewGuid ().GetHashCode (); // unique ID for the gui window.
			visibleNodes [DataStorage.ROOT_GUID] = true;
		}

		public void DrawGUI()
		{
			if (Event.current.type == EventType.Layout && scheduled.Count > 0) {
				foreach(Action a in scheduled) {
					a ();
				}
				scheduled.Clear ();
			}

			if (GUIVisible) {
				mainWindowRect = GUILayout.Window(mainGUIWindowGuid, mainWindowRect, MainGUIWindow, "Vessel Categorizer", GUILayout.Width(0), GUILayout.Height(0));

			}
		}

		public void StartMovingCategory(Guid id)
		{
			if (currentMode != uiMode.normal) {
				return;
			}

			scheduled.Add (() => {
				currentMode = uiMode.editCategories;
				modifyingCategory = id;
				if (modifyingCategory == DataStorage.ROOT_GUID) {
					statusString = "Click a category to move...";
				} else {
					statusString = "Moving category '" + Data.GetCategoryName(id) + "'";
				}
			});

		}

		// recursive function to draw each category
		public void DrawCategory(Guid id, int depth, bool drawCategories = false)
		{
			// first we draw all the categories, and check if they are enabled
			foreach (KeyValuePair<Guid, string> category in Data.Categories.Where(cw => Data.GetParent(cw.Key) == id).OrderBy(cw => cw.Value)) {

				// first make sure it's among the known visible nodes. Start as false.
				if (!visibleNodes.ContainsKey(category.Key)) {
					visibleNodes [category.Key] = false;
				}

				GUILayout.BeginHorizontal ();

				if (depth > 0) {
					GUILayout.Space (depth * 10);
				}

				if (currentMode == uiMode.normal) {
					Color defaultColor = GUI.backgroundColor;

					UITools.DrawCategoryButton (category.Value, XKCDColors.DarkCyan, visibleNodes [category.Key], 
					                            () => {visibleNodes [category.Key] = true; }, 
												() => {visibleNodes [category.Key] = false; },
												() => {StartMovingCategory(category.Key);});

					GUI.backgroundColor = defaultColor;


				} else if (currentMode == uiMode.editCategories) {
					if (GUILayout.Button(category.Value, UIStyle.UISkin.customStyles [(int)uiStyles.categoryButton])) {
						// if we are clicking a button, and the modifyingCategory Guid is ROOT_GUID, we want to set the
						// modifying category option.
						if (modifyingCategory.Equals (DataStorage.ROOT_GUID)) {
							StartMovingCategory (category.Key);
						} else {
							scheduled.Add (() => {
								Data.SetParent(modifyingCategory, category.Key);
								modifyingCategory = DataStorage.ROOT_GUID;
								currentMode = uiMode.normal;
							});
						}
					}
				} else {
					GUILayout.Button (category.Value, UIStyle.UISkin.customStyles [(int)uiStyles.categoryButton]);
				}
				GUILayout.EndHorizontal ();

				// if the category is visible, we will also draw everything known beneath it
				if (visibleNodes[category.Key] || drawCategories) {
					DrawCategory (category.Key, depth + 1, drawCategories);
				}
			}

			if (currentMode == uiMode.normal || currentMode == uiMode.addCategory) {
				foreach (Guid vesselId in Data.Vessels.Keys.Where(vw => Data.GetParent(vw) == id).OrderBy(vw => Tools.GetVesselName(vw))) {
					GUILayout.BeginHorizontal ();
					if (depth > 0) {
						GUILayout.Space (depth * 10);
					}
					GUILayout.Button (Tools.GetVesselName (vesselId), UIStyle.UISkin.customStyles [(int)uiStyles.vesselButton]);
					GUILayout.EndHorizontal ();
				}
			}
		}

		public void MainGUIWindow(int windowID)
		{
			GUILayout.Label ("Status: " + statusString);
			GUILayout.BeginVertical();

			// calculate how big the window should be here
			int vesselsListHeight = Math.Min (Math.Max ((Data.Vessels.Count + Data.Categories.Count + 1) * 15, 400), 600);
			vesselListScroll = GUILayout.BeginScrollView (vesselListScroll, GUILayout.Width(400), GUILayout.Height(vesselsListHeight));

			if (currentMode == uiMode.editCategories) {

				if (GUILayout.Button("No category")) {
					// if we are clicking a button, and the modifyingCategory Guid is ROOT_GUID, we want to set the
					// modifying category option.
					if (!modifyingCategory.Equals (DataStorage.ROOT_GUID)) {
						scheduled.Add (() => {
							Data.SetParent(modifyingCategory, DataStorage.ROOT_GUID);
							modifyingCategory = DataStorage.ROOT_GUID;
							currentMode = uiMode.normal;
						});
					}
				}

				DrawCategory (DataStorage.ROOT_GUID, 1, true);
			} else {
				DrawCategory (DataStorage.ROOT_GUID, 0);
			}

			GUILayout.EndScrollView ();
			GUILayout.EndVertical();

			/**************** ADDING A CATEGORY ******************/
			if (currentMode == uiMode.addCategory) {
				GUILayout.BeginVertical ();
				GUILayout.Label ("Please enter a category name:");
				newCatString = GUILayout.TextField (newCatString);
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Save")) {
					if (newCatString.Length > 0) {
						Data.AddCategory (newCatString);
						currentMode = uiMode.normal;
					} else {
						currentMode = uiMode.normal;
					}
				}
				if (GUILayout.Button ("Cancel")) {
					currentMode = uiMode.normal;
				}
				GUILayout.EndHorizontal ();
				GUILayout.EndVertical ();

			/**************** EDITING CATEGORIES ******************/
			} else if (currentMode == uiMode.editCategories) {
				GUILayout.BeginVertical ();
				GUILayout.Label (statusString);
				if (GUILayout.Button ("Cancel")) {
					currentMode = uiMode.normal;
					modifyingCategory = DataStorage.ROOT_GUID; // set back if we press cancel!
				}
				GUILayout.EndVertical ();

			/**************** NORMAL MODE ******************/
			} else {
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button("Add new category"))
				{
					newCatString = "";
					currentMode = uiMode.addCategory;
				}
				if (GUILayout.Button("Edit categories")) 
				{
					currentMode = uiMode.editCategories;
					modifyingCategory = DataStorage.ROOT_GUID;
					statusString = "Click a category to move...";
				}
				GUILayout.EndHorizontal ();
			}


			GUI.DragWindow (); // make it draggable.
		}


	}
}

