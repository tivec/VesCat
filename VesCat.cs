using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace VesCat
{
	[KSPScenario(ScenarioCreationOptions.AddToAllGames,GameScenes.TRACKSTATION,GameScenes.FLIGHT,GameScenes.SPACECENTER)] 
	public class VesCat : ScenarioModule
	{

		private VCUI UI = new VCUI();
		private CommonTools Tools = CommonTools.Instance;
		private DataStorage Data = DataStorage.Instance;

		public VesCat()
		{
			Debug.Log("[VesCat [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: Constructor");
		}

		public void onVesselCreated(Vessel v) 
		{
			ScreenMessages.PostScreenMessage ("VesCat: Vessel " + v.GetName() + " created.");
			Data.AddVessel (v);
		}

		public void onVesselDestroyed(Vessel v)
		{
			ScreenMessages.PostScreenMessage ("VesCat: Vessel " + v.GetName() + " destroyed.");
			Data.RemoveVessel (v);
		}

		void Start()
		{
			Debug.Log("[VesCat [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: Start");

			Data.UpdateVessels ();
			foreach(Guid g in Data.Vessels.Keys){
				ScreenMessages.PostScreenMessage ("Vessel " + Tools.GetVesselName(g) );
			}

			GameEvents.onVesselCreate.Add (onVesselCreated);
			GameEvents.onVesselDestroy.Add (onVesselDestroyed);

		}

		public override void OnLoad (ConfigNode node)
		{
			ScreenMessages.PostScreenMessage ("VesCat: OnLoad.");
			Debug.Log("[VesCat [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: OnLoad");
			Data.Categories = new Dictionary<Guid, string>(); // reinit this so we don't get duplicates.

			// load up all categories from persistent.sfs
			foreach (ConfigNode n in node.GetNodes().Where(n => n.name == "CATEGORIES"))
			{
				foreach (ConfigNode.Value v in n.values) {
					Guid categoryID = new Guid (v.name);
					String categoryName = v.value;

					if (!Data.Categories.ContainsKey (categoryID)) {
						Data.Categories.Add (categoryID, categoryName);
					}
				}
			}

			// load the category parents.
			foreach (ConfigNode n in node.GetNodes().Where(n => n.name == "CATEGORYPARENTS")) {
				foreach (ConfigNode.Value v in n.values) {
					Guid category = new Guid (v.name);
					Guid parent = new Guid (v.value);

					// validate the category
					if (!Data.Categories.ContainsKey(category)) {
						continue;
					}

					// validate the parent category
					if (!Data.Categories.ContainsKey (parent)) {
						parent = DataStorage.ROOT_GUID;
					}

					if (!Data.CategoryParents.ContainsKey (category)) {
						Data.CategoryParents.Add (category, parent);
					} else {
						Data.CategoryParents [category] = parent;
					}
				}
			}

			// load all known vessels from persistent.sfs, but verify they exist!
			foreach (ConfigNode n in node.GetNodes().Where(n => n.name == "VESSELS")) 
			{
				foreach (ConfigNode.Value v in n.values) {

					Guid vGuid = new Guid (v.name);
					Guid vParent = new Guid (v.value);

					// Verify that the category exists
					if (!Data.Categories.ContainsKey (vParent)) {
						vParent = DataStorage.ROOT_GUID;
					}

					if (!Data.Vessels.ContainsKey (vGuid)) {
						// does the vessel exist?
						if (!FlightGlobals.Vessels.Exists(vS => vS.id == vGuid)) {
							continue;
						}

						// Add it to the data
						Data.Vessels.Add (vGuid, vParent);

					} else {
						// update data with info from save
						Data.Vessels [vGuid] = vParent;
					}
				}
			}
		}

		public override void OnSave (ConfigNode node)
		{

			ScreenMessages.PostScreenMessage ("VesCat: OnSave.");

			// categories
			ConfigNode cat = new ConfigNode ("CATEGORIES");
			foreach (KeyValuePair<Guid,String> entry in Data.Categories) {
				cat.AddValue (entry.Key.ToString (), entry.Value);
			}
			// category parents
			ConfigNode catP = new ConfigNode ("CATEGORYPARENTS");
			foreach (KeyValuePair<Guid,Guid> entry in Data.CategoryParents) {
				catP.AddValue (entry.Key.ToString (), entry.Value.ToString ());
			}
			// vessels
			ConfigNode vess = new ConfigNode ("VESSELS");
			foreach (KeyValuePair<Guid,Guid> entry in Data.Vessels) {
				vess.AddValue(entry.Key.ToString (), entry.Value.ToString ());
			}

			// add the save nodes.
			node.AddNode (cat);
			node.AddNode (catP);
			node.AddNode (vess);

		}

		void OnGUI()
		{
			if (UIStyle.UISkin == null) {
				UIStyle.CustomSkin ();
			}

			UI.DrawGUI ();
		}

		public override void OnAwake ()
		{
			Debug.Log("[VesCat [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: OnAwake");
		}

		void OnDestroy()
		{
			GameEvents.onVesselCreate.Remove (onVesselCreated);
			GameEvents.onVesselDestroy.Remove (onVesselDestroyed);

			Debug.Log ("[VesCat [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: OnDestroy");
		}

	}
}

