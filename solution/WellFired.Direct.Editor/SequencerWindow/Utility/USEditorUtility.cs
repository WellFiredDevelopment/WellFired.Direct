using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Object = UnityEngine.Object;

namespace WellFired
{
	/// <summary>
	/// A selection of Utility functions that can be called to help with using uSequener.
	/// </summary>
	public static class USEditorUtility 
	{
		public static string SelectSequence = "Select a Sequence in the Hierarchy or Hit Create New Sequence to begin.";
		public static string NoAnimatableObjects = "Drag an object from your hierarchy to start animating it.";
		public static string Compiling = "Editor is compiling, we'll be with you shortly";
		public static string AnimationWindowIsOpen = "uSequencer doesn't work whilst the Animation Window is open, close it before continuing.";
		public static string AddingNewAffectedObjectWhilstInAnimationMode = "You cannot add a new Affected Object whilst in Animation Mode (Press the Stop Button to exit Animation Mode)";
		
		private static List<KeyValuePair<USCustomTimelineHierarchyItem, Type>> customTimelineAttributes;
		public static List<KeyValuePair<USCustomTimelineHierarchyItem, Type>> CustomTimelineAttributes
		{
			get
			{
				if(customTimelineAttributes != null)
					return customTimelineAttributes;
				
				customTimelineAttributes = new List<KeyValuePair<USCustomTimelineHierarchyItem, Type>>();
				
				var baseType = typeof(IUSHierarchyItem);
				var types = GetAllSubTypes(baseType); 
				foreach (var type in types)
				{
					if (!type.IsSubclassOf(baseType))
						continue;
					
					foreach (Attribute attr in type.GetCustomAttributes(true))
					{
						var timelineAttr = attr as USCustomTimelineHierarchyItem;
						
						if(timelineAttr == null)
							continue;
						
						customTimelineAttributes.Add(new KeyValuePair<USCustomTimelineHierarchyItem, Type>(timelineAttr, type));
					}
				}
				
				return customTimelineAttributes;
			}
		}
		
		[NonSerialized]
		static private GUISkin uSeqSkin;
		static public GUISkin USeqSkin
		{
			get
			{
				if(!uSeqSkin)
				{
					var Skin = "USequencerProSkin";
					
					if(!EditorGUIUtility.isProSkin)
						Skin = "USequencerFreeSkin";
					
					uSeqSkin = Resources.Load(Skin, typeof(GUISkin)) as GUISkin;
				}
				
				if(!uSeqSkin)
					Debug.LogError("Couldn't find the uSequencer Skin, it is possible it has been moved from the resources folder");
				
				return uSeqSkin;
			}
		}
		
		private static GUIStyle toolbar;
		public static GUIStyle Toolbar
		{
			get 
			{
				if(toolbar == null)
					toolbar = new GUIStyle(EditorStyles.toolbarButton);
				toolbar.normal.background = null;
				return toolbar;
			}
			set { ; }
		}
		
		private static GUIStyle contentBackground;
		public static GUIStyle ContentBackground
		{
			get
			{
				if(contentBackground == null)
					contentBackground = USeqSkin.GetStyle("USContentBackground");
				return contentBackground;
			}
			set { ; }
		}
		
		private static GUIStyle seperatorStyle;
		public static GUIStyle SeperatorStyle
		{
			get
			{
				if(seperatorStyle == null)
					seperatorStyle = USeqSkin.GetStyle("Seperator");
				return seperatorStyle;
			}
			set { ; }
		}
		
		private static Texture playButton;
		public static Texture PlayButton
		{
			get
			{
				if(playButton == null)
					playButton = LoadTexture("Play Button");
				return playButton;
			}
			set { ; }
		}
		
		private static Texture pauseButton;
		public static Texture PauseButton
		{
			get
			{
				if(pauseButton == null)
					pauseButton = LoadTexture("Pause Button");
				return pauseButton;
			}
			set { ; }
		}
		
		private static Texture stopButton;
		public static Texture StopButton
		{
			get
			{
				if(stopButton == null)
					stopButton = LoadTexture("Stop Button");
				return stopButton;
			}
			set { ; }
		}
		
