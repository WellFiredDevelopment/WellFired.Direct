using UnityEngine;
using System.Collections;

namespace WellFired
{
	/// <summary>
	/// Our internal representation of a keyframe.
	/// </summary>
	[System.Serializable]
	public class USInternalKeyframe : ScriptableObject 
	{
		#region Member Variables
		[SerializeField]
		private float value;
		
		[SerializeField]
		private float time;
		
		[SerializeField]
		private float inTangent;
		
		[SerializeField]
		private float outTangent;
		
		[SerializeField]
		private bool brokenTangents;
		
		[SerializeField]
		public USInternalCurve curve;
		#endregion
		
		#region Properties
		public float Value
		{
			get { return value; }
			set 
			{ 
				this.value = value;
				if(curve)
					curve.BuildAnimationCurveFromInternalCurve();
			}
		}
		
		public float Time
		{
			get { return time; }
			set 
			{ 
				time = value;
				if(curve)
				{
					time = Mathf.Max(0.0f, time);
					if(curve.Duration < time)
						curve.Duration = time;
					curve.BuildAnimationCurveFromInternalCurve();
				}
			}
		}
		
		public float InTangent
		{
			get { return inTangent; }
			set 
			{ 
				inTangent = value;
				if(curve)
					curve.BuildAnimationCurveFromInternalCurve();
			}
		}
		
		public float OutTangent
		{
			get { return outTangent; }
			set 
			{ 
				outTangent = value;
				if(curve)
					curve.BuildAnimationCurveFromInternalCurve();
			}
		}
		
		public bool BrokenTangents
		{
			get { return brokenTangents; }
			set 
			{ 
				brokenTangents = value;
				if(curve)
					curve.BuildAnimationCurveFromInternalCurve();
			}
		}
		#endregion
	
		private void OnEnable()
		{
	
		}
		
	    public void ConvertFrom(Keyframe keyframe)
	    {
			value = keyframe.value;
			time = keyframe.time;
			
			inTangent = keyframe.inTangent;
			outTangent = keyframe.outTangent;
	    }
		
		// At the moment, we rely on the unity AnimationCurve to do this work.
		public void Smooth()
		{
			if(!curve)
				return;
			
			// Set to an invalid index by default (Unity's curve default invalid index).
			var index = -1;
			
			for(var n = 0; n < curve.UnityAnimationCurve.keys.Length; n++)
			{
				var keyframe = curve.UnityAnimationCurve.keys[n];
				if(Mathf.Approximately(keyframe.time, time))
					index = n;
				
				if(index != -1)
					break;
			}
			
			// Didn't find the key, bail.
			if(index == -1)
				return;
			
			// OK, do the smoothing.
			curve.UnityAnimationCurve.SmoothTangents(index, 0.0f);
			curve.BuildInternalCurveFromAnimationCurve();
		}
		
		public void Flatten()
		{		
			if(!curve)
				return;
			
			inTangent = 0;
			outTangent = 0;
			
			// We must update the animation curve.
			curve.BuildAnimationCurveFromInternalCurve();
		}
		
		public void RightTangentLinear()
		{
			// There is no point in editting this data without a curve.
			if(!curve)
				return;
		
			// Is there a next keyframe?
			var nextKeyframe = curve.GetNextKeyframe(this);	
			if(nextKeyframe == null)
				return;
						
			// Perform the calculation
			outTangent = (nextKeyframe.value - value) / (nextKeyframe.time - time);
			brokenTangents = true;
			
			// We must update the animation curve.
			curve.BuildAnimationCurveFromInternalCurve();
		}
		
		public void RightTangentConstant()
		{
			outTangent = Mathf.Infinity;
			
			// We must update the animation curve.
			curve.BuildAnimationCurveFromInternalCurve();
		}
		
		public void LeftTangentLinear()
		{
			// There is no point in editting this data without a curve.
			if(!curve)
				return;
		
			// Is there a next keyframe?
			var prevKeyframe = curve.GetPrevKeyframe(this);	
			if(prevKeyframe == null)
				return;
						
			// Perform the calculation
			inTangent = (prevKeyframe.value - value) / (prevKeyframe.time - time);
			brokenTangents = true;
			
			// We must update the animation curve.
			curve.BuildAnimationCurveFromInternalCurve();
		}
		
		public void LeftTangentConstant()
		{
			inTangent = Mathf.Infinity;
			
			// We must update the animation curve.
			curve.BuildAnimationCurveFromInternalCurve();
		}
		
		public void BothTangentLinear()
		{
			LeftTangentLinear();
			RightTangentLinear();
		}

		public void BothTangentConstant()
		{
			LeftTangentConstant();
			RightTangentConstant();
		}
	}
}