\mainpage uSequencer Main Page

\tableofcontents

\section getting_started Getting Started

Have a look at the following pages for a brief overview of uSequencer, its classes and members.
\n[Hierarchy](hierarchy.html)
\n[Classes](annotated.html)


\section extending Extending uSequencer
There are a number of ways to extend uSequencer.

\subsection custom_events Custom Events

Adding custom events to uSequencer is incredibly easy, there are however some requirements
- You must define the [USequencerEvent](class_well_fired_1_1_u_sequencer_event_attribute.html) attribute, this maps the event into the context menu for uSequencer. It works the same as Unity's Menu Item attribute.
- You must define the [USequencerFriendlyName](class_well_fired_1_1_u_sequencer_friendly_name_attribute.html) attribute. This is displayed in the uSequencer window.
- You must inherit from [USEventBase](class_well_fired_1_1_u_s_event_base.html) and implement the [FireEvent](class_well_fired_1_1_u_s_event_base.html#ac8e1de92397cc68c338560a9ef04be46) and [ProcessEvent](class_well_fired_1_1_u_s_event_base.html#af4604fe59403b7b314c1c2f4e32d22d4) methods.

An example event is the USMessageEvent provided with uSequencer.

    using UnityEngine;
    using System.Collections;
    
    [USequencerFriendlyName("Debug Message")]
    [USequencerEvent("Debug/Log Message")]
    public class USMessageEvent : USEventBase 
    {
        public string message = "Default Message";
        
        public override void FireEvent()
        {
            Debug.Log(message);
        }
        
        public override void ProcessEvent(float deltaTime)
        {
          
        }
    }
    



\subsection custom_event_visualisation Custom Event Visualisation

Customising the way events are rendered in usequencer is just as easy, there are however some requirements
- You must define the [CustomUSEditor](class_well_fired_1_1_custom_u_s_editor_attribute.html) attribute, this maps the Event Editor to the Event.
- You must inherit from [USEventBaseEditor](class_well_fired_1_1_u_s_event_base_editor.html) and implement the [RenderEvent](class_well_fired_1_1_u_s_event_base_editor.html#ae69450a551814bde7ca08067b3cbd768) method. This method must return the grabbable area.

An example event visualisation is the USMessageEventEditor provided with uSequencer

	using UnityEditor;
	using UnityEngine;
	using System.Collections;
	
	[CustomUSEditor(typeof(USMessageEvent))]
	public class USMessageEventEditor : USEventBaseEditor
	{
		public override Rect RenderEvent(Rect myArea)
		{
			USMessageEvent messageEvent = TargetEvent as USMessageEvent;
	
			myArea = DrawDurationDefinedBox(myArea);
	
			using(new GUIBeginArea(myArea))
			{
				GUILayout.Label(GetReadableEventName(), DefaultLabel);
				if (messageEvent)
					GUILayout.Label(messageEvent.message, DefaultLabel);
			}
	
			return myArea;
		}
	}
	



\subsection custom_timelines Custom Timelines

Unsurprisingly, it's also easy to add your own, completely custom timelines. It is however slightly more involved than adding new events. First we'll start out with the timeline itself.

- You must inherit from [USTimelineBase](class_well_fired_1_1_u_s_timeline_base.html) and implement the [Process](class_well_fired_1_1_u_s_timeline_base.html#ad3c5c5493b2771809721a2175e59950b) method.

An example timeline is the following USTestTimeline.

	using UnityEngine;
	using System;
	
	[Serializable]
	public class USTestTimeline : USTimelineBase 
	{
		public override void Process(float sequencerTime, float playbackRate)
		{
			Debug.Log(string.Format("Processing Timeline : {0}", sequencerTime));
		}
	}
	

\subsection custom_timeline_visualisation Custom Timeline Visualisation

As well as Adding New Timelines you can also completely customise the way they are visualised by uSequencer, though this is more involved than custom event visualisation.
- You must define the [USCustomTimelineHierarchyItem](class_well_fired_1_1_u_s_custom_timeline_hierarchy_item.html) attribute, this maps timeline type to the renderer as well as mapping the timeline into the context menu for uSequencer. It works the same as Unity's Menu Item attribute.
- You must inherit from [IUSTimelineHierarchyItem](class_well_fired_1_1_i_u_s_timeline_hierarchy_item.html) and implement the [DoGUI](class_well_fired_1_1_i_u_s_hierarchy_item.html#a2d8cb1042be60c97c01767ed6f45ebab) method.
- You can optionally implement [GetISelectableContainers](class_well_fired_1_1_i_u_s_timeline_hierarchy_item.html#a5e3d7a9c4c3658f2878dcf4969968fca) and implement the [ISelectableContainer](interface_well_fired_1_1_i_selectable_container.html) interface, if you need to handle input.

A very simple example Timeline visualisation is below.

	using UnityEngine;
	using UnityEditor;
	using System;

	[Serializable]
	[USCustomTimelineHierarchyItem(typeof(USTestTimeline), "Test Timeline")]
	public class USAnimationTimelineHierarchyItem : IUSTimelineHierarchyItem
	{
		public override void DoGUI(int depth)
		{
			using(new GUIBeginHorizontal())
			{
				GUILayout.Box("", FloatingBackground, GUILayout.MaxWidth(FloatingWidth), GUILayout.Height(17.0f));
				GUILayout.Box("", FloatingBackground, GUILayout.ExpandWidth(true), GUILayout.Height(17.0f));
			}
		}
	}

\section change_log Changelog
Read the full [Changelog](changelog.html).