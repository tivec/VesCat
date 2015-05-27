using System;
using UnityEngine;

namespace VesCat
{
	public static class UITools
	{
		public static void DrawCategoryButton(String text, Color background, bool enable, Action onEnable, Action onDisable, Action onRightClick = null, params GUILayoutOption[] options) {
			Color defaultBG = GUI.backgroundColor;

			if (enable) {
				GUI.backgroundColor = background;
				if (GUILayout.Button(text, UIStyle.UISkin.customStyles [(int)uiStyles.categoryButton], options)) {
					if (onRightClick != null && Event.current.button == 1) {
						onRightClick ();
					} else {
						onDisable ();
					}
				}
			} else {
				if (GUILayout.Button(text, UIStyle.UISkin.customStyles [(int)uiStyles.categoryButton], options)) {
					if (onRightClick != null && Event.current.button == 1) {
						onRightClick ();
					} else {
						onEnable ();
					}
				}
			}

			GUI.backgroundColor = defaultBG;
		}
	}
}

