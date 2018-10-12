using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WellFired.Shared
{
	public class BaseTransition 
	{
		private RenderTexture IntroRenderTexture 
		{
			get
			{
				if(introRenderTexture == null)
					introRenderTexture = new RenderTexture((int)TransitionHelper.MainGameViewSize.x, (int)TransitionHelper.MainGameViewSize.y, 24);
				
				return introRenderTexture;
			}
			set { ; }
		}
		
		private RenderTexture OutroRenderTexture 
		{
			get
			{
				if(outroRenderTexture == null)
					outroRenderTexture = new RenderTexture((int)TransitionHelper.MainGameViewSize.x, (int)TransitionHelper.MainGameViewSize.y, 24);
				
				return outroRenderTexture;
			}
			set { ; }
		}

		public Camera SourceCamera 
		{
			get { return sourceCamera; }
			set { sourceCamera = value; }
		}

		private List<Camera> additionalSourceCameras = new List<Camera>();
		private List<Camera> additionalDestinationCameras = new List<Camera>();
		
		private Camera sourceCamera;
		private Camera destinationCamera;
		private Material renderMaterial;
		private RenderTexture introRenderTexture;
		private RenderTexture outroRenderTexture;
		private bool shouldRender;
		private bool prevIntroCameraState;
		private bool prevOutroCameraState;
		private float ratio;

		public void InitializeTransition(Camera sourceCamera, Camera destinationCamera, List<Camera> additionalSourceCameras, List<Camera> additionalDestinationCameras, TypeOfTransition transitionType)
		{
			if(sourceCamera == null || destinationCamera == null)
				Debug.LogError(string.Format("Cannot create a transition with sourceCamera({0}) and destinationCamera({1}), one of them is null", sourceCamera, destinationCamera));

			renderMaterial = new Material(Resources.Load(string.Format("Transitions/WellFired{0}", transitionType.ToString()), typeof(Material)) as Material);

			if(renderMaterial == null)
				Debug.LogError(string.Format("Couldn't load render material for {0}", transitionType));

			this.sourceCamera = sourceCamera;
			this.destinationCamera = destinationCamera;

			this.additionalSourceCameras = additionalSourceCameras;
			this.additionalDestinationCameras = additionalDestinationCameras;

			prevIntroCameraState = this.sourceCamera.enabled;
			prevOutroCameraState = this.destinationCamera.enabled;
		}		

		public void ProcessTransitionFromOnGUI()
		{
			if(!shouldRender)
				return;

			renderMaterial.SetTexture("_SecondTex", OutroRenderTexture);
			renderMaterial.SetFloat("_Alpha", ratio);
			Graphics.Blit(IntroRenderTexture, default(RenderTexture), renderMaterial);
		}
		
		
		public void ProcessEventFromNoneOnGUI(float deltaTime, float duration)
		{
			sourceCamera.enabled = false;
			destinationCamera.enabled = false;

			sourceCamera.targetTexture = IntroRenderTexture;
			sourceCamera.Render();

			for(var cameraIndex = 0; cameraIndex < additionalSourceCameras.Count; cameraIndex++)
			{
				var camera = additionalSourceCameras[cameraIndex];
				if(!camera)
					continue;
				
				camera.targetTexture = IntroRenderTexture;
				camera.Render();
			}
	
			destinationCamera.targetTexture = OutroRenderTexture;
			destinationCamera.Render();

			for(var cameraIndex = 0; cameraIndex < additionalDestinationCameras.Count; cameraIndex++)
			{
				var camera = additionalDestinationCameras[cameraIndex];
				if(!camera)
					continue;
				
				camera.targetTexture = OutroRenderTexture;
				camera.Render();
			}

			ratio = 1.0f - (deltaTime / duration);
			shouldRender = true;
		}

		public void TransitionComplete()
		{
			shouldRender = false;

			destinationCamera.enabled = true;
			destinationCamera.targetTexture = null;

			sourceCamera.enabled = false;
			sourceCamera.targetTexture = null;

			foreach(var camera in additionalSourceCameras)
			{
				if(!camera)
					continue;
				
				camera.targetTexture = null;
			}
			foreach(var camera in additionalDestinationCameras)
			{
				if(!camera)
					continue;
				
				camera.targetTexture = null;
			}
		}

		public void RevertTransition()
		{
			if(sourceCamera != null)
			{
				sourceCamera.enabled = prevIntroCameraState;
				sourceCamera.targetTexture = null;
			}

			if(destinationCamera != null)
			{
				destinationCamera.enabled = prevOutroCameraState;
				destinationCamera.targetTexture = null;
			}
			
			foreach(var camera in additionalSourceCameras)
			{
				if(!camera)
					continue;
				
				camera.targetTexture = null;
			}

			foreach(var camera in additionalDestinationCameras)
			{
				if(!camera)
					continue;
				
				camera.targetTexture = null;
			}

			Object.DestroyImmediate(IntroRenderTexture);
			Object.DestroyImmediate(OutroRenderTexture);
			
			shouldRender = false;
		}
	}
}