using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace WellFired
{
	/// <summary>
	/// This is at the core of our Property timelines. It contains all property data and updates / processes it.
	/// </summary>
	[Serializable]
	public class USTimelineProperty : USTimelineBase
	{
		#region Member Variables
		[SerializeField]
		private List<USPropertyInfo> propertyList = new List<USPropertyInfo>();
		#endregion
		
		#region Properties
		/// <summary>
		/// Gets the list of properties.
		/// </summary>
		public List<USPropertyInfo> Properties
		{
			get
			{
				return propertyList;
			}
		}
		#endregion

		public override void StartTimeline()
		{
			TryToFixComponentReferences();
		}
		
		public bool HasPropertiesForComponent(Component component)
		{
			foreach(var property in propertyList)
			{
				if(component == property.Component)
					return true;
			}
			
			return false;
		}
		
		public USPropertyInfo GetProperty(string propertyName, Component component)
		{
			foreach(var property in propertyList)
			{
				if(property == null)
					continue;

				var propertyComponentType = USRuntimeUtility.ConvertToSerializableName(property.ComponentType);
				var componentType = USRuntimeUtility.ConvertToSerializableName(component.GetType().ToString());
				
				if(propertyComponentType == componentType && (property.propertyName == propertyName || property.fieldName == propertyName))
					return property;
			}
			
			return null;
		}
		
		public bool ContainsProperty(string propertyName, Component component)
		{
			foreach(var property in propertyList)
			{
				if(property == null)
					continue;
				
				if(property.Component == component && (property.propertyName == propertyName || property.fieldName == propertyName))
					return true;
			}
			
			return false;
		}
		
		public void AddProperty(USPropertyInfo propertyInfo)
		{
			if(propertyInfo.propertyName != null && ContainsProperty(propertyInfo.propertyName, propertyInfo.Component))
				throw new Exception("Cannot Add a property that we already have");
			
			if(propertyInfo.fieldName != null && ContainsProperty(propertyInfo.fieldName, propertyInfo.Component))
				throw new Exception("Cannot Add a field that we already have");
			
			propertyList.Add(propertyInfo);
		}
		
		public void RemoveProperty(USPropertyInfo propertyInfo)
		{	
			propertyList.Remove(propertyInfo);
		}
		
		public void ClearProperties()
		{
			propertyList.Clear();
		}
	
		public override void SkipTimelineTo(float time)
		{
			Process(time, 1.0f);
		}
		
		public override void Process(float sequencerTime, float playbackRate) 
		{
			if(!AffectedObject)
				return;

			for(var propertyIndex = 0; propertyIndex < propertyList.Count; propertyIndex++)
			{
				var property = propertyList[propertyIndex];

				if(property != null)
				{
					if(!property.Component)
						property.Component = AffectedObject.GetComponent(property.ComponentType);
					
					if(property.Component && property.Component.transform != AffectedObject)
						property.Component = AffectedObject.GetComponent(property.ComponentType);
						
					property.SetValue(sequencerTime);
				}
			}
		}
	
		public void TryToFixComponentReferences()
		{
			if(!AffectedObject)
				return;	

			foreach(var property in propertyList)
			{
				if(property != null)
				{
					if(!property.Component)
						property.Component = AffectedObject.GetComponent(property.ComponentType);
					
					if(property.Component && property.Component.transform != AffectedObject)
						property.Component = AffectedObject.GetComponent(property.ComponentType);
				}
			}
		}

		private void OnDrawGizmos()
		{
			if(!ShouldRenderGizmos)
				return;

			foreach(var property in Properties)
			{
				if(property == null || property.Component == null || property.Component.GetType() != typeof(Transform))
					continue;
				
				if(property.PropertyName != "localPosition" && property.PropertyName != "position")
					continue;
				
				var min = Mathf.Infinity;
				var max = 0.0f;
				foreach(var curve in property.curves)
				{
					min = Math.Min(min, curve.FirstKeyframeTime);
					max = Math.Max(max, curve.LastKeyframeTime);
				}
				var sampleRate = (max - min) / 40.0f;
				var start = min;
				while(start < max)
				{
					var prevSampledValue = (Vector3)property.GetValueForTime(start);
					var nextSampledValue = (Vector3)property.GetValueForTime(start + sampleRate);
					start += sampleRate;
					
					var parent = AffectedObject.transform.parent;
					if(property.PropertyName == "localPosition" && parent != null)
					{
						prevSampledValue = parent.TransformPoint(prevSampledValue);
						nextSampledValue = parent.TransformPoint(nextSampledValue);
					}
					
					Gizmos.DrawLine(prevSampledValue, nextSampledValue);
				}
			}
		}

		public override void LateBindAffectedObjectInScene (Transform newAffectedObject)
		{
			foreach(var property in propertyList)
			{
				if(property != null && property.Component == null)
					property.Component = AffectedObject.GetComponent(property.ComponentType);
			}

			base.LateBindAffectedObjectInScene (newAffectedObject);
		}

		public override string GetJson()
		{
			var propertiesString = string.Empty;

			foreach(var property in Properties)
				propertiesString = string.Format("{0},{1}", propertiesString, property.GetJSON());

			var indexOfFirstComma = propertiesString.IndexOf(',');
			if(indexOfFirstComma == 0)
				propertiesString = propertiesString.Substring(1, propertiesString.Length - 1);

			propertiesString = string.Format("[{0}]", propertiesString);
			return propertiesString;
		}
	}
}