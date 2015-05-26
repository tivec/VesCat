using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VesCat
{
	public sealed class DataStorage
	{

		static readonly DataStorage _instance = new DataStorage();

		private Guid root_guid = new Guid("142599e6-0f50-4994-a7f8-6474e9893acc");

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

		public Guid ROOT_GUID {
			get {
				return root_guid;
			}
		}

		public Dictionary<Guid, string> Categories {
			get {
				return this.categories;
			}
		}

		public Dictionary<Guid, Guid> CategoryParents {
			get {
				return this.categoryParents;
			}
		}

		public List<Vessel> AllKnownVessels {
			get {
				return this.allKnownVessels;
			}
		}
		public Dictionary<Guid, Guid> Vessels {
			get {
				return vessels;
			}
		}

		public void AddVessel(Vessel v) {

		}

	}
}

