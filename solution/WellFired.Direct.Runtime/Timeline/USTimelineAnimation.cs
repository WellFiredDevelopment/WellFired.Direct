using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WellFired
{
	[Serializable]
	public class USTimelineAnimation : USTimelineBase 
	{
		private Dictionary<int, List<AnimatorClipInfo>> initialAnimationInfo = new Dictionary<int, List<AnimatorClipInfo>>();
		private Dictionary<int, AnimatorStateInfo> initialAnimatorStateInfo = new Dictionary<int, AnimatorStateInfo>();

		[SerializeField]
		private List<AnimationTrack> animationsTracks = new List<AnimationTrack>();
		public List<AnimationTrack> AnimationTracks
		{
			get { return animationsTracks; }
			private set { animationsTracks = value; }
		}
		
		[SerializeField]
		private Animator animator;
		private Animator Animator
		{
			get
			{
				if(animator == null)
					animator = AffectedObject.GetComponent<Animator>();
				
				return animator;
			}
		}

		[SerializeField]
		private USTimelineAnimationEditorRunner editorRunner;
		private USTimelineAnimationEditorRunner EditorRunner
		{
			get
			{
				if(editorRunner == null)
				{
					editorRunner = ScriptableObject.CreateInstance<USTimelineAnimationEditorRunner>();
					editorRunner.Animator = Animator;
					editorRunner.AnimationTimeline = this;
				}
				return editorRunner;
			}
		}

		[SerializeField]
		private USTimelineAnimationGameRunner gameRunner;
		private USTimelineAnimationGameRunner GameRunner
		{
			get
			{
				if(gameRunner == null)
				{
					gameRunner = ScriptableObject.CreateInstance<USTimelineAnimationGameRunner>();
					gameRunner.Animator = Animator;
					gameRunner.AnimationTimeline = this;
				}
				return gameRunner;
			}
		}

		[SerializeField]
		private AnimationTimelineController animationTimelineController;

		[SerializeField]
		private Vector3 sourcePosition;

		[SerializeField]
		private Quaternion sourceOrientation;

		[SerializeField]
		private float sourceSpeed;

		private bool previousEnabled;

		public override void StartTimeline() 
		{
			sourcePosition = AffectedObject.transform.localPosition;
			sourceOrientation = AffectedObject.transform.localRotation;
			sourceSpeed = Animator.speed;

			for(var layer = 0; layer < Animator.layerCount; layer++)
			{
				initialAnimationInfo[layer] = new List<AnimatorClipInfo>();
				var values = Animator.GetCurrentAnimatorClipInfo(layer);
				foreach(var value in values)
				{
					initialAnimationInfo[layer].Add (value);
				}
			}
			for(var layer = 0; layer < Animator.layerCount; layer++)
				initialAnimatorStateInfo[layer] = Animator.GetCurrentAnimatorStateInfo(layer);

			if(Animator.applyRootMotion)
			{
				var existingComponents = Animator.gameObject.GetComponents<AnimationTimelineController>();
				foreach(var existingComponent in existingComponents)
					DestroyImmediate(existingComponent);

				animationTimelineController = Animator.gameObject.AddComponent<AnimationTimelineController>();
				animationTimelineController.AnimationTimeline = this;
			}

			previousEnabled = Animator.enabled;
			Animator.enabled = false;
		}

		public override void StopTimeline() 
		{ 	
			if(animationTimelineController)
				DestroyImmediate(animationTimelineController);
			animationTimelineController = null;

			Animator.Update(-Sequence.RunningTime);
			Animator.StopPlayback();

			ResetAnimation();

			initialAnimationInfo.Clear();
			initialAnimatorStateInfo.Clear();

			GameRunner.Stop();
			EditorRunner.Stop();

			Animator.speed = sourceSpeed;
			
			Animator.enabled = previousEnabled;
		}

		public override void EndTimeline ()
		{
			base.EndTimeline ();

			Animator.enabled = previousEnabled;
		}

		public void ResetAnimation()
		{	
			for(var layer = 0; layer < Animator.layerCount; layer++)
			{
				if(!initialAnimatorStateInfo.ContainsKey(layer))
					continue;
				
				Animator.Play(initialAnimatorStateInfo[layer].fullPathHash, layer, initialAnimatorStateInfo[layer].normalizedTime);
				Animator.Update(0.0f);
			}

			if(Sequence.RunningTime > 0.0f)
			{
				AffectedObject.transform.localPosition = sourcePosition;
				AffectedObject.transform.localRotation = sourceOrientation;
			}
		}

		public override void Process(float sequenceTime, float playbackRate)
		{
			var shouldTickAnimator = true;//!Application.isPlaying;
			if(shouldTickAnimator)
				EditorRunner.Process(sequenceTime, playbackRate);
			else
				GameRunner.Process(sequenceTime, playbackRate);
		}

		public override void PauseTimeline()
		{
			var shouldTickAnimator = true;//!Application.isPlaying;
			if(shouldTickAnimator)
				EditorRunner.PauseTimeline();
			else
				GameRunner.PauseTimeline();
		}

		public void AddTrack(AnimationTrack animationTrack)
		{
			animationsTracks.Add(animationTrack);
		}
		
		public void RemoveTrack(AnimationTrack animationTrack)
		{
			animationsTracks.Remove(animationTrack);
		}

		public void SetTracks(List<AnimationTrack> animationTracks)
		{
			AnimationTracks = animationTracks;
		}
	}
}