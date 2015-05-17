using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace VesCat
{
	[KSPScenario(ScenarioCreationOptions.AddToAllGames,GameScenes.TRACKSTATION,GameScenes.FLIGHT)] 
	public class VesCat : ScenarioModule
	{
		public static Guid ROOT_GUID = new Guid("142599e6-0f50-4994-a7f8-6474e9893acc");

		private Dictionary<Guid,Guid> vessels = new Dictionary<Guid, Guid>();		// A dictionary of all known vessels
		private Dictionary<Guid,String> categories = new Dictionary<Guid,String>(); // all categories the player has created
		private Dictionary<Guid,Guid> categoryParents = new Dictionary<Guid,Guid>(); // the parenthood of each category.

		public VesCat()
		{
			Debug.Log("[VesCat [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: Constructor");
		}

		void Start()
		{
			Debug.Log("[VesCat [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: Start");

			// iterate all vessels that the player owns, and see if they need to be added to the vessels list
			foreach(Vessel v in FlightGlobals.Vessels.Where(vs => vs.DiscoveryInfo.Level == DiscoveryLevels.Owned))
			{
				Debug.Log ("[VesCat] Found vessel " + v.name + " (" + v.id + ")");
				if (!vessels.ContainsKey (v.id)) {
					vessels.Add (v.id, ROOT_GUID); // if it doesn't exist, add it as a ROOT_GUID sorted vessel.
				}
			}


		}

		public override void OnLoad (ConfigNode node)
		{
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
						parent = ROOT_GUID;
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
							vParent = ROOT_GUID;
						}
					
					}
				}
			}
		}

		public override void OnSave (ConfigNode node)
		{
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

			node.AddNode (cat);
			node.AddNode (catP);
			node.AddNode (vess);
		}

		public override void OnAwake ()
		{
			Debug.Log("[VesCat [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: OnAwake");
		}

		void OnDestroy()
		{
			Debug.Log ("[VesCat [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: OnDestroy");
		}

	}
}

