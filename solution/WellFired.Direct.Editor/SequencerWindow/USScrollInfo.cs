using UnityEngine;
using System;
using System.Collections;

namespace WellFired
{
	[Serializable]
	public class USScrollInfo : ScriptableObject
	{
		[SerializeField]
		public Vector2 currentScroll = Vector2.zero;
		[SerializeField]
		public Vector2 visibleScroll = Vector2.one;
		
		private void OnEnable() { hideFlags = HideFlags.HideAndDontSave; }
		
		public void Reset()
		{
			currentScroll = Vector2.zero;
			visibleScroll = Vector2.one;
		}
	}
}