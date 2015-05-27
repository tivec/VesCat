using System;
using UnityEngine;

namespace VesCat
{

	enum uiStyles
	{
		categoryButton,
		vesselButton
	}

	static class UIStyle
	{
		public static GUISkin UISkin;

		public static void CustomSkin()
		{
			UISkin = (GUISkin)MonoBehaviour.Instantiate (UnityEngine.GUI.skin);
			UISkin.customStyles = new GUIStyle[Enum.GetValues (typeof(uiStyles)).GetLength (0)];


			UISkin.customStyles [(int)uiStyles.categoryButton] = new GUIStyle (UISkin.button);
			//UISkin.customStyles [(int)uiStyles.categoryButton].margin = new RectOffset (0, 4, 0, 2);
			UISkin.customStyles [(int)uiStyles.categoryButton].normal.textColor = XKCDColors.DarkCyan;
			UISkin.customStyles [(int)uiStyles.categoryButton].hover.textColor = XKCDColors.GreenishCyan;
			UISkin.customStyles [(int)uiStyles.categoryButton].focused.textColor = XKCDColors.BrightCyan;
			UISkin.customStyles [(int)uiStyles.categoryButton].alignment = TextAnchor.MiddleLeft;

			UISkin.customStyles [(int)uiStyles.vesselButton] = new GUIStyle (UISkin.button);
			UISkin.customStyles [(int)uiStyles.vesselButton].alignment = TextAnchor.MiddleLeft;

		}
	}
}

