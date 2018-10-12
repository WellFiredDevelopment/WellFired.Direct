using UnityEngine;
using UnityEditor;
using System.Collections;

namespace WellFired.Shared
{
	public static class HandlesUtility 
	{
		static private Color xAxisColor = new Color(0.858823538f, 0.243137255f, 0.113725491f, 0.93f);
		static private Color yAxisColor = new Color(0.6039216f, 0.9529412f, 0.282352954f, 0.93f);
		static private Color zAxisColor = new Color(0.227450982f, 0.478431374f, 0.972549f, 0.93f);
		static private Color centerColor = new Color(0.8f, 0.8f, 0.8f, 0.93f);
		
		static public Vector3 PositionHandle(Vector3 position, Quaternion rotation)
		{
			var snapX = EditorPrefs.GetFloat("MoveSnapX");
			var snapY = EditorPrefs.GetFloat("MoveSnapY");
			var snapZ = EditorPrefs.GetFloat("MoveSnapZ");
			var snapMove = new Vector3(snapX, snapY, snapZ);
			
			var handleSize = GetHandleSize(position);
			var color = Handles.color;
			Handles.color = xAxisColor;
			position = Handles.Slider(position, rotation * Vector3.right, handleSize, new Handles.DrawCapFunction(Handles.ArrowCap), snapX);
			Handles.color = yAxisColor;
			position = Handles.Slider(position, rotation * Vector3.up, handleSize, new Handles.DrawCapFunction(Handles.ArrowCap), snapY);
			Handles.color = zAxisColor;
			position = Handles.Slider(position, rotation * Vector3.forward, handleSize, new Handles.DrawCapFunction(Handles.ArrowCap), snapZ);
			Handles.color = centerColor;
			position = Handles.FreeMoveHandle(position, rotation, handleSize * 0.15f, snapMove, new Handles.DrawCapFunction(Handles.RectangleCap));
			Handles.color = color;
			return position;
		}
		
		static public float GetHandleSize(Vector3 position)
		{
			var current = Camera.current;
			position = Handles.matrix.MultiplyPoint(position);
			if (current)
			{
				var transform = current.transform;
				var position2 = transform.position;
				var z = Vector3.Dot (position - position2, transform.TransformDirection (new Vector3 (0f, 0f, 1f)));
				var a = current.WorldToScreenPoint (position2 + transform.TransformDirection (new Vector3 (0f, 0f, z)));
				var b = current.WorldToScreenPoint (position2 + transform.TransformDirection (new Vector3 (1f, 0f, z)));
				var magnitude = (a - b).magnitude;
				return 40f / Mathf.Max (magnitude, 0.0001f);
			}
			return 20f;
		}
	}
}