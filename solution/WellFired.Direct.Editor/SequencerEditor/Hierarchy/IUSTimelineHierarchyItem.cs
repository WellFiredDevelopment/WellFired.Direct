using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WellFired
{
	/// <summary>
	/// This is an abstract class that represents a hierarchy item for the USSequencer window. You will need to extend this if you wish to implement custom rendering 
	/// for a timeline within the uSequencer window.
	/// </summary>
	public abstract class IUSTimelineHierarchyItem : IUSHierarchyItem
	{
		private static readonly float DEFAULT_HEIGHT = 17.0f;
		private static readonly float ITEM_HEIGHT_STEP = 17.0f;
		private static readonly float MIN_HEIGHT = 17.0f;
		private static readonly float MAX_HEIGHT = 400.0f;

		[SerializeField]
		public USTimelineBase baseTimeline;
		public USTimelineBase BaseTimeline
		{
			get { return baseTimeline; }
			set { baseTimeline = value; }
		}

		/// <summary>
		/// Returns the object that will be pingable if the user is to click on this timeline in the uSequencer hierarchy.
		/// </summary>
		/// <value>The pingable object.</value>
		public virtual Transform PingableObject
		{
			get { return BaseTimeline.transform; }
			set { ; }
		}

		public USTimelineBase TimelineToDestroy()
		{
			return BaseTimeline;
		}
		
		public bool IsForThisTimeline(USTimelineBase timeline)
		{
			return BaseTimeline == timeline;
		}

		/// <summary>
		/// If you need custom initialization code for your Timeline, include it here.
		/// </summary>
		/// <param name="timeline">the Timeline.</param>
		public override void Initialize(USTimelineBase timeline)
		{
			base.Initialize(timeline);
			BaseTimeline = timeline;
		}
		
		#region Shirk / Grow
		/// <summary>
		/// Should return the height of this hierarchy item in the hierarchy
		/// </summary>
		/// <value>The height.</value>
		private float itemHeight;
		protected float ItemHeight
		{
			get 
			{ 
				var prefsString = string.Format("WellFired.IUSHierarchyItem.{0}.{1}.{2}", Shared.Project.Name, BaseTimeline.Sequence.name, BaseTimeline.GetJson());
				if(itemHeight <= 0.0f)
					itemHeight = EditorPrefs.GetFloat(prefsString, DefaultItemHeight);

				return IsExpanded == false ? MinHeight : itemHeight;
			}
			set 
			{
				value = Mathf.Clamp(value, MinHeight, MaxHeight);
				var prefsString = string.Format("WellFired.IUSHierarchyItem.{0}.{1}.{2}", Shared.Project.Name, BaseTimeline.Sequence.name, BaseTimeline.GetJson());

				itemHeight = value;
				EditorPrefs.SetFloat(prefsString, itemHeight);
			} 
		}
		
		/// <summary>
		/// Should return the starting height of your hierarchy item, if you want to change it from it's default
		/// </summary>
		/// <value>The default height of the item.</value>
		public virtual float DefaultItemHeight { get { return DEFAULT_HEIGHT; } }
		
		/// <summary>
		/// If you return true from this property, your item will be able to increase in size
		/// </summary>
		/// <value><c>true</c> if should display shrink button; otherwise, <c>false</c>.</value>
		public virtual bool ShouldDisplayShrinkButton { get; set; }
		
		/// <summary>
		/// If you return true from this property, your item will be able to decrease in size
		/// </summary>
		/// <value><c>true</c> if should display shrink button; otherwise, <c>false</c>.</value>
		public virtual bool ShouldDisplayGrowButton { get; set; }
		
		private void Shrink()
		{
			ItemHeight -= ItemHeightStep;
		}
		
		private void Grow()
		{
			ItemHeight += ItemHeightStep;
		}
		
		private float MinHeight { get { return MIN_HEIGHT; } }
		private float MaxHeight { get { return MAX_HEIGHT; } }
		protected float ItemHeightStep { get { return ITEM_HEIGHT_STEP; } }
		
		protected override void FloatingOnGUI(int depth, Rect parentRect)
		{
			if(IsExpanded && ShouldDisplayShrinkButton)
			{
				var size = 16.0f;
				var moreButtonRect = new Rect(parentRect.width - (size * 3.0f), parentRect.y, size, size);
				
				if(GUI.Button(moreButtonRect, new GUIContent(USEditorUtility.ShrinkButton, "Should we Shrink this timeline?"), USEditorUtility.ButtonNoOutline))
					Shrink();
			}
			
			if(IsExpanded && ShouldDisplayGrowButton)
			{
				var size = 16.0f;
				var moreButtonRect = new Rect(parentRect.width - (size * 2.0f), parentRect.y, size, size);
				
				if(GUI.Button(moreButtonRect, new GUIContent(USEditorUtility.GrowButton, "Should we Grow this timeline?"), USEditorUtility.ButtonNoOutline))
					Grow();
			}

			base.FloatingOnGUI(depth, parentRect);
		}
		#endregion
	}
}