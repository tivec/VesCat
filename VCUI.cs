using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VesCat
{
	enum uiMode
	{
		normal,
		changeCategory,
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
				mainWindowRect = GUILayout.Window(mainGUIWindowGuid, mainWindowRect, mainGUIWindow, "Vessel Categorizer", GUILayout.Width(0), GUILayout.Height(0));

			}
		}

		// recursive function to draw each category
		public void drawCategory(Guid id, int depth, bool drawCategories = false, Action<Guid> callback = null)
		{
			// first we draw all the categories, and check if they are enabled
			foreach (KeyValuePair<Guid, string> category in Data.Categories.Where(cw => Data.getParent(cw.Key) == id)) {

				// first make sure it's among the known visible nodes. Start as false.
				if (!visibleNodes.ContainsKey(category.Key)) {
					visibleNodes [category.Key] = false;
				}

				GUILayout.BeginHorizontal ();

				if (depth > 0) {
					GUILayout.Space (depth * 10);
				}

				visibleNodes[category.Key] = GUILayout.Toggle (visibleNodes[category.Key], category.Value, UIStyle.UISkin.customStyles [(int)uiStyles.categoryButton]);
				GUILayout.EndHorizontal ();
				// if the category is visible, we will also draw everything known beneath it
				if (visibleNodes[category.Key] || drawCategories) {
					drawCategory (category.Key, depth + 1, drawCategories, callback);
				}
			}

			foreach (Guid vesselId in Data.Vessels.Keys.Where(vw => Data.getParent(vw) == id)) {
				GUILayout.BeginHorizontal ();
				if (depth > 0) {
					GUILayout.Space (depth * 10);
				}
				GUILayout.Button (Tools.GetVesselName (vesselId), UIStyle.UISkin.customStyles [(int)uiStyles.vesselButton]);
				GUILayout.EndHorizontal ();
			}
		}

		public void mainGUIWindow(int windowID)
		{
			GUILayout.Label ("Status: " + statusString);
			GUILayout.BeginVertical();

			// calculate how big the window should be here
			int vesselsListHeight = Math.Min (Math.Max ((Data.Vessels.Count + Data.Categories.Count + 1) * 15, 400), 600);
			vesselListScroll = GUILayout.BeginScrollView (vesselListScroll, GUILayout.Width(400), GUILayout.Height(vesselsListHeight));

			drawCategory (DataStorage.ROOT_GUID, 0);

			GUILayout.EndScrollView ();
			GUILayout.EndVertical();

			if (currentMode == uiMode.addCategory) {
				GUILayout.BeginVertical ();
				newCatString = GUILayout.TextField (newCatString);
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button("Save")) {
					Data.AddCategory (newCatString);
					currentMode = uiMode.normal;
					statusString = "Added new category '" + newCatString + "'.";
				}
				if (GUILayout.Button("Cancel")) {
						currentMode = uiMode.normal;
				}
				GUILayout.EndHorizontal ();
				GUILayout.EndVertical();

			} else {
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button("Add new category"))
				{
					newCatString = "";
					currentMode = uiMode.addCategory;
				}
				GUILayout.EndHorizontal ();
			}


			GUI.DragWindow (); // make it draggable.
		}


	}
}

