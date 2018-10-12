using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WellFired.Shared
{
	/// <summary>
	/// A lightweight hierarchy. 
	/// Each item can have children.
	/// Each item can OnGUI.
	/// Each item can OnClick.
	/// Each item must implement the IHierarchyItem interface
	/// </summary>
	public class Hierarchy 
	{
		private List<IHierarchyItem> RootObjects
		{
			get;
			set;
		}

		private Rect VisibleArea 
		{
			get;
			set;
		}
		
		private float itemWidth;
		private float itemHeight;
		private float offset;
		private float offsetStep = 20.0f;

		private Vector2 scrollPos;
		private EditorWindow containingWindow;

		private Hierarchy() { }

		public Hierarchy(float itemWidth, float itemHeight, EditorWindow containingWindow)
		{
			this.containingWindow = containingWindow;
			this.itemWidth = itemWidth;
			this.itemHeight = itemHeight;
		}

		public void AddChild(IHierarchyItem hierarchyItem)
		{
			if(RootObjects == null)
				RootObjects = new List<IHierarchyItem>();

			RootObjects.Add(hierarchyItem);
		}

		public void OnGUI()
		{
			offset = 0.0f;

			using(var scrollView = new GUIBeginScrollView(scrollPos, false, true, GUIStyle.none, GUI.skin.verticalScrollbar))
			{
				using(new GUIBeginVertical())
				{
					foreach(var entry in RootObjects)
						DrawEntry(entry);
				}

				scrollPos = scrollView.Scroll;
			}
		}

		private void DrawEntry(IHierarchyItem entry)
		{
			GUILayout.Label("", GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
			if(Event.current.type == EventType.Repaint)
			{
				entry.RenderRect = GUILayoutUtility.GetLastRect();
				entry.RenderRect = new Rect(offset, entry.RenderRect.y, itemWidth - offset, entry.RenderRect.height);
				containingWindow.Repaint();
			}

			var shouldRenderChildren = false;
			using(new GUIBeginArea(entry.RenderRect))
				shouldRenderChildren = entry.Render();

			var clickableRect = entry.RenderRect;
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && clickableRect.Contains(Event.current.mousePosition))
			{
				if(entry.OnClick != null)
					entry.OnClick(entry);
			
				Event.current.Use();
			}
			
			if(entry.Children != null && shouldRenderChildren)
			{
				offset += offsetStep;
			
				foreach(var child in entry.Children)
					DrawEntry(child);
			
				offset -= offsetStep;
			}
		}
	}
}