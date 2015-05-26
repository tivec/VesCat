using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VesCat
{
	public class VCUI
	{
		// Windows
		private Rect mainWindowRect = new Rect(100, 100, 250, 250);
		private Rect newCategoryWindowRect = new Rect (100, 100, 250, 250);

		// hashed Guids for unique identifiers
		private int mainGUIWindowGuid;
		private int newCategoryWindowGuid;

		// Gui visible at all?
		private bool GUIVisible = true;
		private bool newCategoryVisible = false;

		// Main gui list
		private Vector2 vesselListScroll;

		private CommonTools Tools = CommonTools.Instance;
		private DataStorage Data = DataStorage.Instance;
		private String statusString = "";

		public VCUI ()
		{
			mainGUIWindowGuid = Guid.NewGuid ().GetHashCode (); // unique ID for the gui window.
			newCategoryWindowGuid = Guid.NewGuid ().GetHashCode (); // unique ID for the new category window
		}

		public void DrawGUI()
		{
			if (GUIVisible) {
				mainWindowRect = GUILayout.Window(mainGUIWindowGuid, mainWindowRect, mainGUIWindow, "Vessel Categorizer", GUILayout.Width(0), GUILayout.Height(0));

				if (newCategoryVisible) {
					newCategoryWindowRect = GUILayout.Window(newCategoryWindowGuid, newCategoryWindowRect, newCategoryWindow, "New category", GUILayout.Width (0), GUILayout.Height (0));
					// make the window stick to the main window
					newCategoryWindowRect.x = mainWindowRect.x + mainWindowRect.width + 5;
					newCategoryWindowRect.y = mainWindowRect.y;
				}
			}
		}

		public void newCategoryWindow(int windowID)
		{
			GUILayout.BeginVertical ();
			string text = GUILayout.TextField ("", GUILayout.Width (100), GUILayout.Height (20));
			if (GUILayout.Button("Save")) {
				statusString = text;
				newCategoryVisible = false;
			}
			GUILayout.EndVertical();
		}

		public void mainGUIWindow(int windowID)
		{
			GUILayout.Label ("Status: " + statusString);
			GUILayout.BeginVertical();

			int vesselsListHeight = Math.Min (Math.Max (Data.Vessels.Count * 10, 100), 100);

			vesselListScroll = GUILayout.BeginScrollView (vesselListScroll, GUILayout.Width(400), GUILayout.Height(vesselsListHeight));

			foreach (KeyValuePair<Guid,String> category in Data.Categories) {
				GUILayout.Button (category.Value);
			}

			foreach (Guid id in Data.Vessels.Keys) {
				if(GUILayout.Button (Tools.GetVesselName(id))) {
					try {
						Vessel v = Tools.GetVessel(id);
						statusString = "Clicked " + v.GetName();
					} catch (NullReferenceException ex) {
						// We couldn't find the vessel. That's bad.
						statusString = ex.Message.ToString ();
					}
				}
			}
			GUILayout.EndScrollView ();
			GUILayout.EndVertical();

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button("Add new category"))
			{
				newCategoryVisible = true;
			}
			GUILayout.EndHorizontal ();


			GUI.DragWindow (); // make it draggable.
		}


	}
}

