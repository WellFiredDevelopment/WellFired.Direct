using System.Collections.Generic;
using UnityEngine;
using WellFired.Shared;

namespace WellFired
{
	public class PropertyBoxHierarchyItem : IHierarchyItem
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
		
		private PropertyBox property;
		
		public PropertyBoxHierarchyItem(PropertyBox property)
		{
			this.property = property;
		}
		
		public bool Render()
		{
			property.OnGUI();

			return true;
		}
	}
}