		private static Texture recordButton;
		public static Texture RecordButton
		{
			get
			{
				if(recordButton == null)
					recordButton = LoadTexture("Record Button");
				return recordButton;
			}
			set { ; }
		}
		
		private static Texture prevKeyframeButton;
		public static Texture PrevKeyframeButton
		{
			get
			{
				if(prevKeyframeButton == null)
					prevKeyframeButton = LoadTexture("Prev Keyframe Button");
				return prevKeyframeButton;
			}
			set { ; }
		}
		
		private static Texture nextKeyframeButton;
		public static Texture NextKeyframeButton
		{
			get
			{
				if(nextKeyframeButton == null)
					nextKeyframeButton = LoadTexture("Next Keyframe Button");
				return nextKeyframeButton;
			}
			set { ; }
		}
		
		private static GUIStyle toolbarButtonSmall;
		public static GUIStyle ToolbarButtonSmall
		{
			get
			{
				if(toolbarButtonSmall == null)
					toolbarButtonSmall = USeqSkin.GetStyle("ToolbarButtonSmall");
				return toolbarButtonSmall;
			}
			set { ; }
		}
		
		private static GUIStyle buttonNoOutline;
		public static GUIStyle ButtonNoOutline 
		{
			get
			{
				if(buttonNoOutline == null)
					buttonNoOutline = USeqSkin.GetStyle("ButtonNoOutline");
				return buttonNoOutline;
			}
			set { ; }
		}
		
		private static Texture timelineMarker;
		public static Texture TimelineMarker
		{
			get
			{
				if(timelineMarker == null)
					timelineMarker = Resources.Load("TimelineMarker") as Texture;
				return timelineMarker;
			}
			set { ; }
		}
		
		private static Texture timelineScrubHead;
		public static Texture TimelineScrubHead
		{
			get
			{
				if(timelineScrubHead == null)
					timelineScrubHead = Resources.Load("TimelineScrubHead") as Texture;
				return timelineScrubHead;
			}
			set { ; }
		}
		
		private static Texture timelineScrubTail;
		public static Texture TimelineScrubTail
		{
			get
			{
				if(timelineScrubTail == null)
					timelineScrubTail = Resources.Load("TimelineScrubTail") as Texture;
				return timelineScrubTail;
			}
			set { ; }
		}
		
		private static Texture moreButton;
		public static Texture MoreButton
		{
			get
			{
				if(moreButton == null)
					moreButton = LoadTexture("More Button") as Texture;
				return moreButton;
			}
			set { ; }
		}
		
		private static Texture favouriteButtonEmpty;
		public static Texture FavouriteButtonEmpty
		{
			get
			{
				if(favouriteButtonEmpty == null)
					favouriteButtonEmpty = LoadTexture("PropertyButtonFavouriteEmpty") as Texture;
				return favouriteButtonEmpty;
			}
			set { ; }
		}

		private static Texture favouriteButtonFill;
		public static Texture FavouriteButtonFill
		{
			get
			{
				if(favouriteButtonFill == null)
					favouriteButtonFill = LoadTexture("PropertyButtonFavouriteFill") as Texture;
				return favouriteButtonFill;
			}
			set { ; }
		}
		
		private static Texture shrinkButton;
		public static Texture ShrinkButton
		{
			get
			{
				if(shrinkButton == null)
					shrinkButton = LoadTexture("TimelineButtonSmaller") as Texture;
				return shrinkButton;
			}
			set { ; }
		}
		
		private static Texture growButton;
		public static Texture GrowButton
		{
			get
			{
				if(growButton == null)
					growButton = LoadTexture("TimelineButtonBigger") as Texture;
				return growButton;
			}
			set { ; }
		}
		
		private static GUIStyle scrubBarBackground;
		public static GUIStyle ScrubBarBackground
		{
			get
			{
				if(scrubBarBackground == null)
					scrubBarBackground = USeqSkin.GetStyle("USScrubBarBackground");
				return scrubBarBackground;
			}
			set { ; }
		}
		
		static public string GetReadableTimelineContainerName(USTimelineContainer timelineContainer)
		{
			timelineContainer.RenameTimelineContainer();
			var name = "";
			name = timelineContainer.name.Replace("_Timelines for ", "");
			if(name.Contains("Timelines for "))
				name = timelineContainer.name.Replace("Timelines for ", "");
			
			return name;
		}
		
