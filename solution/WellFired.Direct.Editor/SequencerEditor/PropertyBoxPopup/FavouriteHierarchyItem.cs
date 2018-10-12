using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WellFired.Shared;

namespace WellFired
{
	public class FavouriteHierarchyItem : IHierarchyItem
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
			get;
			set;
		}

		private PropertyBox propertyBox;
		
		public FavouriteHierarchyItem(PropertyBox propertyBox)
		{
			this.propertyBox = propertyBox;
		}
		
		public bool Render()
		{
			propertyBox.ShouldDisplayComponent = true;
			propertyBox.OnGUI();
			return false;
		}
	}
}