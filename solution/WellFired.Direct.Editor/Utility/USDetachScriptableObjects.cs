using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WellFired
{
	public class USDetachScriptableObjects : EditorWindow 
	{
		private USSequencer SequenceToDetach;
	
		[MenuItem("Window/WellFired/.Direct/Utility/Make Sequences Unique")]
		private static void OpenWindow()
		{
			var window = GetWindow(typeof(USDetachScriptableObjects), false, "Utility", true) as USDetachScriptableObjects;
			window.position = new Rect(100, 100, 600, 100);
		}
	
		private void OnGUI()
		{	
			var helpStyle = new GUIStyle(GUI.skin.label);
			helpStyle.wordWrap = true;
			helpStyle.alignment = TextAnchor.UpperLeft;
			GUILayout.Label("Use this window when you have duplicated cutscenes in your scene and you would like to make them unique.", helpStyle, GUILayout.ExpandWidth(true));
	
			SequenceToDetach = EditorGUILayout.ObjectField(SequenceToDetach, typeof(USSequencer), true) as USSequencer;
	
			if(SequenceToDetach)
			{
				if(GUILayout.Button(string.Format("Detach : {0}", SequenceToDetach.name)))
					ProcessSequence(SequenceToDetach);
			}
			else
			{
				if(GUILayout.Button("Detach All in Hierarchy"))
				{
					var objects = FindObjectsOfType(typeof (USSequencer)) as USSequencer[];
					foreach (var sequence in objects)
						ProcessSequence(sequence);
				}
			}
		}

		public static void ProcessTimeline(USTimelineBase timelineBase)
		{
			var eventTimeline = timelineBase as USTimelineEvent;
			if(eventTimeline != null)
				ProcessEventTimeline(eventTimeline);
			
			var propertyTimeline = timelineBase as USTimelineProperty;
			if(propertyTimeline != null)
				ProcessPropertyTimeline(propertyTimeline);
			
			var observerTimeline = timelineBase as USTimelineObserver;
			if(observerTimeline != null)
				ProcessObserverTimeline(observerTimeline);
			
			var objectPathTimeline = timelineBase as USTimelineObjectPath;
			if(objectPathTimeline != null)
				ProcessObjectPathTimeline(objectPathTimeline);
			
			var animationTimeline = timelineBase as USTimelineAnimation;
			if(animationTimeline != null)
				ProcessAnimationTimeline(animationTimeline);
		}
		
		public static void ProcessEventTimeline(USTimelineEvent eventTimeline)
		{
			foreach(var eventBase in eventTimeline.Events)
				eventBase.MakeUnique();
		}
		
		public static void ProcessObserverTimeline(USTimelineObserver observerTimeline)
		{
			var keyframes = new List<USObserverKeyframe>();
			foreach(var keyframe in observerTimeline.observerKeyframes)
			{
				keyframes.Add(Instantiate(keyframe) as USObserverKeyframe);
			}
			
			observerTimeline.SetKeyframes(keyframes);
		}
		
		public static void ProcessObjectPathTimeline(USTimelineObjectPath objectPathTimeline)
		{
			var keyframes = new List<SplineKeyframe>();
			foreach(var keyframe in objectPathTimeline.Keyframes)
			{
				keyframes.Add(Instantiate(keyframe) as SplineKeyframe);
			}

			objectPathTimeline.SetKeyframes(keyframes);
		}
		
		public static void ProcessAnimationTimeline(USTimelineAnimation animationTimeline)
		{
			var animationTracks = new List<AnimationTrack>();
			foreach(var animationTrack in animationTimeline.AnimationTracks)
			{
				var duplicateAnimationTrack = Instantiate(animationTrack) as AnimationTrack;

				var animationClipData = new List<AnimationClipData>();
				foreach(var animationClip in animationTrack.TrackClips)
				{
					animationClipData.Add(Instantiate(animationClip) as AnimationClipData);
				}

				duplicateAnimationTrack.SetClipData(animationClipData);
				animationTracks.Add(duplicateAnimationTrack);
			}

			animationTimeline.SetTracks(animationTracks);
		}

		public static void ProcessPropertyTimeline(USTimelineProperty propertyTimeline)
		{
			if(!propertyTimeline || propertyTimeline.Properties == null)
				return;
			
			var propertyList = new List<USPropertyInfo>();
			for(var n = 0; n < propertyTimeline.Properties.Count; n++)
			{
				if(propertyTimeline.Properties[n] == null)
					continue;
				
				var usPropertyInfo = (USPropertyInfo)CreateInstance(typeof(USPropertyInfo));
				usPropertyInfo.CreatePropertyInfo(propertyTimeline.Properties[n].PropertyType);
				usPropertyInfo.Component = propertyTimeline.Properties[n].Component;
				usPropertyInfo.ComponentType = propertyTimeline.Properties[n].Component.GetType().ToString();
				usPropertyInfo.propertyInfo = propertyTimeline.Properties[n].propertyInfo;
				usPropertyInfo.fieldInfo = propertyTimeline.Properties[n].fieldInfo;
				propertyList.Add(usPropertyInfo);
				
				for(var curveIndex = 0; curveIndex < usPropertyInfo.curves.Count; curveIndex++)
				{
					var newCurve = usPropertyInfo.curves[curveIndex];
					var oldCurve = propertyTimeline.Properties[n].curves[curveIndex];
					
					foreach(var keyframe in oldCurve.Keys)
					{
						var newKeyframe = newCurve.AddKeyframe(keyframe.Time, keyframe.Value);
						newKeyframe.InTangent = keyframe.InTangent;
						newKeyframe.OutTangent = keyframe.OutTangent;
					}
				}
			}
			
			for(var n = 0; n < propertyTimeline.Properties.Count; n++)
				propertyTimeline.RemoveProperty(propertyTimeline.Properties[n]);
			
			propertyTimeline.Properties.Clear();
			for(var n = 0; n < propertyList.Count; n++)
				propertyTimeline.AddProperty(propertyList[n]);
		}

		public static void ProcessTimelineContainer(USTimelineContainer timelineContainer)
		{
			foreach(var baseTimeline in timelineContainer.Timelines)
				ProcessTimeline(baseTimeline);
		}
	
		public static void ProcessSequence(USSequencer sequence)
		{
			var windows = Resources.FindObjectsOfTypeAll<USWindow>();
			foreach(var window in windows)
				window.ResetSelectedSequence();
	
			foreach(var timelineContainer in sequence.TimelineContainers)
			{
				if(!timelineContainer)
					continue;
			
				ProcessTimelineContainer(timelineContainer);
			}
			
			Debug.Log("Succesfully Detached : " + sequence.name);
			
			PrefabUtility.DisconnectPrefabInstance(sequence.gameObject);
			Debug.Log("Succesfully DePrefabed : " + sequence.name);
		}
	}
}