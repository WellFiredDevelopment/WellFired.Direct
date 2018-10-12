using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace WellFired.Shared
{
	public static class DirectoryExtensions 
	{
		private static readonly string DEFAULT_INSTALLATION_DIRECTORY = string.Format("{0}/WellFired", Application.dataPath);
		private static readonly string PROJECT_PATH = Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets"), "/Assets".Length);
		
		public static string ProjectPath
		{
			get { return PROJECT_PATH; }
			private set { ; }
		}

		public static string DefaultInstallationDirectory
		{
			get { return DEFAULT_INSTALLATION_DIRECTORY; }
			private set { ; }
		}
		
		public static string DirectoryOf(string file)
		{
			var result = string.Empty;
			var fileList = new DirectoryInfo(ProjectPath + "/Assets").GetFiles(file, SearchOption.AllDirectories); 
			if(fileList.Length == 1)
			{
				result = fileList[0].DirectoryName.Substring(ProjectPath.Length, fileList[0].DirectoryName.Length - ProjectPath.Length).Replace('\\', '/') + '/';
			}
			else
			{
				Debug.LogWarning(string.Format("Cannot find Directory of {0}", file));
				return string.Empty;
			}

			return result;
		}
		
		public static string RelativePathOfProjectFile(string projectFile)
		{
			var directory = DirectoryOf(projectFile);
			return string.Format("{0}{1}", directory, projectFile);
		}
		
		public static string AbsolutePathOfProjectFile(string projectFile)
		{
			var relativePath = RelativePathOfProjectFile(projectFile);
			var path = string.Format("{0}{1}", ProjectPath, relativePath);
			
			if(Application.platform == RuntimePlatform.OSXEditor)
				return path;
			
			return SlashesToWindowsSlashes(path);
		}
		
		/// <summary>
		/// Converts all slashes in the given paths to windows backslashes.	
		/// the given array.
		/// </summary>
		/// <param name='path'>
		/// the path
		/// </param>
		public static string SlashesToWindowsSlashes(string path) 
		{
			return path.Replace("/", "\\");
		}
		
		/// <summary>
		/// Converts all backslashes in the given paths to forward slashes. The conversion is done in-place and modifies the
		/// given array.
		/// </summary>
		/// <param name='paths'>
		/// the array of paths.
		/// </param>
		public static void NormalizeSlashes(string[] paths) 
		{
			for(var i = 0; i < paths.Length; i++)
				paths[i] = NormalizeSlashes(paths[i]);
		}
		
		/// <summary>
		/// Converts all backslashes in the given path to forward slashes.
		/// </summary>
		/// <returns>
		/// The normalized path.
		/// </returns>
		/// <param name='path'>
		/// The path to normalize.
		/// </param>
		public static string NormalizeSlashes(string path) 
		{
			return Regex.Replace(path.Replace("\\", "/"), "//+", "/");
		}
		
		public static void RecursivelyDeleteDirectory(string directoryToDelete)
		{
			var directories = Directory.GetDirectories(directoryToDelete);
			foreach(var directory in directories)
			{
				RecursivelyDeleteDirectory(directory);
			}
			
			var files = Directory.GetFiles(directoryToDelete);
			foreach(var file in files)
			{
				File.Delete(file);
			}
			
			Directory.Delete(directoryToDelete);
		}
		
		public static void CopyDirectory(string sourceDirectory, string destinationDirectory)
		{
			//Now Create all of the directories
			foreach(var dirPath in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
			{
				Directory.CreateDirectory(dirPath.Replace(sourceDirectory, destinationDirectory));
			}
			
			//Copy all the files & Replaces any files with the same name
			foreach(var newPath in Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories))
			{
				File.Copy(newPath, newPath.Replace(sourceDirectory, destinationDirectory), true);
			}
		}
		
		public static void RemoveAllEmptySubDirectories(string startLocation)
		{ 
			foreach(var directory in Directory.GetDirectories(startLocation))
			{
				RemoveAllEmptySubDirectories(directory);
				if(Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
				{
					Directory.Delete(directory, false);
					File.Delete(string.Format("{0}.meta", directory));
				}
			}
		}
	}
}