		static public Texture LoadTexture(string textureName)
		{
			var directoryName = EditorGUIUtility.isProSkin ? "_ProElements" : "_FreeElements";
			var fullFilename = String.Format("{0}/{1}", directoryName, textureName);
			return Resources.Load(fullFilename) as Texture; 
		}
		
		public static List<string> CollectAllUnusedAssetsFromSequence(USSequencer sequence)
		{
			var unusedAssets = new List<string>();
			
			var isPrefab = PrefabUtility.GetPrefabParent(sequence.gameObject) == null && PrefabUtility.GetPrefabObject(sequence.gameObject) != null;
			if(isPrefab)
			{
				Debug.LogError("Sequence is a prefab");
				return unusedAssets;
			}
			
			var prefabParent = PrefabUtility.GetPrefabParent(sequence.gameObject) as GameObject;
			var prefabName = AssetDatabase.GetAssetPath(prefabParent);
			var relativePath = Path.GetDirectoryName(prefabName);
			var relativeScriptableObjectPath = relativePath + "/ScriptableObjects";
			var assetsUsed = new List<UnityEngine.Object>();
			
			foreach(var timelineContainer in sequence.TimelineContainers)
			{
				if(!timelineContainer)
					continue;
				
				foreach(var baseTimeline in timelineContainer.Timelines)
				{
					var assetsUsedForThisTimeline = new List<UnityEngine.Object>();
					
					var timelineProperty = baseTimeline as USTimelineProperty;
					var timelineObserver = baseTimeline as USTimelineObserver;
					var timelineEvent = baseTimeline as USTimelineEvent;
					var timelineobjectPath = baseTimeline as USTimelineObjectPath;
					var timelineAnimation = baseTimeline as USTimelineAnimation;
					
					if(timelineProperty)
						assetsUsedForThisTimeline = CollectUsedAssetsForProperty(timelineProperty);
					else if(timelineObserver)
						assetsUsedForThisTimeline = CollectUsedAssetsForObserver(timelineObserver);
					else if(timelineEvent)
						assetsUsedForThisTimeline = CollectUsedAssetsForEvent(timelineEvent);
					else if(timelineobjectPath)
						assetsUsedForThisTimeline = CollectUsedAssetsForObjectPath(timelineobjectPath);
					else if(timelineAnimation)
						assetsUsedForThisTimeline = CollectUsedAssetsForObjectPath(timelineAnimation);
					
					assetsUsed.AddRange(assetsUsedForThisTimeline);
				}
			}
			
			// Ensure we don't delete files that could be a potentially desctructive operation.
			var canValidateSequence = false;
			var files = Directory.GetFiles(relativePath, "*.prefab");
			if(files.Count() > 1)
				Debug.LogError(string.Format("uSequencer will not automatically clean {0} because it has more than one sequence in it. When creating sequence prefabs, they should not be nested.", relativePath), sequence.gameObject);
			else
				canValidateSequence = true;
			
			if(!canValidateSequence)
				return unusedAssets;
			
			var scriptableObjectsInProject = Directory.GetFiles(relativeScriptableObjectPath, "*.asset");
			var scriptableObjectsToDelete = new List<string>();
			
			foreach(var scriptableObjectInProject in scriptableObjectsInProject)
			{
				var scriptableObjectFilename = Path.GetFileNameWithoutExtension(scriptableObjectInProject);
				
				var isUsed = false;
				foreach(var assetUsed in assetsUsed)
				{
					if(assetUsed == null)
						continue;
					
					if(scriptableObjectFilename == assetUsed.name)
						isUsed = true;
				}
				
				if(!isUsed)
					scriptableObjectsToDelete.Add(scriptableObjectInProject);
			}
			
			return scriptableObjectsToDelete;
		}
		
