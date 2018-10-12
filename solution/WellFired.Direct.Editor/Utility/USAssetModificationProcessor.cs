using UnityEditor;
using UnityEngine;

namespace WellFired
{
	public class USAssetModificationProcessor : UnityEditor.AssetModificationProcessor
	{
		private static string[] OnWillSaveAssets(string[] paths)
		{
	        foreach(var path in paths)
	        {
	            if(path.Contains(".unity"))
				{
					var windows = Resources.FindObjectsOfTypeAll<USWindow>();
					foreach(var window in windows)
						window.RevertForSave();
				}
			}

			return paths;
		}
	}
}