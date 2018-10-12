using UnityEngine;

namespace WellFired.Shared
{
	public static class Project 
	{
		public static string Name
		{
			get
			{
				var s = Application.dataPath.Split('/');
				var projectName = s[s.Length - 2];
				return projectName;
			}
		}
	}
}