using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace WellFired.Shared
{
	public interface IUnityEditorHelper
	{
		void AddUpdateListener(Action listener);
		void RemoveUpdateListener(Action listener);
		bool IsPrefab(GameObject testObject);
	}
}