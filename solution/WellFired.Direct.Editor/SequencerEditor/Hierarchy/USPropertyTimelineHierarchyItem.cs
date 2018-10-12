#if WELLFIRED_INTERNAL
#define DEBUG_PROPERTYTIMELINE
#endif

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WellFired
{
	[Serializable]
	[USCustomTimelineHierarchyItem(typeof(USTimelineProperty), "Property Timeline")]
	public class USPropertyTimelineHierarchyItem : IUSTimelineHierarchyItem 
	{
		private static readonly float DEFAULT_PROPERTY_TIMELINE_HEIGHT = 204.0f;

		public override bool ShouldDisplayMoreButton
		{
			get { return true; }
			set { ; }
		}

		public override bool ShouldDisplayShrinkButton
		{
			get { return true; }
			set { ; }
		}
		
		public override bool ShouldDisplayGrowButton
		{
			get { return true; }
			set { ; }
		}
		
		public override float DefaultItemHeight
		{
			get { return DEFAULT_PROPERTY_TIMELINE_HEIGHT; }
		}

		public override Transform PingableObject
		{
			get { return PropertyTimeline.AffectedObject; }
			set { ; }
		}

		[SerializeField]
		public USTimelineProperty PropertyTimeline
		{
			get { return BaseTimeline as USTimelineProperty; }
			set { BaseTimeline = value; }
		}
		
		[SerializeField]
		private AnimationCurveEditor _curveEditor;
		public AnimationCurveEditor CurveEditor
		{
			get { return _curveEditor; }
			set { _curveEditor = value; }
		}
	
		[SerializeField]
		public List<ISelectableContainer> ISelectableContainers
		{
			get;
			set;
		}
	
		[SerializeField]
		public float MaxHeight
		{
			get;
			private set;
		}
	
		[SerializeField]
		private List<PropertyBox> propertyBoxes;

		private Rect animateButton;
		private PropertyBox removingProperty;
		private PropertyBoxPopup propertyBoxHierarchy;

		public override void OnEnable()
		{
			base.OnEnable();
	
			removingProperty = null;
			if(CurveEditor == null)
				CurveEditor = CreateInstance<AnimationCurveEditor>();
			
			if(propertyBoxes == null)
				propertyBoxes = new List<PropertyBox>();

			Undo.undoRedoPerformed -= CreatePropertyBoxes;
			Undo.undoRedoPerformed += CreatePropertyBoxes;
			Undo.postprocessModifications -= PostProcess;
			Undo.postprocessModifications += PostProcess;
	
			if(ISelectableContainers == null)
			{
				ISelectableContainers = new List<ISelectableContainer>();
				ISelectableContainers.Add(CurveEditor);
			}
		}

		private void OnDestroy()
		{
			Undo.undoRedoPerformed -= CreatePropertyBoxes;
			Undo.postprocessModifications -= PostProcess;

			Destroy (_curveEditor);
		}
		
		public override void DoGUI(int depth)
		{
			BaseTimeline.ShouldRenderGizmos = IsExpanded && USPreferenceWindow.RenderHierarchyGizmos;
	
			using(new Shared.GUIBeginHorizontal())
			{
				using(new Shared.GUIBeginVertical(GUILayout.MaxWidth(FloatingWidth)))
				{
					FloatingOnGUI(depth);

					if(IsExpanded)
					{
						var propertyArea = FloatingBackgroundRect;
						propertyArea.y += ItemHeightStep;
						propertyArea.x = GetXOffsetForDepth(depth + 1);
						propertyArea.width -= propertyArea.x;

						using(new Shared.GUIBeginArea(propertyArea))
						{
							foreach(var propertyBox in propertyBoxes)
							{
								using(new Shared.GUIBeginHorizontal())
								{
									propertyBox.OnGUI();

									using(new Shared.GUIChangeColor(Color.red))
									{
										if(GUILayout.Button("-", GUILayout.Width(20.0f)))
											removingProperty = propertyBox;
									}

									// This can happen during undo redo.
									if(propertyBox.PropertyFieldInfo == null)
										continue;

									var preFix = propertyBox.PropertyFieldInfo.Name;
									var propertyInfo = PropertyTimeline.GetProperty(preFix, propertyBox.PropertyFieldInfo.Component);
									using(new Shared.GUIChangeColor(propertyInfo.UseCurrentValue ? Color.red : GUI.color))
									{
										if(GUILayout.Button ("C"))
											propertyInfo.UseCurrentValue = !propertyInfo.UseCurrentValue;
									}
								}
							}

							if(GUILayout.Button("Animate"))
							{
								if(Event.current.type == EventType.Repaint)
									animateButton = GUILayoutUtility.GetLastRect();

								var components = PropertyTimeline.AffectedObject.GetComponents<Component>().ToList();

								var allPropertyBoxes = new List<PropertyBox>();
								foreach(var component in components)
								{
									var properties = component.GetType().GetProperties().Where(property => !PropertyFieldInfoUtility.ShouldIgnoreProperty(property, component));
									var fields = component.GetType().GetFields().Where(field => !PropertyFieldInfoUtility.shouldIgnoreField(field, component));
									
									foreach(var property in properties)
										allPropertyBoxes.Add(new PropertyBox(new PropertyFieldInfo(component, property), true));
									
									foreach(var field in fields)
										allPropertyBoxes.Add(new PropertyBox(new PropertyFieldInfo(component, field), true));
								}

								foreach(var propertyBox in propertyBoxes)
								{
									var overlappingProperties = allPropertyBoxes.Where(innerPropertyBox => innerPropertyBox.Component == propertyBox.Component && innerPropertyBox.PropertyName == propertyBox.PropertyName);
									foreach(var overlappingProperty in overlappingProperties)
										overlappingProperty.AddingProperty = true;
								}

								USWindow.ShowPopupForProperties(animateButton, allPropertyBoxes, CommitModifications);
							}
						}
					}
				}
	
				if(Event.current.type == EventType.Repaint)
				{
					var newMaxHeight = GUILayoutUtility.GetLastRect().height;
					
					if(MaxHeight != newMaxHeight)
					{
						EditorWindow.Repaint();
						MaxHeight = newMaxHeight;
					}
				}
	 
				ContentOnGUI();
			}

			if(removingProperty != null)
				RemoveProperty(removingProperty);

			removingProperty = null;

			if(Event.current.commandName == "UndoRedoPerformed")
			{
				return;
			}

			if(CurveEditor.AreCurvesDirty)
			{
				PropertyTimeline.Process(PropertyTimeline.Sequence.RunningTime, PropertyTimeline.Sequence.PlaybackRate);
				CurveEditor.AreCurvesDirty = false;
			}
		}
		
		protected override void FloatingOnGUI(int depth)
		{
			GUILayout.Box("", FloatingBackground, GUILayout.MaxWidth(FloatingWidth), GUILayout.Height(ItemHeight));
			
			if(Event.current.type == EventType.Repaint)
			{
				var lastRect = GUILayoutUtility.GetLastRect();
				lastRect.x += GetXOffsetForDepth(depth);
				lastRect.width -= GetXOffsetForDepth(depth);
				if(FloatingBackgroundRect != lastRect)
				{
					EditorWindow.Repaint();
					FloatingBackgroundRect = lastRect;
				}
			}
	
			var wasLabelPressed = false;
			var newExpandedState = USEditor.FoldoutLabel(FloatingBackgroundRect, IsExpanded, PropertyTimeline.name, out wasLabelPressed);
			if(newExpandedState != IsExpanded)
			{
				USUndoManager.PropertyChange(this, "Foldout");
				IsExpanded = newExpandedState;
				EditorWindow.Repaint();
			}

			base.FloatingOnGUI(depth);
		}

		private void ContentOnGUI()
		{
			CurveEditor.MaxHeight = MaxHeight;
			CurveEditor.Duration = PropertyTimeline.Sequence.Duration;

			if(IsExpanded)
				CurveEditor.OnGUI();
			else
				CurveEditor.OnCollapsedGUI();
			
			if(Event.current.type == EventType.Repaint)
				ContentBackgroundRect = GUILayoutUtility.GetLastRect();
		}
		
		public override List<ISelectableContainer> GetISelectableContainers()
		{
			return ISelectableContainers;
		}
	
		public override void CheckConsistency()
		{
			CurveEditor.EditorWindow = EditorWindow;
			CurveEditor.CurrentXMarkerDist = CurrentXMarkerDist;
			CurveEditor.XScale = XScale;
			CurveEditor.XScroll = XScroll;
			CurveEditor.YScroll = YScroll;
			base.CheckConsistency();
		}
	
		public override void StoreBaseState()
		{
			foreach(var property in PropertyTimeline.Properties)
				property.StoreBaseState();
	
			base.StoreBaseState();
		}
		
		public override void RestoreBaseState()
		{
			if(PropertyTimeline == null)
			{
#if DEBUG_PROPERTYTIMELINE
				Debug.LogWarning("This should never be null, unless doing an undo");
#endif
				return;
			}

			foreach(var property in PropertyTimeline.Properties)
			{
				if(property == null)
				{
					Debug.LogWarning("Something is null on : " + PropertyTimeline, PropertyTimeline.gameObject);
					continue;
				}
				property.RestoreBaseState();
			}

			base.RestoreBaseState();
		}
		
		public override void ExternalModification()
		{
			base.ExternalModification();
			CurveEditor.RebuildCurvesOnNextGUI = true;
			CurveEditor.AreCurvesDirty = true;
			EditorWindow.Repaint();
		}
		
		public override void Initialize(USTimelineBase timeline)
		{
			base.Initialize(timeline);

			if(PropertyTimeline.AffectedObject == null)
				return;

			CurveEditor.Curves = PropertyTimeline.Properties.SelectMany(property => property.curves).ToList();
			CreatePropertyBoxes();
		}

		private void CreatePropertyBoxes()
		{
			propertyBoxes.Clear();

			if(PropertyTimeline == null)
				return;

			foreach(var property in PropertyTimeline.Properties)
			{
				if(property)
				{
					if(property.propertyInfo != null)
						propertyBoxes.Add(new PropertyBox(new PropertyFieldInfo(property.Component, property.propertyInfo), false));
					else if(property.fieldInfo != null)
						propertyBoxes.Add(new PropertyBox(new PropertyFieldInfo(property.Component, property.fieldInfo), false));
				}
			}

			propertyBoxes.ForEach(propertyBox => {
				propertyBox.ShouldShowFavouriteButton = false;
				propertyBox.ShouldShowAddRemove = false;
			});
		}

		public float GetPreviousShownKeyframeTime(float runningTime)
		{
			throw new NotImplementedException();
		}

		public float GetNextShownKeyframeTime(float runningTime)
		{
			throw new NotImplementedException();
		}

		private UndoPropertyModification[] PostProcess(UndoPropertyModification[] modifications)
		{
			if(PropertyTimeline && PropertyTimeline.Sequence && PropertyTimeline.Sequence.IsPlaying)
				return modifications;

			if(!AnimationHelper.IsInAnimationMode)
				return modifications;

			if(!USPreferenceWindow.AutoKeyframing)
				return modifications;

			if(USWindow.IsScrubbing)
				return modifications;

			try
			{
				var propertyModifications = ExtractRecordableModifications(modifications);
				foreach(var modifiedProperty in propertyModifications)
				{
					var modifiedCurves = modifiedProperty.GetModifiedCurvesAtTime(PropertyTimeline.Sequence.RunningTime);
					foreach(var modifiedCurve in modifiedCurves)
						CurveEditor.AddKeyframeAtTime(modifiedCurve);
				}
			}
			catch(Exception e)
			{
				Debug.Log(e);
			}

			return modifications;
		}

		private IEnumerable<USPropertyInfo> ExtractRecordableModifications(UndoPropertyModification[] modifications)
		{
			var extractedModifications = new List<USPropertyInfo>();
			foreach(var modification in modifications)
			{
				if(PropertyTimeline == null || PropertyTimeline.AffectedObject == null)
					continue;

				var binding = default(EditorCurveBinding);
				AnimationUtility.PropertyModificationToEditorCurveBinding(modification.currentValue, PropertyTimeline.AffectedObject.gameObject, out binding);

				foreach(var propertyInfo in PropertyTimeline.Properties)
				{
					if(string.IsNullOrEmpty(propertyInfo.InternalName))
						continue;

					if(!binding.propertyName.Contains(propertyInfo.InternalName))
						continue;

					extractedModifications.Add(propertyInfo);
					break;
				}
			}

			return extractedModifications;
		}

		private void AddProperty(PropertyBox propertyBox)
		{
			Debug.Log("Adding Property " + propertyBox);
			
			var usPropertyInfo = CreateInstance<USPropertyInfo>();
			
			USUndoManager.RegisterCreatedObjectUndo(usPropertyInfo, "Add Curve");
			USUndoManager.RegisterCompleteObjectUndo(PropertyTimeline, "Add Curve");
			USUndoManager.RegisterCompleteObjectUndo(this, "Add Curve");
			USUndoManager.RegisterCompleteObjectUndo(CurveEditor, "Add Curve");
			
			object propertyValue = null;
			usPropertyInfo.Component = propertyBox.PropertyFieldInfo.Component;
			
			if(propertyBox.PropertyFieldInfo.Property != null)
			{
				usPropertyInfo.propertyInfo = propertyBox.PropertyFieldInfo.Property;
				propertyValue = propertyBox.PropertyFieldInfo.Property.GetValue(propertyBox.PropertyFieldInfo.Component, null);
			}
			else if(propertyBox.PropertyFieldInfo.Field != null)
			{
				usPropertyInfo.fieldInfo = propertyBox.PropertyFieldInfo.Field;
				propertyValue = propertyBox.PropertyFieldInfo.Field.GetValue(propertyBox.PropertyFieldInfo.Component);
			}
			
			usPropertyInfo.InternalName = propertyBox.PropertyFieldInfo.MappedType;
			usPropertyInfo.CreatePropertyInfo(USPropertyInfo.GetMappedType(propertyValue.GetType()));
			usPropertyInfo.AddKeyframe(propertyValue, 0.0f, CurveAutoTangentModes.None);
			usPropertyInfo.AddKeyframe(propertyValue, PropertyTimeline.Sequence.Duration, CurveAutoTangentModes.None);
			PropertyTimeline.AddProperty(usPropertyInfo);
			
			usPropertyInfo.StoreBaseState();

			var newCurves = CurveEditor.Curves;
			newCurves.AddRange(usPropertyInfo.curves);
			CurveEditor.Curves = newCurves;

			propertyBox.ShouldShowFavouriteButton = false;
			propertyBox.ShouldShowAddRemove = false;
			propertyBoxes.Add(propertyBox);
		}

		private void RemoveProperty(PropertyBox propertyBox)
		{
			Debug.Log("Removing Property " + propertyBox);
			USUndoManager.RegisterCompleteObjectUndo(PropertyTimeline, "Remove Curve");
			USUndoManager.RegisterCompleteObjectUndo(this, "Remove Curve");
			USUndoManager.PropertyChange(CurveEditor, "Add Curve");
				
			propertyBoxes.Remove(propertyBox);
				
			var preFix = propertyBox.PropertyName;
			var propertyInfo = PropertyTimeline.GetProperty(preFix, propertyBox.Component);
				
			propertyInfo.curves.ForEach(curve => CurveEditor.Curves.Remove(curve));
				
			propertyInfo.RestoreBaseState();
			PropertyTimeline.RemoveProperty(propertyInfo);
			USUndoManager.DestroyImmediate(propertyInfo);
		}

		private void CommitModifications(IEnumerable<PropertyBox> modifications)
		{
			foreach(var modification in modifications)
			{
				if(modification.AddingProperty)
					AddProperty(modification);
				else
					RemoveProperty(modification);
			}
		}
	}
}