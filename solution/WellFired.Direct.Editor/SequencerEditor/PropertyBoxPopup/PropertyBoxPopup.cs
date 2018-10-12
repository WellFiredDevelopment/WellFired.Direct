using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WellFired.Shared;

namespace WellFired
{
	public class PropertyBoxPopup : EditorWindow
	{
		private static PropertyBoxPopup popup;
		private static Vector2 windowSize = new Vector2(305.0f, 400.0f);
		private static PropertyBoxPopup addPropertyBoxPopup;
		private static List<PropertyBox> propertyBoxes;

		private Action<IEnumerable<PropertyBox>> CommitModifications;
		private Hierarchy popupHierarchy;

		public static bool IsOpen 
		{
			get;
			private set;
		}

		private static float stored;
		public static float OpenCooldown 
		{
			get { return stored; }
			set
			{
				stored = Mathf.Clamp(value, 0.0f, 1.0f);
			}
		}

		public void OnDestroy()
		{
			CommitModifications(propertyBoxes.Where(propertyBox => propertyBox.HasPropertyStateBeenModified));
			IsOpen = false;
		}

		public static void ShowAtPosition(Rect buttonRect, List<PropertyBox> propertiesToShow, Action<IEnumerable<PropertyBox>> CommitModificationsDelegate)
		{
			Event.current.Use();
			if(addPropertyBoxPopup == null)
				addPropertyBoxPopup = CreateInstance<PropertyBoxPopup>();
 		
			addPropertyBoxPopup.Init(buttonRect, propertiesToShow, CommitModificationsDelegate);
		}

		private void Init(Rect buttonRect, List<PropertyBox> propertiesToShow, Action<IEnumerable<PropertyBox>> CommitModificationsDelegate)
		{
			CommitModifications = CommitModificationsDelegate;
			propertyBoxes = propertiesToShow;
			propertyBoxes.ForEach(propertyBox => propertyBox.FavouriteCallback += (obj) => SortHierarchy());
			SortHierarchy();

			buttonRect = GUIToScreenRect(buttonRect);
		
			var popupLocationHelperType = Type.GetType("UnityEditor.PopupLocationHelper+PopupLocation, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			var enumValue = Enum.Parse(popupLocationHelperType, "Right");

			// create array of correct type
			var array = Array.CreateInstance(popupLocationHelperType, 1);
			array.SetValue(enumValue, 0);

		    var method = PlatformSpecificFactory.ReflectionHelper.GetNonPublicMethod(typeof(EditorWindow), "ShowAsDropDown", new [] { typeof(Rect), typeof(Vector2), array.GetType() });

		    try
			{
				method.Invoke(this, new object[] { buttonRect, windowSize, array });
			}
			catch(InvalidOperationException)
			{

			}

			IsOpen = true;
		}

		private void SortHierarchy()
		{
			popupHierarchy = new Hierarchy(windowSize.x - 26.0f, 17.0f, this);

			var favourites = propertyBoxes.Where(propertyBox => propertyBox.Favourite);
			var suggestions = propertyBoxes.Where(propertyBox => PropertyFieldInfoUtility.IsSuggestedProperty(propertyBox.Component, propertyBox.PropertyName));
			var displayableProperties = propertyBoxes.ToList();

			var favouritesItem = new LabelHierarchyItem("Favourites");
			popupHierarchy.AddChild(favouritesItem);
			
			if(!favourites.Any())
			{
				var emptyLabelItem = new LabelHierarchyItem("Click the heart next to any property.");
				favouritesItem.Children = new List<IHierarchyItem>() { emptyLabelItem, };
			}
			else
			{
				favouritesItem.Children = new List<IHierarchyItem>();
				foreach(var favourite in favourites)
				{
					favouritesItem.Children.Add(new FavouriteHierarchyItem(favourite));
				}
			}

			if(suggestions.Any())
			{
				var suggestionsLabel = new LabelHierarchyItem("Suggestions");
				popupHierarchy.AddChild(suggestionsLabel);
				suggestionsLabel.Children = new List<IHierarchyItem>();

				foreach(var suggested in suggestions)
				{
					if(favourites.Contains(suggested))
						continue;

					suggestionsLabel.Children.Add(new SuggestionHierarchyItem(suggested));
				}
			}

			var allPropertiesItem = new LabelHierarchyItem("All Properties");
			allPropertiesItem.Children = new List<IHierarchyItem>();
			popupHierarchy.AddChild(allPropertiesItem);

			var components = displayableProperties.Select(displayableProperty => displayableProperty.Component);
			components = components.Distinct().ToList();
			foreach(var component in components)
			{
				var properties = displayableProperties.Where(displayableProperty => displayableProperty.Component == component).ToList();
				allPropertiesItem.Children.Add(new ComponentHierarchyItem(component, properties));
			}
		}

		private Rect GUIToScreenRect(Rect guiRect)
		{
			var vector = GUIUtility.GUIToScreenPoint(new Vector2(guiRect.x, guiRect.y));
			guiRect.x = vector.x;
			guiRect.y = vector.y;
			return guiRect;
		}

		public void OnGUI()
		{
			OpenCooldown = 1.0f;
			GUI.Box(new Rect(0f, 0f, windowSize.x, windowSize.y), GUIContent.none, new GUIStyle("grey_border"));
			popupHierarchy.OnGUI();
		}
	}
}