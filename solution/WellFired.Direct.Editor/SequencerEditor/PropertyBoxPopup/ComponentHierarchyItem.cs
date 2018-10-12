using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WellFired.Shared;

namespace WellFired
{
	public class ComponentHierarchyItem : IHierarchyItem
	{
		private bool IsExpanded
		{
			get 
			{
				var prefsString = string.Format("WellFired.ComponentHierarchyItem.{0}", component.GetType());
				return EditorPrefs.GetBool(prefsString, false); 
			}
			set
			{
				var prefsString = string.Format("WellFired.ComponentHierarchyItem.{0}", component.GetType());
				EditorPrefs.SetBool(prefsString, value); 
			}
		}

		public Rect RenderRect
		{
			get;
			set;
		}
		
		public List<IHierarchyItem> Children
		{
			get;
			set;
		}
		
		public System.Action<IHierarchyItem> OnClick
		{
			get;
			set;
		}

		private Component component;

		private ComponentHierarchyItem() { }

		public ComponentHierarchyItem(Component component, List<PropertyBox> propertyBoxes)
		{
			this.component = component;

			Children = new List<IHierarchyItem>();
			foreach(var propertyBox in propertyBoxes)
				Children.Add(new PropertyHierarchyItem(propertyBox));
		}

		public bool Render()
		{
			var displayableContent = new GUIContent(component.GetType().Name, EditorGUIUtility.ObjectContent(component, component.GetType()).image);
			IsExpanded = EditorGUILayout.Foldout(IsExpanded, displayableContent);

			return IsExpanded;
		}
	}
}