		public static void CreatePrefabFrom(USSequencer sequence, bool quiet)
		{
			if(!quiet)
			{
				if(!USPreferenceWindow.HasReadDocumentationPrefabs)
				{
					if(!EditorUtility.DisplayDialog("Warning", "It is IMPORTANT that you read the 'uSequencer InDepth Guide.pdf', " +
					                                "located in your uSequencer Directory, so you can understand how prefabs work before you continue. It will also " +
					                                "give you a understanding of your options for sharing sequences.", "continue", "cancel"))
						return;
				}
			}

			var absolutePath = "";
			var relativePath = "";
			var absoluteScriptableObjectPath = "";
			var relativeScriptableObjectPath = "";
			var prefabName = "";
			
			var parentPrefab = PrefabUtility.GetPrefabParent(sequence);
			if(!parentPrefab)
			{
				if(quiet)
					throw new Exception("Cannot use quiet mode on a sequence that isn't a prefab");

				if(!Directory.Exists(Application.dataPath + "/uSequencerData"))
					absolutePath = EditorUtility.OpenFolderPanel("Destination Directory (inside Asset folder)", "Assets", "uSequencerData");
				else
					absolutePath = EditorUtility.OpenFolderPanel("Destination Directory (inside Asset folder)", "Assets/uSequencerData", "uSequencerData");
				
				if(!absolutePath.Contains(Application.dataPath))
				{
					EditorUtility.DisplayDialog("Invalid Folder", "The folder you select must be within your unity project's Assets folder", "ok");
					return;
				}
				
				if(absolutePath == "")
				{
					Debug.LogWarning("Invalid Path Specified For Prefab");
					return;
				}
				
				relativePath = absolutePath.Substring(Application.dataPath.Length);
				
				var sequenceFolder = sequence.name;
				absolutePath += ("/" + sequenceFolder);
				relativePath = ("Assets" + relativePath + "/" + sequenceFolder);
				
				absoluteScriptableObjectPath = absolutePath + "/ScriptableObjects";
				relativeScriptableObjectPath = relativePath + "/ScriptableObjects";
				prefabName = relativePath + "/" + sequence.gameObject.name + ".prefab";
			}
			else
			{
				prefabName = AssetDatabase.GetAssetPath(parentPrefab);
				relativePath = Path.GetDirectoryName(prefabName);
				absolutePath = Path.GetFullPath(relativePath);
				absoluteScriptableObjectPath = absolutePath + "/ScriptableObjects";
				relativeScriptableObjectPath = relativePath + "/ScriptableObjects";
			}
			
			if(!Directory.Exists(absolutePath))
			{
				Directory.CreateDirectory(absolutePath);
				AssetDatabase.SaveAssets();
			}
			if(!Directory.Exists(absoluteScriptableObjectPath))
			{
				Directory.CreateDirectory(absoluteScriptableObjectPath);
				AssetDatabase.SaveAssets();
			}
			
			foreach(var timelineContainer in sequence.TimelineContainers)
			{
				if(!timelineContainer)
					continue;
				
				foreach(var baseTimeline in timelineContainer.Timelines)
				{
					var timelineProperty = baseTimeline as USTimelineProperty;
					var timelineObserver = baseTimeline as USTimelineObserver;
					var timelineEvent = baseTimeline as USTimelineEvent;
					var timelineobjectPath = baseTimeline as USTimelineObjectPath;
					var animationPath = baseTimeline as USTimelineAnimation;
					
					if(timelineProperty)
						HandlePrefabRecordProperty(timelineProperty, relativeScriptableObjectPath);
					else if(timelineObserver)
						HandlePrefabRecordObserver(timelineObserver, relativeScriptableObjectPath);
					else if(timelineEvent)
						HandlePrefabRecordEvent(timelineEvent, relativeScriptableObjectPath);
					else if(timelineobjectPath)
						HandlePrefabRecordObjectPath(timelineobjectPath, relativeScriptableObjectPath);
					else if(animationPath)
						HandlePrefabRecordAnimation(animationPath, relativeScriptableObjectPath);
					else
						Debug.LogError("Unknown timeline: " + baseTimeline);
				}
			}
			
			sequence.ObservedObjects.Clear();
			EditorUtility.SetDirty(sequence);
			AssetDatabase.SaveAssets();
			
			var shouldCleanOutputDirectory = false;
			
			if(AssetDatabase.LoadAssetAtPath(prefabName, typeof(GameObject))) 
			{
				if(quiet || EditorUtility.DisplayDialog("Are you sure?", "The prefab already exists. Do you want to update it?", "Yes", "No"))
				{
					UpdateExisting(sequence.gameObject, prefabName);
					shouldCleanOutputDirectory = true;
				}
			}
			else
			{
				CreateNew(sequence.gameObject, prefabName);
			}
			
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			
			if(shouldCleanOutputDirectory)
			{
				var unusedAssets = CollectAllUnusedAssetsFromSequence(sequence);
				foreach(var unusedAsset in unusedAssets)
					AssetDatabase.DeleteAsset(unusedAsset);
			}
			
			AssetDatabase.SaveAssets();
		}
		
