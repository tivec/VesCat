using System;
using System.Linq;
using System.Reflection;
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


		// Code for this is based off of how HaystackContinued (https://qberticus/HaystackContinued) does camera focus.
		public static void FocusMapCamera(Vessel v) 
		{
			if (HighLogic.LoadedScene == GameScenes.TRACKSTATION) {
				var trackObject = (SpaceTracking)UnityEngine.Object.FindObjectOfType (typeof(SpaceTracking));
				var method = trackObject.GetType ().GetMethod ("RequestVessel", BindingFlags.NonPublic | BindingFlags.Instance);
				method.Invoke (trackObject, new object[] { v });
			} else if (HighLogic.LoadedScene == GameScenes.FLIGHT && MapView.MapIsEnabled) {
				PlanetariumCamera camera = MapView.MapCamera;
				foreach (var obj in camera.targets)
				{
					if (obj.vessel != null && obj.vessel.GetInstanceID() == v.GetInstanceID()) {
						camera.SetTarget(obj);
					}
				}
			}
		}

		// Code for this is based off of how HaystackContinued (https://qberticus/HaystackContinued) switches vessels
		public static void TrackingCenterSwitchTo(Vessel v)
		{
			var trackObject = (SpaceTracking) UnityEngine.Object.FindObjectOfType (typeof(SpaceTracking));
			var method = trackObject.GetType ().GetMethod ("BoardVessel", BindingFlags.NonPublic | BindingFlags.Instance);
			method.Invoke (trackObject, new object[] { v });
		}



	}
}

