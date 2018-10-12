using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using WellFired.Data;
using Object = UnityEngine.Object;

namespace WellFired.UI
{
	public class WindowStack 
	{
		private Stack<IWindow> windowStack = new Stack<IWindow>();
		private Canvas rootCanvas;
		private EventSystem eventSystem;

		public WindowStack(Canvas canvas, EventSystem eventSystem)
		{
			rootCanvas = canvas;
			this.eventSystem = eventSystem;
		}

		public IWindow OpenWindowWithData(Type windowTypeToOpen, DataBaseEntry data)
		{
			var window = OpenWindow(windowTypeToOpen);
			var dataComponentWindow = window as WindowWithDataComponent;
			dataComponentWindow.InitFromData(data);
			return window;
		}

		public IWindow OpenWindow(Type windowTypeToOpen)
		{
			var windowType = windowTypeToOpen.Name.ToString();
			var windowPath = string.Format("UI/Window/{0}/{1}", windowType, windowType);

			// Instantiate our object and get the components we need
			var loadedObject = Resources.Load(windowPath) as GameObject;
			var instantiatedObject = Object.Instantiate(loadedObject) as GameObject;
			var rectTransform = instantiatedObject.GetComponent<RectTransform>();
			var window = instantiatedObject.GetComponent<Window>();

			instantiatedObject.transform.SetParent(rootCanvas.transform, false);

			// Alter the Z Position if needed.
			if(windowStack.Count > 0 &&  windowStack.Peek() != default(IWindow))
			{
				var peekRectTransform = (windowStack.Peek() as MonoBehaviour).GetComponent<RectTransform>();
				var peekedLocalZ = peekRectTransform.localPosition.z;

				var currentLocalPosition = rectTransform.localPosition;
				currentLocalPosition.z = peekedLocalZ - 5.0f;
				rectTransform.localPosition = currentLocalPosition;
			}

			PushWindow(window);

			window.WindowStack = this;

			if(eventSystem != default(EventSystem))
			{
				if(window.FirstSelectedGameObject != null)
				{
					eventSystem.SetSelectedGameObject(window.FirstSelectedGameObject);
				}
			}

			return windowStack.Peek();
		}

		public void CloseWindow(IWindow window)
		{
			if(windowStack.Peek() != window)
			{
				throw new Exception (string.Format ("Trying to pop a window {0} that is not at the head of the stack", window.GetType ()));
			}

			PopWindow();
		}

		private void PushWindow(IWindow window)
		{
			windowStack.Push(window);
		}

		private void PopWindow()
		{
			windowStack.Pop();
		}
	}
}