		private static void HandlePrefabRecordProperty(USTimelineProperty timelineProperty, string relativeScriptableObjectPath)
		{
			if(timelineProperty == null || timelineProperty.Properties == null)
				return;

			foreach(var propertyInfo in timelineProperty.Properties)
			{
				if(!propertyInfo || !propertyInfo.Component)
					continue;

				propertyInfo.ComponentType = propertyInfo.Component.GetType().ToString();
				
				if(!AssetDatabase.Contains(propertyInfo.GetInstanceID()))
				{
					var assetName = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", relativeScriptableObjectPath, propertyInfo.GetInstanceID()));
					AssetDatabase.CreateAsset(propertyInfo, assetName);
				}
				
				EditorUtility.SetDirty(propertyInfo);
				
				if(propertyInfo.curves == null)
					continue;

				foreach(var curve in propertyInfo.curves)
				{
					if(curve == null)
					{
						Debug.LogWarning(String.Format("One of the curves on {0} is null, something has gone wrong", timelineProperty));
						continue;
					}

					if(!AssetDatabase.Contains(curve.GetInstanceID()))
					{
						var assetName = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", relativeScriptableObjectPath, curve.GetInstanceID()));
						AssetDatabase.CreateAsset(curve, assetName);
					}
					
					EditorUtility.SetDirty(curve);
					
					if(curve.Keys == null)
						continue;
					
					foreach(var internalKeyframe in curve.Keys)
					{
						if(internalKeyframe == null)
						{
							Debug.LogWarning(String.Format("One of the keyframes on {0} is null, something has gone wrong", timelineProperty));
							continue;
						}

						if(!AssetDatabase.Contains(internalKeyframe.GetInstanceID()))
						{
							var assetName = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", relativeScriptableObjectPath, internalKeyframe.GetInstanceID()));
							AssetDatabase.CreateAsset(internalKeyframe, assetName);
						}
						
						EditorUtility.SetDirty(internalKeyframe);
					}
				}
			}
		}
		
		private static void HandlePrefabRecordObserver(USTimelineObserver timelineObserver, string relativeScriptableObjectPath)
		{
			EditorUtility.SetDirty(timelineObserver);
			foreach(var observerKeyframe in timelineObserver.observerKeyframes)
			{
				if(!observerKeyframe)
					continue;
				
				observerKeyframe.cameraPath = observerKeyframe.KeyframeCamera.transform.GetFullHierarchyPath();
				
				if(!AssetDatabase.Contains(observerKeyframe.GetInstanceID()))
				{
					var assetName = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", relativeScriptableObjectPath, observerKeyframe.GetInstanceID()));
					AssetDatabase.CreateAsset(observerKeyframe, assetName);
				}
				
				EditorUtility.SetDirty(observerKeyframe);
			}
		}
		
		private static void HandlePrefabRecordEvent(USTimelineEvent timelineEvent, string relativeScriptableObjectPath)
		{
			EditorUtility.SetDirty(timelineEvent);
			
			foreach(var eventBase in timelineEvent.Events)
			{
				if(!eventBase)
					continue;
				
				var objectsToSerialize = eventBase.GetAdditionalObjects();
				if(objectsToSerialize == null || objectsToSerialize.Length == 0)
					continue;
				
				var objectPathsToSerialize = new List<string>();
				foreach(var objectToSerialize in objectsToSerialize)
					objectPathsToSerialize.Add(objectToSerialize.transform.GetFullHierarchyPath());
				
				eventBase.SetSerializedAdditionalObjectsPaths(objectPathsToSerialize.ToArray());
			}
		}
		
