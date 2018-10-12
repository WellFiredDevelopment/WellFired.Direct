using UnityEngine;
using UnityEngine.UI;

namespace WellFired
{
	/// <summary>
	/// You can add this component to a Unity UI button to hookup Play Sequence functionality to that button.
	/// </summary>
	[RequireComponent(typeof(Button))]
	public class ReplaySequenceButton : MonoBehaviour 
	{
		[SerializeField]
		private USSequencer sequenceToPlay;

		[SerializeField]
		private bool manageInteractiveState = true;
	
		private void Start() 
		{
			var button = GetComponent<Button>();
	
			if(!button)
			{
				Debug.LogError("The component Play Sequence button must be added to a Unity UI Button");
				return;
			}
			
			if(!sequenceToPlay)
			{
				Debug.LogError("The Sequence to play field must be hooked up in the Inspector");
				return;
			}
	
			button.onClick.AddListener(() => PlaySequence());
			
			button.interactable = sequenceToPlay.RunningTime > sequenceToPlay.Duration;
			if(manageInteractiveState)
			{
				sequenceToPlay.PlaybackStarted += (sequence) => button.interactable = false;
				sequenceToPlay.PlaybackPaused += (sequence) => button.interactable = false;
				sequenceToPlay.PlaybackFinished += (sequence) => button.interactable = true;
				sequenceToPlay.PlaybackStopped += (sequence) => button.interactable = false;
			}
		}

		private void RunningTimeUpdated(USSequencer sequence)
		{
			var button = GetComponent<Button>();
			var canPlay = USRuntimeUtility.CanPlaySequence(sequence);
			button.interactable = canPlay;
			Debug.Log (canPlay);
		}
	
		private void PlaySequence()
		{
			if(sequenceToPlay.RunningTime >= sequenceToPlay.Duration)
				sequenceToPlay.RunningTime = 0.0f;

			sequenceToPlay.Play();
		}
	}
}