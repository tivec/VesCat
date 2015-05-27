using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VesCat
{
	public sealed class DataStorage
	{

		static readonly DataStorage _instance = new DataStorage();

		public static Guid ROOT_GUID = new Guid("142599e6-0f50-4994-a7f8-6474e9893acc");

		private Dictionary<Guid,Guid> vessels = new Dictionary<Guid, Guid>();		// A dictionary of all known vessels
		private Dictionary<Guid,String> categories = new Dictionary<Guid,String>(); // all categories the player has created
		private Dictionary<Guid,Guid> categoryParents = new Dictionary<Guid,Guid>(); // the parenthood of each category.
		private List<Vessel> allKnownVessels = new List<Vessel>();



		public static DataStorage Instance {
			get {
				return _instance;
			}
		}

		public DataStorage ()
		{
		}

		public Dictionary<Guid, Guid> Vessels {
			get {
				return this.vessels;
			}
			set {
				vessels = value;
			}
		}

		public Dictionary<Guid, string> Categories {
			get {
				return this.categories;
			}
			set {
				categories = value;
			}
		}

		public Dictionary<Guid, Guid> CategoryParents {
			get {
				return this.categoryParents;
			}
			set {
				categoryParents = value;
			}
		}

		public List<Vessel> AllKnownVessels {
			get {
				return this.allKnownVessels;
			}
		}

		/* This function adds a vessel to the known vessels list. It also makes certain the vessel has a pairing */
		public void AddVessel(Vessel v) {
			if (v.DiscoveryInfo.Level != DiscoveryLevels.Owned) {
				return; // bail out if the vessel isn't owned
			}

			// add it to known vessels
			if (!allKnownVessels.Contains(v)) {
				allKnownVessels.Add (v);
			} 

			// and add it to our list of vessels to 
			if (!vessels.ContainsKey(v.id)) {
				vessels.Add (v.id, ROOT_GUID);
			}
		}

		/* This function removes a vessel from the known vessels list. It also forgets about pairing. */
		public void RemoveVessel(Vessel v) {
			if (allKnownVessels.Contains(v)) {
				allKnownVessels.Remove (v);
			}

			if (vessels.ContainsKey(v.id)) {
				vessels.Remove (v.id);
			}
		}

		public void UpdateVessels() {
			foreach (Vessel v in FlightGlobals.Vessels.Where(vs => vs.DiscoveryInfo.Level == DiscoveryLevels.Owned)) {
				AddVessel (v);
			}

			// now we check if all the vessels in vessels exist
			List<Guid> toRemove = new List<Guid> ();
			foreach (Guid id in Vessels.Keys) {
				if (!FlightGlobals.Vessels.Exists(vS => vS.id == id)) {
					toRemove.Add (id);
				}
			}

			// finally, remove those that no longer exist from the vessels dictionary.
			foreach(Guid id in toRemove) {
				Vessels.Remove (id);
			}
		}

		public void AddCategory(string category) {
			Guid id = Guid.NewGuid ();
			// add the category, and set its parent to root
			Categories.Add (id, category);
			CategoryParents.Add (id, ROOT_GUID);
		}

		public string GetCategoryName(Guid id) {
			if (!Categories.ContainsKey(id)) {
				return "BUG: " + id + " has no entry.";
			}
			return Categories [id];
		}

		// returns the parent of the given Guid
		public Guid GetParent(Guid id) {
			if (CategoryParents.ContainsKey(id)) {
				return CategoryParents[id];
			} else if (Vessels.ContainsKey(id)) {
				return Vessels [id];
			}
			return ROOT_GUID;
		}

		public void SetParent(Guid id, Guid parent) {
			if ((!Categories.ContainsKey (parent) && parent != ROOT_GUID) || id == parent) {
				// we can't assign to unknown categories, or other vessels
				return;
			} else {
				if (Categories.ContainsKey (id)) {
					Debug.Log ("Assigning parent category " + parent + " to category " + id);
					CategoryParents [id] = parent;
				} else if (Vessels.ContainsKey (id)) {
					Debug.Log ("Assigning parent category " + parent + " to vessel " + id);
					Vessels [id] = parent;
				}
			}
		}
	}
}

