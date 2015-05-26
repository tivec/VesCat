using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace VesCat
{
	[KSPScenario(ScenarioCreationOptions.AddToAllGames,GameScenes.TRACKSTATION,GameScenes.FLIGHT,GameScenes.SPACECENTER)] 
	public class VesCat : ScenarioModule
	{


		private Dictionary<Guid,Guid> vessels = new Dictionary<Guid, Guid>();		// A dictionary of all known vessels
		private Dictionary<Guid,String> categories = new Dictionary<Guid,String>(); // all categories the player has created
		private Dictionary<Guid,Guid> categoryParents = new Dictionary<Guid,Guid>(); // the parenthood of each category.
		private List<Vessel> allKnownVessels = new List<Vessel>();
		private UITools UI = new UITools();
		private CommonTools Tools = CommonTools.Instance;
		private DataStorage Data = DataStorage.Instance;

		public VesCat()
		{
			Debug.Log("[VesCat [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: Constructor");
		}

		public void onVesselCreated(Vessel v) 
		{
			ScreenMessages.PostScreenMessage ("VesCat: Vessel " + v.GetName() + " created.");
			if (v.DiscoveryInfo.Level != DiscoveryLevels.Owned) {
				return; // bail out if the vessel isn't owned
			}

			// add it to known vessels
			if (!allKnownVessels.Contains(v)) {
				allKnownVessels.Add (v);
			} 

			// and add it to our list of vessels to 
			if (!vessels.ContainsKey(v.id)) {
				vessels.Add (v.id, Data.ROOT_GUID);
			}
			UI.updateVessels (vessels);
		}

		public void onVesselDestroyed(Vessel v)
		{
			ScreenMessages.PostScreenMessage ("VesCat: Vessel " + v.GetName() + " destroyed.");
			if (v.DiscoveryInfo.Level != DiscoveryLevels.Owned) {
				return; // bail out if the vessel isn't owned
			}

			if (allKnownVessels.Contains(v)) {
				allKnownVessels.Remove (v);
			}

			if (vessels.ContainsKey(v.id)) {
				vessels.Remove (v.id);
			}

			UI.updateVessels (vessels);
		}

		public void UpdateVessels()
		{
			// update our list of known vessels
			allKnownVessels = new List<Vessel> ();
			foreach (Vessel v in FlightGlobals.Vessels.Where(vs => vs.DiscoveryInfo.Level == DiscoveryLevels.Owned)) {
				allKnownVessels.Add (v);
			}

			// first check vesList against vessels to see if we have any new vessels
			foreach (Vessel v in allKnownVessels){
				if (!vessels.ContainsKey(v.id)) {
					vessels.Add (v.id, Data.ROOT_GUID); // if it doesn't exist, add it as a ROOT_GUID sorted vessel.
				}
			}

			// now we check if all the vessels in vessels exist
			List<Guid> toRemove = new List<Guid> ();
			foreach (Guid id in vessels.Keys) {

				if (!allKnownVessels.Exists(vS => vS.id == id)) {
					toRemove.Add (id);
				}
			}

			// finally, remove those that no longer exist from the vessels dictionary.
			foreach(Guid id in toRemove) {
				vessels.Remove (id);
			}

			UI.updateVessels (vessels);
		}

		void Start()
		{
			Debug.Log("[VesCat [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: Start");

			UpdateVessels ();
			foreach(Guid g in vessels.Keys){
				ScreenMessages.PostScreenMessage ("Vessel " + Tools.GetVesselName(g) );
			}

			GameEvents.onVesselCreate.Add (onVesselCreated);
			GameEvents.onVesselDestroy.Add (onVesselDestroyed);

		}

		public override void OnLoad (ConfigNode node)
		{
			ScreenMessages.PostScreenMessage ("VesCat: OnLoad.");
			Debug.Log("[VesCat [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: OnLoad");
			categories = new Dictionary<Guid, string>(); // reinit this so we don't get duplicates.

			// load up all categories from persistent.sfs
			foreach (ConfigNode n in node.GetNodes().Where(n => n.name == "CATEGORIES"))
			{
				foreach (ConfigNode.Value v in n.values) {
					Guid categoryID = new Guid (v.name);
					String categoryName = v.value;

					if (!categories.ContainsKey (categoryID)) {
						categories.Add (categoryID, categoryName);
					}
				}
			}

			// load the category parents.
			foreach (ConfigNode n in node.GetNodes().Where(n => n.name == "CATEGORYPARENTS")) {
				foreach (ConfigNode.Value v in n.values) {
					Guid category = new Guid (v.name);
					Guid parent = new Guid (v.value);

					if (!categories.ContainsKey (parent)) {
						parent = Data.ROOT_GUID;
					}

					categoryParents.Add (category, parent);
				}
			}

			// load all known vessels from persistent.sfs, but verify they exist!
			foreach (ConfigNode n in node.GetNodes().Where(n => n.name == "VESSELS")) 
			{
				foreach (ConfigNode.Value v in n.values) {

					// does the vessel exist?
					Guid vGuid = new Guid (v.name);
					Guid vParent = new Guid (v.value);

					if (!vessels.ContainsKey (vGuid)) {
						if (!FlightGlobals.Vessels.Exists(vS => vS.id == vGuid)) {
							continue;
						}

						if (!categories.ContainsKey (vParent)) {
							vParent = Data.ROOT_GUID;
						}
					
					}
				}
			}
		}

		public override void OnSave (ConfigNode node)
		{

			ScreenMessages.PostScreenMessage ("VesCat: OnSave.");

			// categories
			ConfigNode cat = new ConfigNode ("CATEGORIES");
			foreach (KeyValuePair<Guid,String> entry in categories) {
				cat.AddValue (entry.Key.ToString (), entry.Value);
			}
			// category parents
			ConfigNode catP = new ConfigNode ("CATEGORYPARENTS");
			foreach (KeyValuePair<Guid,Guid> entry in categoryParents) {
				catP.AddValue (entry.Key.ToString (), entry.Value.ToString ());
			}
			// vessels
			ConfigNode vess = new ConfigNode ("VESSELS");
			foreach (KeyValuePair<Guid,Guid> entry in vessels) {
				vess.AddValue(entry.Key.ToString (), entry.Value.ToString ());
			}

			// add the save nodes.
			node.AddNode (cat);
			node.AddNode (catP);
			node.AddNode (vess);

		}

		void OnGUI()
		{
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

