using System.Collections.Generic;
using UnityEngine;

namespace WellFired.Shared
{
	/// <summary>
	/// A lightweight hierarchy. 
	/// Each item can have children.
	/// Each item can OnGUI.
	/// Each item can OnClick.
	/// </summary>
	public interface IHierarchyItem 
	{
		Rect RenderRect
		{
			get;
			set;
		}

		List<IHierarchyItem> Children
		{
			get;
			set;
		}

		System.Action<IHierarchyItem> OnClick
		{
			get;
			set;
		}

		bool Render();
	}
}