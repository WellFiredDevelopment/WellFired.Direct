using System;
using UnityEditor;
using UnityEngine;
using WellFired.Shared;

namespace WellFired
{
	[Serializable]
	public class PropertyBox 
	{
		private static readonly string EDITOR_PREFS_FAVOURITE_STRING = "WELLFIRED.PROPERTYBOX.FAVOURITES.{0}.{1}";
		
		public Action<PropertyBox> FavouriteCallback
		{ 
			get; 
			set; 
		}

		public PropertyFieldInfo PropertyFieldInfo
		{
			get;
			set;
		}
		
		public bool HasPropertyStateBeenModified
		{ 
			get; 
			set; 
		}

		public bool ShouldDisplayComponent 
		{
			get;
			set;
		}

		public bool ShouldShowFavouriteButton
		{ 
			get; 
			set; 
		}
		
		public bool ShouldShowAddRemove
		{ 
			get; 
			set; 
		}
		
		public bool AddingProperty
		{ 
			get; 
			set; 
		}

		public Component Component
		{ 
			get;
			set;
		}

		public string PropertyName
		{ 
			get; 
			set; 
		}
		
		public bool Favourite 
		{ 
			get 
			{
				var prefsKey = string.Format(EDITOR_PREFS_FAVOURITE_STRING, Component.name, PropertyName);
				return EditorPrefs.GetBool(prefsKey, false);
			}
			set
			{
				var prefsKey = string.Format(EDITOR_PREFS_FAVOURITE_STRING, Component.name, PropertyName);
				EditorPrefs.SetBool(prefsKey, value);
			}
		}
		
		private GUIContent DisplayName
		{ 
			get 
			{ 
				if(!ShouldDisplayComponent)
					return new GUIContent(string.Format("{0}", PropertyName), componentImage);

				return new GUIContent(string.Format("{0} -> {1}", Component.GetType().Name, PropertyName), componentImage);
			}
		}

		private Texture componentImage;

		private PropertyBox() { }
		
		public PropertyBox(PropertyFieldInfo propertyFieldInfo, bool shouldShowFavouriteButton)
		{
			ShouldShowAddRemove = true;
			AddingProperty = false;
			PropertyFieldInfo = propertyFieldInfo;
			ShouldDisplayComponent = true; 
			ShouldShowFavouriteButton = shouldShowFavouriteButton;
			componentImage = EditorGUIUtility.ObjectContent(propertyFieldInfo.Component, propertyFieldInfo.Component.GetType()).image;
			Component = propertyFieldInfo.Component;
			PropertyName = propertyFieldInfo.Name;
		}

		public PropertyBox(USPropertyInfo propertyInfo, bool shouldShowFavouriteButton)
		{
			ShouldShowAddRemove = true;
			AddingProperty = false;
			ShouldDisplayComponent = true; 
			ShouldShowFavouriteButton = shouldShowFavouriteButton;
			componentImage = EditorGUIUtility.ObjectContent(propertyInfo.Component, propertyInfo.Component.GetType()).image;
			Component = propertyInfo.Component;
			PropertyName = propertyInfo.PropertyName;
		}

		public void OnGUI()
		{
			using(new EditorGUIBeginHorizontal())
			{
				EditorGUILayout.LabelField(DisplayName, GUILayout.MaxWidth(180.0f));

				GUILayout.FlexibleSpace();

				if(ShouldShowFavouriteButton)
				{
					if(GUILayout.Button(new GUIContent(Favourite ? USEditorUtility.FavouriteButtonFill : USEditorUtility.FavouriteButtonEmpty, "Adds or Removes this property from the favourites"), "label", GUILayout.MaxHeight(18.0f)))
					{
						Favourite = !Favourite;
						if(FavouriteCallback != null)
							FavouriteCallback(this);
					}
				}

				if(ShouldShowAddRemove)
				{
					using(new GUIChangeColor(AddingProperty == true ? Color.red : GUI.color))
					{
						if(GUILayout.Button(AddingProperty == true ? "-" : "+", GUILayout.Width(20.0f)))
						{
							HasPropertyStateBeenModified = !HasPropertyStateBeenModified;
							AddingProperty = !AddingProperty;
						}
					}
				}
			}
		}
	}
}