﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WellFired
{
	[Serializable]
	public partial class EventEditor : ScriptableObject, ISelectableContainer
	{
		[SerializeField] 
		private List<EventRenderData> cachedEventRenderData;
	
		[SerializeField]
		private USTimelineEvent eventTimeline;
		public USTimelineEvent EventTimeline
		{
			get { return eventTimeline; }
			set { eventTimeline = value; }
		}
		
		[SerializeField]
		public Rect DisplayArea
		{
			get;
			private set;
		}
		
		[SerializeField]
		private Rect HeaderArea
		{
			get;
			set;
		}
		
		public float Duration
		{
			get;
			set;
		}
		
		public float XScale
		{
			get;
			set;
		}
		
		public float XScroll
		{
			get;
			set;
		}
		
		public float YScroll
		{
			get;
			set;
		}
		
		private GUIStyle TimelineBackground
		{
			get
			{
				return USEditorUtility.USeqSkin.GetStyle("TimelinePaneBackground");
			}
		}
	
		private void OnEnable()
		{
			hideFlags = HideFlags.HideAndDontSave;
	
			if(cachedEventRenderData == null)
				cachedEventRenderData = new List<EventRenderData>();
	
			if(SelectedObjects == null)
				SelectedObjects = new List<UnityEngine.Object>();
	
			Undo.undoRedoPerformed -= UndoRedoCallback;
			Undo.undoRedoPerformed += UndoRedoCallback;
			EditorApplication.update -= Update;
			EditorApplication.update += Update;
		}
		
		private void OnDestroy()
		{
			Undo.undoRedoPerformed -= UndoRedoCallback;
			EditorApplication.update -= Update;
		}
	
		private void UndoRedoCallback()
		{
			
		}
	
		public void InitializeWithEvents(IEnumerable<USEventBase> events)
		{
			foreach(var evt in events)
			{
				var cachedData = CreateInstance<EventRenderData>();
				cachedData.Initialize(evt);

				cachedEventRenderData.Add(cachedData);
			}
		}
	
		private void Update()
		{
	
		}
		
		public void OnGUI()
		{	
			GUILayout.Box("", TimelineBackground, GUILayout.MaxHeight(50.0f), GUILayout.ExpandWidth(true));
			if(Event.current.type == EventType.Repaint)
				DisplayArea = GUILayoutUtility.GetLastRect();
	
			var sortedEvents = cachedEventRenderData.OrderBy(element => element.Event.FireTime).ToList();
			for(var index = 0; index < sortedEvents.Count(); index++)
			{
				var data = sortedEvents[index];
				var evt = data.Event;
	
				var xPos = DisplayArea.width * (evt.FireTime / Duration);
				data.RenderPosition = new Vector2(DisplayArea.x + ((xPos * XScale) - XScroll), DisplayArea.y);
				data.RenderRect = new Rect(DisplayArea.x + ((xPos * XScale) - XScroll), DisplayArea.y, 4.0f, DisplayArea.height - 1.0f);
	
				using(new Shared.GUIChangeColor(SelectedObjects.Contains(evt) ? Color.yellow : GUI.color))
					data.RenderRect = data.EventEditor.DrawEventInTimeline(data.RenderRect, DisplayArea.x, DisplayArea.width, Duration, XScale, XScroll);
			}
	
			HandleEvent();
		}
	
		public void OnCollapsedGUI()
		{
			GUILayout.Box("", TimelineBackground, GUILayout.MaxHeight(17.0f), GUILayout.ExpandWidth(true));
			if(Event.current.type == EventType.Repaint)
				DisplayArea = GUILayoutUtility.GetLastRect();
	
			var sortedEvents = cachedEventRenderData.OrderBy(element => element.Event.FireTime).ToList();
			for(var index = 0; index < sortedEvents.Count(); index++)
			{
				var data = sortedEvents[index];
				var evt = sortedEvents[index].Event;
	
				var xPos = DisplayArea.width * (evt.FireTime / Duration);
				data.RenderPosition = new Vector2(DisplayArea.x + ((xPos * XScale) - XScroll), DisplayArea.y);
				data.RenderRect = new Rect(DisplayArea.x + ((xPos * XScale) - XScroll), DisplayArea.y, 4.0f, DisplayArea.height - 1.0f);
	
				using(new Shared.GUIChangeColor(SelectedObjects.Contains(evt) ? Color.yellow : GUI.color))
					data.EventEditor.DrawCollapsedEventInTimeline(data.RenderRect, DisplayArea.x, DisplayArea.width, Duration, XScale, XScroll);
			}
			
			HandleEvent();
		}
	
		private void HandleEvent()
		{
			var isContext = Event.current.type == EventType.MouseDown && Event.current.button == 1;
	
			if(!isContext)
				return;
			
			var contextMenu = new GenericMenu();
	
			foreach(var data in cachedEventRenderData)
			{
				if(data.RenderRect.Contains(Event.current.mousePosition))
				{
					contextMenu.AddItem(new GUIContent("Remove Event"), false, () => { RemoveEvent(data.Event); });
					contextMenu.AddSeparator("");
					break;
				}
			}
			
			if(DisplayArea.Contains(Event.current.mousePosition))
			{
				var baseAdd = "Add Event/";
				var newTime = (((Event.current.mousePosition.x + XScroll - DisplayArea.x) / DisplayArea.width) * Duration) / XScale;

				var baseType = typeof(USEventBase);
				var types = USEditorUtility.GetAllSubTypes(baseType).Where(type => type.IsSubclassOf(baseType));
				foreach (var type in types)
				{
					var fullAdd = baseAdd;
					var customAttributes = type.GetCustomAttributes(true).Where(attr => attr is USequencerEventAttribute).Cast<USequencerEventAttribute>();
					foreach (var customAttribute in customAttributes)
						fullAdd += customAttribute.EventPath;
	
					contextMenu.AddItem(new GUIContent(fullAdd), false, (obj) => AddEvent(newTime, (Type)obj), (object)type );
				}
			}
			
			if(DisplayArea.Contains(Event.current.mousePosition))
			{
				Event.current.Use();
				contextMenu.ShowAsContext();
			}
		}
	
		private void RemoveEvent(USEventBase baseEvent)
		{
			USUndoManager.RegisterCompleteObjectUndo(this, "Remove Event");
	
			EventRenderData foundEventData = null;
			foreach(var cachedRenderData in cachedEventRenderData)
				if(cachedRenderData.Event == baseEvent)
					foundEventData = cachedRenderData;
	
			cachedEventRenderData.Remove(foundEventData);
	
			var removedGameObject = foundEventData.Event.gameObject;
			USUndoManager.DestroyImmediate(foundEventData);
			USUndoManager.DestroyImmediate(removedGameObject);
		}
	
		private void AddEvent(float time, Type type)
		{
			var eventGO = new GameObject(type.Name);
			eventGO.transform.parent = EventTimeline.transform;
			eventGO.transform.position = Vector3.zero;
			eventGO.transform.rotation = Quaternion.identity;
			USUndoManager.RegisterCreatedObjectUndo(eventGO, "Add Event");
	
			var eventComponent = eventGO.AddComponent(type) as USEventBase;
			eventComponent.FireTime = time;
			
			var cachedData = CreateInstance<EventRenderData>();
			cachedData.Initialize(eventComponent);

			USUndoManager.RegisterCreatedObjectUndo(cachedData, "Add Event");

			USUndoManager.RegisterCompleteObjectUndo(this, "Add Event");
			cachedEventRenderData.Add(cachedData);
		}
	}
}