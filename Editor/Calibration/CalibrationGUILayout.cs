using System;
using UnityEngine;
using UnityEditor;

namespace HTW.CAVE.Calibration
{
	internal static class CalibrationGUILayout
	{
		public static GUIStyle lowerToolbarStyle;
		
		public static GUIStyle scrollViewButtonStyle;

		public static GUIStyle scrollViewPopupStyle;
		
		public static GUIStyle scrollViewSelectedButtonStyle;
		
		static CalibrationGUILayout()
		{
			lowerToolbarStyle = new GUIStyle(EditorStyles.toolbar);
			lowerToolbarStyle.fixedHeight = 40f;

			scrollViewButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
			scrollViewButtonStyle.fontStyle = EditorStyles.label.fontStyle;
			scrollViewButtonStyle.fontSize = EditorStyles.label.fontSize;
			scrollViewButtonStyle.alignment = TextAnchor.MiddleLeft;
			scrollViewButtonStyle.fixedHeight = 30f;

			scrollViewPopupStyle = new GUIStyle(EditorStyles.toolbarPopup);
			scrollViewPopupStyle.fixedHeight = 30f;
			
			scrollViewSelectedButtonStyle = new GUIStyle(scrollViewButtonStyle);
			scrollViewSelectedButtonStyle.fontStyle = FontStyle.Bold;
		}
	
		public static void BeginToolbar() => 
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
	
		public static void EndToolbar() => EditorGUILayout.EndHorizontal();
	
		public static bool ToolbarButton(string text, float maxWidth) =>
			GUILayout.Button(text, EditorStyles.toolbarButton, GUILayout.MaxWidth(maxWidth));
		
		public static int ToolbarPopup(int index, string[] options, float width) =>
			EditorGUILayout.Popup(index, options, EditorStyles.toolbarPopup, GUILayout.Width(width - 1));
	
		public static string ToolbarTextField(string text) =>
			EditorGUILayout.TextField(text, EditorStyles.toolbarTextField);
	
		public static void BeginLowerToolbar()
		{
			EditorGUILayout.BeginVertical(lowerToolbarStyle);
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
		}

		public static void EndLowerToolbar()
		{
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
		
		public static Vector2 BeginScrollView(Vector2 scrollPosition) =>
			EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
		
		public static void EndScrollView() => EditorGUILayout.EndScrollView();
		
		public static void ScrollViewDummy(float width)
		{
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.ExpandHeight(true));
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndVertical();
			DrawScrollViewBorder();
		}
		
		public static void ScrollViewFlexibleSpace()
		{
			EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndVertical();
			DrawScrollViewBorder();
		}
		
		private static void DrawScrollViewBorder()
		{
			var rect = GUILayoutUtility.GetLastRect();
			rect.x = rect.x + rect.width - 1;
			rect.width = 1;
			EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 1f));
		}
		
		public static bool ScrollViewButton(string text, bool selected = false, params GUILayoutOption[] layouts) =>
			GUILayout.Button(text, selected ? scrollViewSelectedButtonStyle : scrollViewButtonStyle,
				layouts);

		public static int ScrollViewPopup(int index, string[] options, params GUILayoutOption[] layouts) =>
			EditorGUILayout.Popup(index, options, scrollViewPopupStyle, layouts);
		
		public static void ScrollViewSeperator()
		{
			var rect = EditorGUILayout.GetControlRect(false, 1);
			rect.x -= 1;
			rect.width += 8;
			EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 1f));
		}
		
		public static VirtualCamera.Quad QuadField(VirtualCamera.Quad quad)
		{
			var wide = EditorGUIUtility.wideMode;
			var width =  EditorGUIUtility.labelWidth;
			
			EditorGUIUtility.wideMode = true;
			EditorGUIUtility.labelWidth -= 70f;

			EditorGUILayout.BeginHorizontal();
			quad.topLeft = EditorGUILayout.Vector2Field("Top Left", quad.topLeft, GUILayout.MaxWidth(300f));
			quad.topRight = EditorGUILayout.Vector2Field("Top Right", quad.topRight, GUILayout.MaxWidth(300f));
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();
			quad.bottomLeft = EditorGUILayout.Vector2Field("Bottom Left", quad.bottomLeft, GUILayout.MaxWidth(300f));
			quad.bottomRight = EditorGUILayout.Vector2Field("Bottom Right", quad.bottomRight, GUILayout.MaxWidth(300f));
			EditorGUILayout.EndHorizontal();
			
			EditorGUIUtility.wideMode = wide;
			EditorGUIUtility.labelWidth = width;
			
			return quad;
		}
	}
}