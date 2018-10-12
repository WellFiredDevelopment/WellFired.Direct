using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace WellFired
{
	/// <summary>
	/// This class represents a USTimelineEvent, this is the core element to our event system.
	/// Events are stored as child objects.
	/// </summary>
	public class USTimelineEvent : USTimelineBase 
	{
		private float elapsedTime;
		
		#region Properties
		/// <summary>
		/// How many events does this timeline have.
		/// </summary>
		public int EventCount
		{
			get
			{
				return transform.childCount;
			}
		}
		
		/// <summary>
		/// Gets the events.
		/// </summary>
		private USEventBase[] cachedEvents;
		public USEventBase[] Events
		{
			get
			{
				if(cachedEvents != null)
					return cachedEvents;

				cachedEvents = GetComponentsInChildren<USEventBase>();
				return cachedEvents;
			}
		}
		#endregion
		
		/// <summary>
		/// Gets the event at Index.
		/// </summary>
		public USEventBase Event(int index)
		{
			if(index >= EventCount)
			{
				Debug.LogError("Trying to access an element in mEventList that does not exist from USTimelineEvent::Event");
				return null;
			}
	
			var eventBase = transform.GetChild(index).GetComponent<USEventBase>();
			if(!eventBase)
			{
				Debug.LogWarning("We've requested an event that doesn't have a USEventBase component : " + eventBase.name + " from timeline : " + name);
			}
			
			return eventBase;
		}
		
		public override void StopTimeline()
		{
			var prevElapsedTime = elapsedTime;
			elapsedTime = 0.0f;
			
			var events = Events.Where(e => e.FireTime <= prevElapsedTime).ToArray();
			foreach(var eventBase in events.Reverse())
				eventBase.StopEvent();
		}
		
		public override void PauseTimeline()
		{
			foreach(var eventBase in Events)
				eventBase.PauseEvent();
		}
		
		public override void ResumeTimeline()
		{
			foreach(var eventBase in Events)
			{
				if(!eventBase.IsFireAndForgetEvent && Sequence.RunningTime > eventBase.FireTime && Sequence.RunningTime < (eventBase.FireTime + eventBase.Duration))
					eventBase.ResumeEvent();
			}
		}
		
		/// <summary>
		/// This function will skip the timeline time to the time passed, any event that is flagged to fire when skipped 
		/// (None by default) will not fired.
		/// </summary>
		public override void SkipTimelineTo(float time)
		{
			var prevElapsedTime = elapsedTime;
			elapsedTime = time;
		
			foreach(Transform child in transform)
			{
				var baseEvent = child.GetComponent<USEventBase>();
				if (!baseEvent)
					continue;
				
				var shouldSkipEvent = !baseEvent.IsFireAndForgetEvent || !baseEvent.FireOnSkip;
				if(shouldSkipEvent)
					continue;
	
				if ((prevElapsedTime < baseEvent.FireTime || prevElapsedTime <= 0.0f) && time > baseEvent.FireTime)
				{
					if (Sequence.IsPlaying && baseEvent.AffectedObject)
						baseEvent.FireEvent();
				}
			}
		}
		
		public override void Process(float sequencerTime, float playbackRate)
		{	
			var prevElapsedTime = elapsedTime;
			elapsedTime = sequencerTime;
			
			var events = Events;
			
			if(prevElapsedTime < elapsedTime)
				Array.Sort(events, delegate(USEventBase a, USEventBase b) { return a.FireTime.CompareTo(b.FireTime); });
			else
				Array.Sort(events, delegate(USEventBase a, USEventBase b) { return b.FireTime.CompareTo(a.FireTime); });
		
			foreach(var baseEvent in events)
			{	 
				if(playbackRate >= 0.0f)
					FireEvent(baseEvent, prevElapsedTime, elapsedTime);
				else
					FireEventReverse(baseEvent, prevElapsedTime, elapsedTime);
				
				FireEventCommon(baseEvent, sequencerTime, prevElapsedTime, elapsedTime);
			}
		}
		
		private void FireEvent(USEventBase baseEvent, float prevElapsedTime, float elapsedTime)
		{	
			if ((prevElapsedTime < baseEvent.FireTime || prevElapsedTime <= 0.0f) && elapsedTime >= baseEvent.FireTime)
			{
				if (baseEvent.AffectedObject)
					baseEvent.FireEvent();
			}
		}
		
		private void FireEventReverse(USEventBase baseEvent, float prevElapsedTime, float elapsedTime)
		{

		}
		
		private void FireEventCommon(USEventBase baseEvent, float sequencerTime, float prevElapsedTime, float elapsedTime)
		{
			if (elapsedTime > baseEvent.FireTime && elapsedTime <= baseEvent.FireTime + baseEvent.Duration)
			{
				var deltaTime = sequencerTime - baseEvent.FireTime;
				if (baseEvent.AffectedObject)
					baseEvent.ProcessEvent(deltaTime);
			}
			
			if(prevElapsedTime < baseEvent.FireTime + baseEvent.Duration && elapsedTime >= baseEvent.FireTime + baseEvent.Duration)
			{
				if (baseEvent.AffectedObject)
				{
					var deltaTime = sequencerTime - baseEvent.FireTime;
					baseEvent.ProcessEvent(deltaTime);
					baseEvent.EndEvent();
				}
			}
				
			if(prevElapsedTime >= baseEvent.FireTime && elapsedTime < baseEvent.FireTime)
			{
				if(baseEvent.AffectedObject)
					baseEvent.UndoEvent();
			}	
		}
		
		public override void ManuallySetTime(float sequencerTime)
		{
			foreach(Transform child in transform)
			{
				var baseEvent = child.GetComponent<USEventBase>();
				if (!baseEvent)
					continue;
				
				var deltaTime = sequencerTime - baseEvent.FireTime;
				if (baseEvent.AffectedObject)
					baseEvent.ManuallySetTime(deltaTime);
			}
		}
		
		public void AddNewEvent(USEventBase sequencerEvent)
		{	
			sequencerEvent.transform.parent = transform;
	
	        SortEvents();
		}
		
		public void RemoveAndDestroyEvent(Transform sequencerEvent)
		{
			if (!sequencerEvent.IsChildOf(transform))
			{
				Debug.LogError("We are trying to delete an Event that doesn't belong to this Timeline, from USTimelineEvent::RemoveAndDestroyEvent");
				return;
			}
	
			DestroyImmediate(sequencerEvent.gameObject);
		}
	
	    public void SortEvents()
	    {
			Debug.LogWarning("Implement a sorting algorithm here!");
	    }

		public override void ResetCachedData()
		{
			base.ResetCachedData();
			cachedEvents = null;
		}
	}
}