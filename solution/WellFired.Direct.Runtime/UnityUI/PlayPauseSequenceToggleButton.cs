using UnityEngine;
using UnityEngine.UI;

namespace WellFired
{
	/// <summary>
	/// You can add this component to a Unity UI button to hookup Play Sequence functionality to that button.
	/// </summary>
	[RequireComponent(typeof(Button))]
	public class PlayPauseSequenceToggleButton : MonoBehaviour 
	{
		[SerializeField]
		private string pausedText = "Play";
		
		[SerializeField]
		private string playingText = "Pause";

		[SerializeField]
		private USSequencer sequenceToPlay;

		private Text cachedButtonLabel;
	
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
	
			button.onClick.AddListener(() => ToggleSequence());
			cachedButtonLabel = button.GetComponentInChildren<Text>();
		}

		private void Update()
		{
			if(sequenceToPlay.IsPlaying)
			{
				cachedButtonLabel.text = playingText;
			}
			else
			{
				cachedButtonLabel.text = pausedText;
			}
		}

		private void ToggleSequence()
		{
			if(sequenceToPlay.RunningTime >= sequenceToPlay.Duration)
			{
				sequenceToPlay.RunningTime = 0.0f;
				sequenceToPlay.Play();
			}
			else
			{
				if(sequenceToPlay.IsPlaying)
				{
					sequenceToPlay.Pause();
				}
				else
				{
					sequenceToPlay.Play();
				}
			}
		}
	}
}