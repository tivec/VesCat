using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VesCat
{
	public sealed class CommonTools
	{
		static readonly CommonTools _instance = new CommonTools ();

		public static CommonTools Instance {
			get {
				return _instance;
			}
		}

		CommonTools()
		{
		}

		public String GetVesselName(Guid id)
		{
			Vessel v = FlightGlobals.Vessels.Find (vs => vs.id == id);
			if (v != null) {
				return v.GetName ();
			}

			return "BUG: Unknown vessel Guid("+id.ToString()+")";
		}

		public Vessel GetVessel(Guid id)
		{
			Vessel v = FlightGlobals.Vessels.Find (vs => vs.id == id);
			if (v != null) {
				return v;
			}
			throw new NullReferenceException("In GetVessel, Guid " + id.ToString() + " does not match a vessel.");
		}
	}
}