		private static void HandlePrefabRecordObjectPath(USTimelineObjectPath timelineObjectPath, string relativeScriptableObjectPath)
		{
			EditorUtility.SetDirty(timelineObjectPath);

			timelineObjectPath.RecordAdditionalObjects();

			foreach(var splineKeyframe in timelineObjectPath.Keyframes)
			{	
				if(!AssetDatabase.Contains(splineKeyframe.GetInstanceID()))
				{
					var assetName = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", relativeScriptableObjectPath, splineKeyframe.GetInstanceID()));
					AssetDatabase.CreateAsset(splineKeyframe, assetName);
				}
				
				EditorUtility.SetDirty(splineKeyframe);
			}
		}

		private static void HandlePrefabRecordAnimation(USTimelineAnimation timelineAnimation, string relativeScriptableObjectPath)
		{
			EditorUtility.SetDirty(timelineAnimation);

			foreach(var track in timelineAnimation.AnimationTracks)
			{	
				if(!AssetDatabase.Contains(track.GetInstanceID()))
				{
					var assetName = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", relativeScriptableObjectPath, track.GetInstanceID()));
					AssetDatabase.CreateAsset(track, assetName);
				}

				EditorUtility.SetDirty(track);

				foreach(var clipData in track.TrackClips)
				{
					if (!AssetDatabase.Contains(clipData.GetInstanceID()))
					{
						var assetName = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", relativeScriptableObjectPath, clipData.GetInstanceID()));
						AssetDatabase.CreateAsset(clipData, assetName);
					}

					EditorUtility.SetDirty(clipData);
				}
			}
		}
		
		private static List<UnityEngine.Object> CollectUsedAssetsForProperty(USTimelineProperty timelineProperty)
		{
			var usedAssets = new List<UnityEngine.Object>();
			
			if(timelineProperty.Properties == null)
				return usedAssets;
			
			foreach(var propertyInfo in timelineProperty.Properties)
			{
				if(!propertyInfo || !propertyInfo.Component)
					continue;
				
				usedAssets.Add(propertyInfo);
				
				if(propertyInfo.curves == null)
					continue;
				
				foreach(var curve in propertyInfo.curves)
				{
					usedAssets.Add(curve);
					
					if(curve.Keys == null)
						continue;
					
					foreach(var internalKeyframe in curve.Keys)
						usedAssets.Add(internalKeyframe);
				}
			}
			
			return usedAssets;
		}
		
		private static List<UnityEngine.Object> CollectUsedAssetsForObserver(USTimelineObserver timelineObserver)
		{
			var usedAssets = new List<UnityEngine.Object>();
			
			foreach(var observerKeyframe in timelineObserver.observerKeyframes)
			{
				if(!observerKeyframe)
					continue;
				
				usedAssets.Add(observerKeyframe);
			}
			
			return usedAssets;
		}
		
		private static List<UnityEngine.Object> CollectUsedAssetsForEvent(USTimelineEvent timelineEvent)
		{
			var usedAssets = new List<UnityEngine.Object>();
			
			foreach(var eventBase in timelineEvent.Events)
			{
				if(!eventBase)
					continue;
				
				usedAssets.Add(eventBase);
			}
			
			return usedAssets;
		}
		
		private static List<UnityEngine.Object> CollectUsedAssetsForObjectPath(USTimelineObjectPath timelineObjectPath)
		{
			var usedAssets = new List<UnityEngine.Object>();
			
			foreach(var splineKeyframe in timelineObjectPath.Keyframes)
				usedAssets.Add(splineKeyframe);
			
			return usedAssets;
		}
		
		private static List<UnityEngine.Object> CollectUsedAssetsForObjectPath(USTimelineAnimation timelineAnimation)
		{
			var usedAssets = new List<UnityEngine.Object>();
			foreach(var track in timelineAnimation.AnimationTracks)
			{
				usedAssets.Add(track);
				foreach(var clipData in track.TrackClips)
					usedAssets.Add(clipData);
			}

			return usedAssets;
		}
		
