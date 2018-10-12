using UnityEngine;
using UnityEditor;
using System.Collections;

namespace WellFired
{
	[CustomEditor(typeof(USObserverKeyframe))]
	[CanEditMultipleObjects()]
	public class USObserverKeyframeInspector : Editor 
	{
	    SerializedProperty FireTime;
		SerializedProperty OurCamera;
		SerializedProperty TransitionType;
		SerializedProperty TransitionDuration;
		SerializedProperty AdditionalSourceCameras;
		SerializedProperty AdditionalDestinationCameras;
		
		void OnEnable() 
		{
	        // Setup the SerializedProperties
	        FireTime = serializedObject.FindProperty("fireTime");
			OurCamera = serializedObject.FindProperty("camera");
			TransitionType = serializedObject.FindProperty("transitionType");
			TransitionDuration = serializedObject.FindProperty("transitionDuration");
			AdditionalSourceCameras = serializedObject.FindProperty("additionalSourceCameras");
			AdditionalDestinationCameras = serializedObject.FindProperty("additionalDestinationCameras");
	    }
		
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			
			EditorGUILayout.PropertyField(FireTime, new GUIContent("Fire Time"));
			EditorGUILayout.PropertyField(OurCamera, new GUIContent("Camera"));

			EditorGUILayout.Space();

			EditorGUILayout.HelpBox("Transitions may not preview with 100% accuracy, but your build players will always look perfect.", MessageType.Info);

			EditorGUILayout.PropertyField(TransitionType, new GUIContent("Transition Type"));

			var transitionType = (Shared.TypeOfTransition)TransitionType.enumValueIndex;
			var transitionDuration = TransitionDuration.floatValue;
			if(transitionType != Shared.TypeOfTransition.Cut)
			{
				EditorGUILayout.PropertyField(TransitionDuration, new GUIContent("Transition Duration"));

				if(transitionDuration <= 0.0f)
					TransitionDuration.floatValue = Shared.TransitionHelper.DefaultTransitionTimeFor(transitionType);
			}

			EditorGUILayout.Space();
			
			EditorGUILayout.HelpBox("Extra cameras you might want to render during the transition (UI/ Ignored, etc)", MessageType.None);
			
			EditorGUILayout.PropertyField(AdditionalSourceCameras, new GUIContent("Additional Source Cameras"), true);
			EditorGUILayout.PropertyField(AdditionalDestinationCameras, new GUIContent("Additional Destination Cameras"), true);
			
			EditorGUILayout.Space();
			
			if(GUILayout.Button("Select Camera"))
				Selection.activeObject = OurCamera.objectReferenceValue;
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}