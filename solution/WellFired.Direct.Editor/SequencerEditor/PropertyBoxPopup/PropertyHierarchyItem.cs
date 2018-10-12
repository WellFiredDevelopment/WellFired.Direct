using System.Collections.Generic;
using UnityEngine;
using WellFired.Shared;

namespace WellFired
{
	public class PropertyHierarchyItem : IHierarchyItem
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

		private PropertyBox propertyBox;

		public PropertyHierarchyItem(PropertyBox propertyBox)
		{
			this.propertyBox = propertyBox;
		}
		
		public bool Render()
		{
			propertyBox.ShouldDisplayComponent = false;
			propertyBox.OnGUI();

			return true;
		}
	}
}