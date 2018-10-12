﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace WellFired
{
	[Serializable]
	public class USWindow : EditorWindow 
	{
		public static Vector2 minWindowSize = new Vector2(750.0f, 250.0f);
		
		[SerializeField]
		private static USWindow thisWindow;
		
		[MenuItem ("Window/WellFired/.Direct/Open %u")]
		public static void OpenuSequencerWindow() 
		{
			var window = GetWindow (typeof(USWindow), false, ".Direct") as USWindow;
			
			window.autoRepaintOnSceneChange = true;
			window.minSize = minWindowSize;
		}
		
		[MenuItem ("Window/WellFired/.Direct/Close %i")]
		static void CloseuSequencerWindow() 
		{
			var window = GetWindow<USWindow>();
			window.Close();
		}
		
		[SerializeField]
		private USSequencer currentSequence;
		public USSequencer CurrentSequence
		{
			get { return currentSequence; }
			private set 
			{ 
				currentSequence = value; 
				if(currentSequence)
					currentSequence.ResetCachedData(); 
			}
		}
		
		[SerializeField]
		private bool showOnlyAnimated;
		private bool ShowOnlyAnimated
		{
			get { return showOnlyAnimated; }
			set { showOnlyAnimated = value; }
		}
		
		[SerializeField]
		private bool isInAnimationMode;
		private bool IsInAnimationMode
		{
			get { return isInAnimationMode; }
			set { isInAnimationMode = value; AnimationHelper.IsInAnimationMode = IsInAnimationMode; }
		}
		
		[SerializeField]
		private bool isArmed;
		private bool IsArmed
		{
			get { return isArmed; }
			set { isArmed = value; }
		}
		
		[SerializeField]
		private USContent contentRenderer;
		private USContent ContentRenderer
		{
			get { return contentRenderer; }
			set { contentRenderer = value; }
		}
		
		[SerializeField]
		private float PreviousTime
		{
			get;
			set;
		}
		
		private Rect TopBar
		{
			get;
			set;
		}
		
		private Rect Content
		{
			get;
			set;
		}
		
		private Rect BottomBar
		{
			get;
			set;
		}
		
		private double showAnimationModeTime
		{
			get;
			set;
		}

		public static bool IsScrubbing 
		{
			get { return thisWindow.ContentRenderer.ScrubHandleDrag; }
			set { ; }
		}
		
		[SerializeField]
		private CurveAutoTangentModes autoTangentMode = CurveAutoTangentModes.Smooth;
		private CurveAutoTangentModes AutoTangentMode
		{
			get { return autoTangentMode; }
			set { autoTangentMode = value; }
		}
	
		private void OnEnable()
		{
			showAnimationModeTime = EditorApplication.timeSinceStartup;
			thisWindow = this;
			hideFlags = HideFlags.HideAndDontSave;
			
			if(ContentRenderer == null)
				ContentRenderer = CreateInstance<USContent>();
			ContentRenderer.SequenceWindow = this;
			
			// Run our upgrade paths, do this for each sequence in the hierarchy.
			var sequences = FindObjectsOfType(typeof(USSequencer)) as USSequencer[];
			foreach(var sequence in sequences)
				USUpgradePaths.RunUpgradePaths(sequence);
			
			EditorApplication.hierarchyWindowChanged -= OnHierarchyChanged;
			EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;
			SceneView.onSceneGUIDelegate -= OnScene;
			SceneView.onSceneGUIDelegate += OnScene;
			Undo.undoRedoPerformed -= UndoRedoCallback;
			Undo.undoRedoPerformed += UndoRedoCallback;
			EditorApplication.playmodeStateChanged -= PlayModeStateChanged;
			EditorApplication.playmodeStateChanged += PlayModeStateChanged;
			EditorApplication.update -= SequenceUpdate;
			EditorApplication.update += SequenceUpdate;
		}
		
		private void OnDestroy()
		{
			thisWindow = null;
			EditorApplication.hierarchyWindowChanged -= OnHierarchyChanged;
			SceneView.onSceneGUIDelegate -= OnScene;
			Undo.undoRedoPerformed -= UndoRedoCallback;
			EditorApplication.playmodeStateChanged -= PlayModeStateChanged;
			EditorApplication.update -= SequenceUpdate;
			
			StopProcessingAnimationMode();
			
			if(CurrentSequence)
				CurrentSequence.Stop();
		}
		
		private void UndoRedoCallback()
		{
			// This hack ensures that we process and update our in progress Sequence when Undo / Redoing
			if(CurrentSequence)
			{
				var previousRunningTime = CurrentSequence.RunningTime;

				// if we undo we always revert to our base state.
				ContentRenderer.RestoreBaseState();

				// if our running time is then > 0.0f, we process the timeline
				if(previousRunningTime > 0.0f)
				{
					CurrentSequence.RunningTime = previousRunningTime;
				}
			}

			Repaint();
		}
		
		private void PlayModeStateChanged()
		{
			PropertyBoxPopup.OpenCooldown = 0.0f;
			StopProcessingAnimationMode();
			IsArmed = false;
		}
		
		private void OnGUI()
		{
			AnimationCurveEditor.AutoTangentMode = AutoTangentMode;
			AnimationHelper.IsInAnimationMode = IsInAnimationMode;
			
			if(!CurrentSequence)
				ShowNotification(new GUIContent(USEditorUtility.SelectSequence));
			if(CurrentSequence && CurrentSequence.TimelineContainerCount <= 1)
				ShowNotification(new GUIContent(USEditorUtility.NoAnimatableObjects));
			
			if(CurrentSequence && CurrentSequence.TimelineContainerCount > 1 && EditorApplication.timeSinceStartup - showAnimationModeTime > 3000)
				RemoveNotification();
			
			using(new Shared.GUIBeginVertical())
			{
				DisplayTopBar();
				DisplayEdittableArea();
				DisplayBottomBar();
			}
			
			ProcessHotkeys();
			
			if(Event.current.type == EventType.DragUpdated)
			{
				if(!AnimationHelper.IsInAnimationMode)
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Link;
					Event.current.Use();
				}
				else
				{
					showAnimationModeTime = EditorApplication.timeSinceStartup;
					ShowNotification(new GUIContent(USEditorUtility.AddingNewAffectedObjectWhilstInAnimationMode));
				}
			}
			
			if(Event.current.type == EventType.DragPerform)
			{
				if(!AnimationHelper.IsInAnimationMode)
				{
					foreach(var dragObject in DragAndDrop.objectReferences)
					{
						var GO = dragObject as GameObject;
						if(GO != CurrentSequence.gameObject)
						{					
							DragAndDrop.AcceptDrag();
							
							//Do we already have a timeline for this object
							foreach(var timelineContainer in CurrentSequence.TimelineContainers)
							{
								if(timelineContainer.AffectedObject == GO.transform)
									return;
							}
							
							var newTimelineContainer = CurrentSequence.CreateNewTimelineContainer(GO.transform);
							USUndoManager.RegisterCreatedObjectUndo(newTimelineContainer.gameObject, "Add New Timeline Container");
							ContentRenderer.AddNewTimelineContainer(newTimelineContainer);
						}
					}
					
					Event.current.Use();
				}
			}
		}
		
		private void DisplayTopBar()
		{
			var space = 16.0f;
			GUILayout.Box("", EditorStyles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(18.0f));
			
			if(Event.current.type == EventType.Repaint)
				TopBar = GUILayoutUtility.GetLastRect();
			
			using(new Shared.GUIBeginArea(TopBar))
			{
				using(new Shared.GUIBeginHorizontal())
				{
					if(GUILayout.Button("Create New Sequence", EditorStyles.toolbarButton)) 
					{
						var newSequence = new GameObject("Sequence");
						USUndoManager.RegisterCreatedObjectUndo(newSequence, "Create new sequence");

						var sequence = newSequence.AddComponent<USSequencer>();
						sequence.Version = USUpgradePaths.CurrentVersionNumber;
						USUndoManager.RegisterCreatedObjectUndo(sequence, "Create new sequence");

						USUndoManager.RegisterCompleteObjectUndo(newSequence, "Create new sequence");
						USRuntimeUtility.CreateAndAttachObserver(sequence);

						if(CurrentSequence == null)
						{
							Selection.activeGameObject = newSequence;
							Selection.activeTransform = newSequence.transform;
							SequenceSwitch(sequence);
						}

						Repaint();
					}

					var currentSequence = CurrentSequence != null ? CurrentSequence.name : "";
					var label = "Select a Sequence";
					if(CurrentSequence != null)
						label = String.Format("Editting : {0}", currentSequence);
					if (GUILayout.Button(label, EditorStyles.toolbarButton, GUILayout.Width(150.0f)))
					{
						var menu = new GenericMenu();
						var sequences = FindObjectsOfType(typeof(USSequencer)) as USSequencer[];
						var orderedSequences = sequences.OrderBy(sequence => sequence.name);
						foreach (var sequence in orderedSequences)
							menu.AddItem(new GUIContent(sequence.name), 
							             currentSequence == sequence.name ? true : false, 
							             (obj) => Selection.activeGameObject = (GameObject)obj, 
							             sequence.gameObject);
						menu.ShowAsContext();
					}

					GUILayout.Space(space);
					GUILayout.Box("", USEditorUtility.SeperatorStyle, GUILayout.Height(18.0f));
					GUILayout.Space(space);
					
					if(CurrentSequence != null)
					{
						using(new Shared.GUIChangeColor((AnimationHelper.IsInAnimationMode || IsArmed) ? Color.red : GUI.color))
						{
							if(GUILayout.Button(new GUIContent(!CurrentSequence.IsPlaying ? USEditorUtility.PlayButton : USEditorUtility.PauseButton, "Toggle Play Mode (P)"), USEditorUtility.ToolbarButtonSmall))
								PlayOrPause();
							
							if(GUILayout.Button(USEditorUtility.StopButton, USEditorUtility.ToolbarButtonSmall))
								Stop();
						}
						using(new Shared.GUIEnable(EditorApplication.isPlaying))
						{
							var buttonContent = new GUIContent(USEditorUtility.RecordButton, !EditorApplication.isPlaying ? "You must be in Play Mode to enable this button" : "Start g");
							if(GUILayout.Button(buttonContent, USEditorUtility.ToolbarButtonSmall))
								Record();
						}
						
						GUILayout.Space(space);
						
						USUndoManager.BeginChangeCheck();
						GUILayout.Button(new GUIContent(USEditorUtility.PrevKeyframeButton, "Prev Keyframe (Alt + ,)"), USEditorUtility.ToolbarButtonSmall);
						if(USUndoManager.EndChangeCheck())
						{
							USUndoManager.PropertyChange(this, "Previous Keyframe");
							GoToPrevKeyframe();
						}
						
						USUndoManager.BeginChangeCheck();
						GUILayout.Button(new GUIContent(USEditorUtility.NextKeyframeButton, "Next Keyframe (Alt + .)"), USEditorUtility.ToolbarButtonSmall);
						if(USUndoManager.EndChangeCheck())
						{
							USUndoManager.PropertyChange(this, "Next Keyframe");
							GoToNextKeyframe();
						}
						
						GUILayout.Space(space);
						GUILayout.Box("", USEditorUtility.SeperatorStyle, GUILayout.Height(18.0f));
						GUILayout.Space(space);
						
						EditorGUILayout.LabelField(new GUIContent("Keyframe Snap", "The amount keyframes will snap to when dragged in the uSequencer window. (Left Shift to activate)"), GUILayout.MaxWidth(100.0f));
						USUndoManager.BeginChangeCheck();
						var snapAmount = EditorGUILayout.FloatField(new GUIContent("", "The amount keyframes will snap to when dragged in the uSequencer window. (Left Shift to activate)"), ContentRenderer.SnapAmount, GUILayout.MaxWidth(40.0f));
						if(USUndoManager.EndChangeCheck())
						{
							USUndoManager.PropertyChange(this, "Snap Amount");
							ContentRenderer.SnapAmount = snapAmount;
						}
						
						GUILayout.Space(space);
						GUILayout.Box("", USEditorUtility.SeperatorStyle, GUILayout.Height(18.0f));
						GUILayout.Space(space);
					}
					
					GUILayout.FlexibleSpace();
					
					if(CurrentSequence && GUILayout.Button("Duplicate Sequence", EditorStyles.toolbarButton))
						USEditorUtility.DuplicateSequence(CurrentSequence);
					
					if(CurrentSequence && GUILayout.Button(PrefabUtility.GetPrefabObject(CurrentSequence.gameObject) ? "Update Prefab" : "Create Prefab", EditorStyles.toolbarButton))
					{
						CurrentSequence.Stop();
						StopProcessingAnimationMode();
						USEditorUtility.CreatePrefabFrom(CurrentSequence, false);
					}
				}
			}
		}
		
		private void DisplayEdittableArea()
		{
			if(CurrentSequence)
				contentRenderer.OnGUI();
			else
				GUILayout.Box("", USEditorUtility.ContentBackground, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		}
		
		private void DisplayBottomBar()
		{
			var space = 16.0f;
			GUILayout.Box("", EditorStyles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(20.0f));
			
			if(Event.current.type == EventType.Repaint)
				BottomBar = GUILayoutUtility.GetLastRect();
			
			if(CurrentSequence == null)
				return;
			
			using(new Shared.GUIBeginArea(BottomBar))
			{
				using(new Shared.GUIBeginHorizontal())
				{
					USUndoManager.BeginChangeCheck();
					string[] showAllOptions = { "Show All", "Show Only Animated" };	
					var selectedShowAll = 0;
					if(ShowOnlyAnimated)
						selectedShowAll = 1;
					selectedShowAll = EditorGUILayout.Popup(selectedShowAll, showAllOptions, EditorStyles.toolbarPopup, GUILayout.MaxWidth(120.0f));
					if(USUndoManager.EndChangeCheck())
					{
						USUndoManager.PropertyChange(this, "Show Animated");
						if(selectedShowAll == 1)
							ShowOnlyAnimated = true;
						else
							ShowOnlyAnimated = false;
					}
					
					USUndoManager.BeginChangeCheck();
					string[] playBackOptions = { "Playback : Normal", "Playback : Looping", "Playback : PingPong" };	
					var selectedAnimationType = 0;
					if(CurrentSequence.IsLopping)
						selectedAnimationType = 1;
					else if(CurrentSequence.IsPingPonging)
						selectedAnimationType = 2;
					selectedAnimationType = EditorGUILayout.Popup(selectedAnimationType, playBackOptions, EditorStyles.toolbarPopup, GUILayout.MaxWidth(120.0f));
					if(USUndoManager.EndChangeCheck())
					{
						USUndoManager.PropertyChange(CurrentSequence, "Playback State");
						CurrentSequence.IsLopping = false;
						CurrentSequence.IsPingPonging = false;
						if(selectedAnimationType == 1)
						{
							CurrentSequence.IsLopping = true;
							CurrentSequence.IsPingPonging = false;
						}
						else if(selectedAnimationType == 2)
						{
							CurrentSequence.IsLopping = false;
							CurrentSequence.IsPingPonging = true;
						}
					}
					
					USUndoManager.BeginChangeCheck();
					string[] curveAutoTangentModesNames = { "Tangent Mode : None", "Tangent Mode : Smooth", "Tangent Mode : Flatten", "Tangent Mode : RightLinear", "Tangent Mode : RightConstant", "Tangent Mode : LeftLinear", "Tangent Mode : LeftConstant", "Tangent Mode : BothLinear", "Tangent Mode : BothConstant", };
					var newAutoTangentMode = EditorGUILayout.Popup((int)AutoTangentMode, curveAutoTangentModesNames, EditorStyles.toolbarPopup, GUILayout.MaxWidth(150.0f));
					if(USUndoManager.EndChangeCheck())
					{
						USUndoManager.PropertyChange(this, "AutoTangentMode");
						AutoTangentMode = (CurveAutoTangentModes)newAutoTangentMode;
						AnimationCurveEditor.AutoTangentMode = AutoTangentMode;
					}
					
					GUILayout.Space(space);
					GUILayout.Box("", USEditorUtility.SeperatorStyle, GUILayout.Height(18.0f));
					GUILayout.Space(space);
					
					EditorGUILayout.LabelField("", "Running Time", GUILayout.MaxWidth(100.0f));
					USUndoManager.BeginChangeCheck();
					var runningTime = EditorGUILayout.FloatField("", CurrentSequence.RunningTime, GUILayout.MaxWidth(50.0f));
					if(USUndoManager.EndChangeCheck())
					{
						USUndoManager.PropertyChange(CurrentSequence, "Running Time");
						SetRunningTime(runningTime);
					}
					
					GUILayout.Space(space);
					GUILayout.Box("", USEditorUtility.SeperatorStyle, GUILayout.Height(18.0f));
					GUILayout.Space(space);
					
					EditorGUILayout.LabelField("", "Duration", GUILayout.MaxWidth(50.0f));
					USUndoManager.BeginChangeCheck();
					var duration = EditorGUILayout.FloatField("", CurrentSequence.Duration, GUILayout.MaxWidth(50.0f));
					if(USUndoManager.EndChangeCheck())
					{
						USUndoManager.PropertyChange(CurrentSequence, "Duration");
						CurrentSequence.Duration = duration;
						ContentRenderer.UpdateCachedMarkerInformation();
						ContentRenderer.USHierarchy.ExternalModification();
					}
					
					GUILayout.Space(space);
					GUILayout.Box("", USEditorUtility.SeperatorStyle, GUILayout.Height(18.0f));
					GUILayout.Space(space);
					
					EditorGUILayout.LabelField("", "PlaybackRate", GUILayout.MaxWidth(80.0f));
					USUndoManager.BeginChangeCheck();
					var playbackRate = EditorGUILayout.FloatField("", CurrentSequence.PlaybackRate, GUILayout.MaxWidth(50.0f));
					if(USUndoManager.EndChangeCheck())
					{
						USUndoManager.PropertyChange(CurrentSequence, "PlaybackRate");
						CurrentSequence.PlaybackRate = playbackRate;
						ContentRenderer.UpdateCachedMarkerInformation();
					}

					GUILayout.FlexibleSpace();
				}
			}
		}
		
		private void ProcessHotkeys()
		{
			if(Event.current.rawType == EventType.KeyDown && Event.current.keyCode == KeyCode.P)
			{
				PlayOrPause();
				Event.current.Use();
			}
			
			if(Event.current.rawType == EventType.KeyDown && Event.current.keyCode == KeyCode.S)
			{
				Stop();
				Event.current.Use();
			}
			
			if(Event.current.rawType == EventType.KeyDown && (Event.current.keyCode == KeyCode.Backspace || Event.current.keyCode == KeyCode.Delete))
			{
				foreach(var timelineContainer in contentRenderer.USHierarchy.RootItems)
				{
					foreach(var timeline in timelineContainer.Children)
					{
						var ISelectableContainers = timeline.GetISelectableContainers();
						foreach(var ISelectableContainer in ISelectableContainers)
							ISelectableContainer.DeleteSelection();
					}
				}
				Event.current.Use();
			}
			
			if(Event.current.rawType == EventType.KeyDown && Event.current.keyCode == KeyCode.K && IsInAnimationMode)
			{
				foreach(var root in contentRenderer.USHierarchy.RootItems)
				{
					if(!root.IsExpanded)
						continue;

					foreach(var timelineContainer in root.Children)
					{
						if(!(timelineContainer is USPropertyTimelineHierarchyItem))
							continue;
						
						var propertyTimelineContainer = timelineContainer as USPropertyTimelineHierarchyItem;
						
						if(!propertyTimelineContainer.IsExpanded)
							continue;
					}
				}
				Event.current.Use();
			}
			
			if(Event.current.shift)
				ContentRenderer.Snap = true;
			else
				ContentRenderer.Snap = false;
			
			if(Event.current.rawType == EventType.KeyDown && Event.current.keyCode == KeyCode.Period && Event.current.alt)
			{
				GoToNextKeyframe();
				Event.current.Use();
			}
			else if(Event.current.rawType == EventType.KeyDown && Event.current.keyCode == KeyCode.Comma && Event.current.alt)
			{
				GoToPrevKeyframe();
				Event.current.Use();
			}
			else if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Period)
			{
				SetRunningTime((float)Math.Round(CurrentSequence.RunningTime + 0.01f, 2));
				Event.current.Use();
			}
			else if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Comma)
			{
				SetRunningTime((float)Math.Round(CurrentSequence.RunningTime - 0.01f, 2));
				Event.current.Use();
			}

			if(showPopup)
				ActuallyShowPopup();
		}
		
		private void PlayOrPause()
		{
			if(!CurrentSequence)
				return;
			
			if(CurrentSequence.IsPlaying)
			{
				CurrentSequence.Pause();
				
				if(isArmed && GetOrSpawnRecorder())
					GetOrSpawnRecorder().PauseRecording();
			}
			else
			{
				USUndoManager.PropertyChange(CurrentSequence, "Play");
				
				if(isArmed && !System.IO.Directory.Exists(USRecordRuntimePreferences.CapturePath))
				{
					EditorUtility.DisplayDialog("Error", String.Format("Recording Directory : {0} doesn't exist, make sure you set one up in the uSequencer preferences (Edit/uSeqeucer/Recording Preferences)", USRecordRuntimePreferences.CapturePath), "OK");
					return;
				}
				
				StartProcessingAnimationMode();
				CurrentSequence.Play();
				
				if(isArmed && GetOrSpawnRecorder())
				{
					var recordingSequence = GetOrSpawnRecorder();
					
					recordingSequence.StartRecording();
					
					recordingSequence.CapturePath = USRecordRuntimePreferences.CapturePath;
					recordingSequence.CaptureFrameRate = USRecord.GetFramerate();
					recordingSequence.UpscaleAmount = USRecord.GetUpscaleAmount();
				}
			}
		}
		
		private void Stop()
		{
			if(!CurrentSequence)
				return;
			
			USUndoManager.PropertyChange(CurrentSequence, "Stop");
			
			CurrentSequence.Stop();
			StopProcessingAnimationMode();
			
			if(isArmed)
			{
				if(GetOrSpawnRecorder())
					GetOrSpawnRecorder().StopRecording();
				
				DestroyImmediate(GetOrSpawnRecorder().gameObject);
			}
			
			isArmed = false;
		}
		
		private void Record()
		{
			if(!CurrentSequence)
				return;
			
			if(isArmed)
				DestroyImmediate(GetOrSpawnRecorder().gameObject);
			
			isArmed = !isArmed;
			
			if(isArmed)
				GetOrSpawnRecorder();
		}
		
		private USRecordSequence GetOrSpawnRecorder()
		{
			var recorder = FindObjectOfType(typeof(USRecordSequence)) as USRecordSequence;
			
			if(recorder)
				return recorder;
			
			var recordingObject = new GameObject("Recording Object");
			var recordingSequence = recordingObject.AddComponent<USRecordSequence>();
			recordingSequence.CapturePath = USRecordRuntimePreferences.CapturePath;
			recordingSequence.CaptureFrameRate = USRecord.GetFramerate();
			recordingSequence.UpscaleAmount = USRecord.GetUpscaleAmount();
			
			return recordingSequence;
		}
		
		private void SequenceUpdate()
		{
			if(PropertyBoxPopup.OpenCooldown >= 0.0f)
			{
				var currentTime = Time.realtimeSinceStartup;
				var deltaTime = currentTime - PreviousTime;
				PropertyBoxPopup.OpenCooldown = PropertyBoxPopup.OpenCooldown - deltaTime;
			}
			PropertyBoxPopup.OpenCooldown = Mathf.Clamp(PropertyBoxPopup.OpenCooldown, 0.0f, 1.0f);

			if(CurrentSequence)
			{
				var currentTime = Time.realtimeSinceStartup;
				var deltaTime = currentTime - PreviousTime;

				if(Mathf.Abs(deltaTime) > USSequencer.SequenceUpdateRate)
				{
					if(CurrentSequence.IsPlaying && !Application.isPlaying)
					{
						CurrentSequence.UpdateSequencer(deltaTime * Time.timeScale);
						Repaint();
					}
					PreviousTime = currentTime;
				}
			}
			
			USSequencer nextSequence = null;
			
			if(Selection.activeGameObject != null && (CurrentSequence == null || Selection.activeGameObject != CurrentSequence.gameObject))
			{
				nextSequence = Selection.activeGameObject.GetComponent<USSequencer>();
				if(nextSequence != null)
				{
					var isPrefab = PrefabUtility.GetPrefabParent(nextSequence.gameObject) == null && PrefabUtility.GetPrefabObject(nextSequence.gameObject) != null;
					if(isPrefab)
						nextSequence = null;
				}
			}
			else
			{
				return;
			}

			if(nextSequence == null)
				return;
			
			if(!Application.isPlaying && CurrentSequence != nextSequence)
			{
				ShowOnlyAnimated = false;
				
				if(CurrentSequence)
					CurrentSequence.Stop();
				
				if(nextSequence)
					nextSequence.Stop();

				StopProcessingAnimationMode();
			}

			SequenceSwitch(nextSequence);

			Repaint();
		}

		private void SequenceSwitch(USSequencer nextSequence)
		{
			USUndoManager.RegisterCompleteObjectUndo(this, "Select new sequence");
			CurrentSequence = nextSequence;
			
			USUndoManager.RegisterCompleteObjectUndo(ContentRenderer, "Select new sequence");
			ContentRenderer.OnSequenceChange(CurrentSequence);
			
			TryToFixPropertyTimelines(CurrentSequence);
			TryToFixObserverTimelines(CurrentSequence);
		}
		
		private void StartProcessingAnimationMode()
		{
			if(AnimationHelper.IsInAnimationMode)
				return;
			
			if(Application.isPlaying)
				return;
			
			var objects = new List<Component>();
			
			// All observered objects
			foreach (var observedObject in CurrentSequence.ObservedObjects)
				SaveObjectValueInAnimationMode(observedObject, ref objects);
			
			USUndoManager.RegisterCompleteObjectUndo(this, "Play");
			IsInAnimationMode = true;

			ContentRenderer.StoreBaseState();
		}
		
		private void StopProcessingAnimationMode()
		{
			
			if(CurrentSequence)
				CurrentSequence.Stop();

			if (!AnimationHelper.IsInAnimationMode)
				return;
			
			if(Application.isPlaying)
				return;

			USUndoManager.RegisterCompleteObjectUndo(this, "Play");
			IsInAnimationMode = false;

			ContentRenderer.RestoreBaseState();
		}
		
		public void RevertForSave()
		{
			Stop();
		}
		
		private void SaveObjectValueInAnimationMode(Transform parent, ref List<Component> listObjs)
		{
			if (parent && parent.gameObject)
			{
				var componentList = parent.gameObject.GetComponents<Component>();
				for (var i = 0; i < componentList.Length; i++)
					listObjs.Add(componentList[i]);
				
				foreach (Transform transform in parent)
					SaveObjectValueInAnimationMode(transform, ref listObjs);
			}
		}
		
		private void GoToNextKeyframe()
		{
			SetRunningTime(USEditorUtility.FindNextVisibleKeyframeTime(ContentRenderer.USHierarchy, CurrentSequence));
		}
		
		private void GoToPrevKeyframe()
		{
			SetRunningTime(USEditorUtility.FindPrevVisibleKeyframeTime(ContentRenderer.USHierarchy, CurrentSequence));
		}
		
		public void SetRunningTime(float newRunningTime)
		{
			StartProcessingAnimationMode();
			if(!CurrentSequence.IsPlaying)
				CurrentSequence.Play();
			CurrentSequence.Pause();
			
			USUndoManager.PropertyChange(CurrentSequence, "Set Running Time");
			CurrentSequence.RunningTime = newRunningTime;
		}
		
		public void ResetSelectedSequence()
		{
			RevertForSave();
			CurrentSequence = null;
		}

		private void TryToFixObserverTimelines(USSequencer sequence)
		{
			foreach(var timelineContainers in sequence.TimelineContainers)
			{
				foreach(var timeline in timelineContainers.Timelines)
				{
					var observerTimeline = timeline as USTimelineObserver;
					if(!observerTimeline)
						continue;
					
					observerTimeline.FixCameraReferences();
				}
			}
		}
		
		private void TryToFixPropertyTimelines(USSequencer sequence)
		{
			foreach(var timelineContainers in sequence.TimelineContainers)
			{
				foreach(var timeline in timelineContainers.Timelines)
				{
					var propertyTimeline = timeline as USTimelineProperty;
					if(!propertyTimeline)
						continue;

					propertyTimeline.TryToFixComponentReferences();
				}
			}
		}
		
		public void ExternalModification()
		{
			ContentRenderer.ExternalModification();
		}
		
		private void OnSceneGUI()
		{
			if(!USPreferenceWindow.RenderHierarchyGizmos)
				return;
			
			if(ContentRenderer != null)
				ContentRenderer.OnSceneGUI();
		}
		
		private static void OnScene(SceneView sceneview)
		{
			if(thisWindow != null)
				thisWindow.OnSceneGUI();
		}

		private void OnHierarchyChanged()
		{
			if(Application.isPlaying)
				return;

#if WELLFIRED_INTERNAL
			Debug.Log("Hierarchy Changed");
#endif

			if(CurrentSequence)
				CurrentSequence.ResetCachedData();
		}

		#region Weird Popup Hack
		private static Rect popupLocation;
		private static bool showPopup;
		private static List<PropertyBox> propertiesToShow;
		private static Action<IEnumerable<PropertyBox>> commitModifications;

		public static void ShowPopupForProperties(Rect animateButton, List<PropertyBox> properties, Action<IEnumerable<PropertyBox>> commitModificationsDelegate)
		{
			popupLocation = animateButton;
			propertiesToShow = properties;
			showPopup = true;
			commitModifications = commitModificationsDelegate;
		}

		private void ActuallyShowPopup()
		{
			PropertyBoxPopup.ShowAtPosition(popupLocation, propertiesToShow, commitModifications);
			showPopup = false;
		}

		public bool HasOpenPopup {
			get { return PropertyBoxPopup.IsOpen || PropertyBoxPopup.OpenCooldown > 0.0f; }
		}
		#endregion
	}
}
