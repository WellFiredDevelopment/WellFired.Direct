using UnityEngine;
using System.Collections;
using System;

namespace WellFired.Shared
{
	public class VersionInformation : System.Object, IComparable
	{
		private static readonly string INVALID_VERSION = "Unknown";

		public static VersionInformation InvalidVersion()
		{
			var newVersionInfo = new VersionInformation();
			newVersionInfo.FullVersionString = INVALID_VERSION;
			return newVersionInfo;
		}

		public static VersionInformation BuildFrom(string versionInformation)
		{
			var newVersionInfo = new VersionInformation();
			newVersionInfo.IsBeta = versionInformation.Contains("b");
			var versionInformationWithoutBeta = versionInformation;

			if(newVersionInfo.IsBeta)
			{
				var betaIndex = versionInformation.IndexOf("b");
				versionInformationWithoutBeta = versionInformationWithoutBeta.Remove(betaIndex, versionInformation.Length - betaIndex);
			}

			var splitVersion = versionInformationWithoutBeta.Split(new Char[] {'.'}, StringSplitOptions.RemoveEmptyEntries);

			if(splitVersion.Length > 0)
				newVersionInfo.Major = Convert.ToInt32(splitVersion[0]);
			if(splitVersion.Length > 1)
				newVersionInfo.Minor = Convert.ToInt32(splitVersion[1]);
			if(splitVersion.Length > 2)
				newVersionInfo.Revision = Convert.ToInt32(splitVersion[2]);

			newVersionInfo.FullVersionString = versionInformation;

			return newVersionInfo;
		}

		public string FullVersionString { get; set; }
		public int Major { get; set; }
		public int Minor { get; set; }
		public int Revision { get; set; }
		public bool IsBeta { get; set; }

		public int CompareTo(System.Object other)
		{
			if(other == null)
				return 1;
			
			var otherVersionInformation = other as VersionInformation;
			if(otherVersionInformation == null)
				return 1;

			if(Major > otherVersionInformation.Major)
				return 1;
			else if(Major < otherVersionInformation.Major)
				return -1;

			if(Minor > otherVersionInformation.Minor)
				return 1;
			else if(Minor < otherVersionInformation.Minor)
				return -1;
			
			if(Revision > otherVersionInformation.Revision)
				return 1;
			else if(Revision < otherVersionInformation.Revision)
				return -1;

			if(IsBeta && !otherVersionInformation.IsBeta)
				return 1;
			else if(!IsBeta && otherVersionInformation.IsBeta)
				return -1;

			return 0;
		}
		
		public override bool Equals(System.Object other)
		{
			if(other == null)
				return false;

			var otherVersionInformation = other as VersionInformation;
			if((System.Object)otherVersionInformation == null)
				return false;

			return CompareTo(otherVersionInformation) == 0;
		}
		
		public bool Equals(VersionInformation other)
		{
			if((object)other == null)
				return false;
			
			return CompareTo(other) == 0;
		}
		
		public override int GetHashCode()
		{
			return Major ^ Minor ^ Revision ^ Convert.ToInt32(IsBeta);
		}
	}
}