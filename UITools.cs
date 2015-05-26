using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VesCat
{
	public class UITools
	{

		private bool GUIVisible = true;
		private int mainGUIWindowGuid;


		private Vector2 vesselListScroll;

		private Rect mainWindow = new Rect(100, 100, 250, 250);
		private Dictionary<Guid,Guid> vessels = new Dictionary<Guid, Guid> ();
		private CommonTools Tools = CommonTools.Instance;
		private String statusString = "";

		public UITools ()
		{
			mainGUIWindowGuid = Guid.NewGuid ().GetHashCode (); // unique ID for the gui window.
		}

		public void updateVessels(Dictionary<Guid,Guid> ves)
		{
			vessels = ves;
		}

		public void DrawGUI()
		{
			if (GUIVisible) {
				mainWindow = GUILayout.Window(mainGUIWindowGuid, mainWindow, mainGUIWindow, "Vessel Categorizer", GUILayout.Width(0), GUILayout.Height(0));
			}
		}

		public void mainGUIWindow(int windowID)
		{
			GUILayout.Label ("Status: " + statusString);
			GUILayout.BeginVertical();
			vesselListScroll = GUILayout.BeginScrollView (vesselListScroll, GUILayout.Width(400), GUILayout.Height(100));
			foreach (Guid id in vessels.Keys) {
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
			GUI.DragWindow (); // make it draggable.
		}


	}
}