		private static void CreateNew(GameObject gameObject, String localPath)
		{	
			var prefab = PrefabUtility.CreateEmptyPrefab(localPath);
			var prefabGO = PrefabUtility.ReplacePrefab(gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
			EditorUtility.SetDirty(prefabGO);
		}
		
		private static void UpdateExisting(GameObject gameObject, String localPath)
		{	
			var prefabGO = PrefabUtility.ReplacePrefab(gameObject, PrefabUtility.GetPrefabParent(gameObject), ReplacePrefabOptions.ConnectToPrefab);
			EditorUtility.SetDirty(prefabGO);
		}

		public static bool CanContextClickTimelineContainer(USTimelineContainer timelineContainer)
		{
			return !USRuntimeUtility.HasObserverTimeline(timelineContainer.transform);
		}

		public static bool CanContextClickTimeline(USTimelineBase baseTimeline)
		{
			return true;
		}
		
		public static Type[] GetAllSubTypes(Type aBaseClass)
		{
			var result = new List<Type>();
			var AS = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var A in AS)
			{
				try
				{
					var types = A.GetTypes();
					foreach (var T in types)
					{
						if (T.IsSubclassOf(aBaseClass))
							result.Add(T);
					}
				}
				catch(ReflectionTypeLoadException) { ; }
			}
			return result.ToArray();
		}
		
		public static bool DoRectsOverlap(Rect RectA, Rect RectB)
		{
			return 	RectA.xMin < RectB.xMax && RectA.xMax > RectB.xMin &&
				RectA.yMin < RectB.yMax && RectA.yMax > RectB.yMin;
		}
		
		public static int FindKeyframeIndex(AnimationCurve curve, Keyframe keyframe)
		{
			for(var n = 0; n < curve.keys.Length; n++)
			{
				if(curve.keys[n].Equals(keyframe))
					return n;
			}
			
			return -1;
		}
		
		public static void RemoveFromUnitySelection(UnityEngine.Object objectToRemove)
		{
			var newSelection = Selection.objects.ToList();
			newSelection.RemoveAll(element => element == objectToRemove);
			Selection.objects = newSelection.ToArray();
		}
		
		public static void RemoveFromUnitySelection(List<UnityEngine.Object> objectToRemove)
		{
			var newSelection = Selection.objects.ToList();
			newSelection.RemoveAll(element => objectToRemove.Contains(element));
			Selection.objects = newSelection.ToArray();
		}

		public static float FindPrevVisibleKeyframeTime(USHierarchy hierarchy, USSequencer sequence)
		{
			var previousKeyframeTime = 0.0f;
			foreach(var rootItems in hierarchy.RootItems)
			{
				if(!rootItems.IsExpanded)
					continue;
				
				foreach(var rootChild in rootItems.Children)
				{
					var propertyTimelineContainer = rootChild as USPropertyTimelineHierarchyItem;

					if(propertyTimelineContainer == null || !propertyTimelineContainer.IsExpanded)
						continue;

					var componentPreviousKeyframeTime = propertyTimelineContainer.GetPreviousShownKeyframeTime(sequence.RunningTime);
					if(componentPreviousKeyframeTime > previousKeyframeTime)
						previousKeyframeTime = componentPreviousKeyframeTime;
				}
			}

			return previousKeyframeTime;
		}

		public static float FindNextVisibleKeyframeTime(USHierarchy hierarchy, USSequencer sequence)
		{
			var nextKeyframeTime = sequence.Duration;
			foreach(var rootItems in hierarchy.RootItems)
			{
				if(!rootItems.IsExpanded)
					continue;
				
				foreach(var rootChild in rootItems.Children)
				{
					var propertyTimelineContainer = rootChild as USPropertyTimelineHierarchyItem;
					
					if(propertyTimelineContainer == null || !propertyTimelineContainer.IsExpanded)
						continue;
					
					var componentNextKeyframeTime = propertyTimelineContainer.GetNextShownKeyframeTime(sequence.RunningTime);
					if(componentNextKeyframeTime < nextKeyframeTime)
						nextKeyframeTime = componentNextKeyframeTime;
				}
			}
			
			return nextKeyframeTime;
		}

		public static void DuplicateSequence(USSequencer currentSequence)
		{
			var duplicateObject = Object.Instantiate(currentSequence.gameObject) as GameObject;
			USDetachScriptableObjects.ProcessSequence(duplicateObject.GetComponent<USSequencer>());
			Selection.activeGameObject = duplicateObject;
			
			USUndoManager.RegisterCreatedObjectUndo(duplicateObject, "Duplicate Sequence");
		}
	}
}