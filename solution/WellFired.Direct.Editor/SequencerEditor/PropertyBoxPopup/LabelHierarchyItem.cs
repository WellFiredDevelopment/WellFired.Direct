using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WellFired.Shared;

namespace WellFired
{
	public class LabelHierarchyItem : IHierarchyItem
	{
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

		private bool IsExpanded
		{
			get 
			{
				var prefsString = string.Format("WellFired.LabelHierarchyItem.{0}", label);
				return EditorPrefs.GetBool(prefsString, true); 
			}
			set
			{
				var prefsString = string.Format("WellFired.LabelHierarchyItem.{0}", label);
				EditorPrefs.SetBool(prefsString, value); 
			}
		}
		
		private string label;
		
		public LabelHierarchyItem(string label)
		{
			this.label = label;
		}
		
		public bool Render()
		{
			if(Children != null && Children.Count > 0)
			{
				IsExpanded = EditorGUILayout.Foldout(IsExpanded, new GUIContent(label));
				return IsExpanded;
			}

			EditorGUILayout.LabelField(new GUIContent(label));
			return false;
		}
	}
}