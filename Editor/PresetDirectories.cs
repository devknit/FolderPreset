
using System.IO;
using UnityEngine;
using UnityEditor;

namespace FolderPreset
{
	[System.Serializable]
	class PresetDirectories : ScriptableObject
	{
		public PresetDirectories()
		{
			directories = new Element[]
			{
				new Element( "AnimatorControllers", false),
				new Element( "Animations", false),
				new Element( "Materials", true),
				new Element( "Models", false),
				new Element( "Textures", true),
				new Element( "Textures/Cube", true),
				new Element( "Prefabs", true),
				new Element( "Scripts", false),
				new Element( "Scenes", false),
				new Element( "Shaders", false),
			};
		}
		public bool IsValid()
		{
			return (directories?.Length ?? 0) > 0;
		}
		public void Verify()
		{
			
		}
		public float GetHeight()
		{
			return EditorGUIUtility.singleLineHeight * directories.Length;
		}
		public void OnGUI()
		{
			for( int i0 = 0; i0 < directories.Length; ++i0)
			{
				Element directory = directories[ i0];
				
				bool newEnabled = EditorGUILayout.Toggle( 
					((i0 == directories.Length - 1)? "└─" : "├─") + 
					directory.name, directory.enabled);
				if( directory.enabled != newEnabled)
				{
					Undo.RecordObject( this, "Change Enabled");
					directory.enabled = newEnabled;
				}
			}
		}
		public void CreateDirectories( string current)
		{
			for( int i0 = 0; i0 < directories.Length; ++i0)
			{
				Element directory = directories[ i0];
				
				if( directory.enabled != false)
				{
					CreateDirectory( current + "/" + directory.name);
				}
			}
		}
		static void CreateDirectory( string path)
		{
			if( Directory.Exists( path) == false)
			{
				Directory.CreateDirectory( path);
				AssetDatabase.ImportAsset( path);
			}
		}
		
		[System.Serializable]
		class Element
		{
			public Element( string name, bool enabled)
			{
				this.name = name;
				this.enabled = enabled;
			}
			[SerializeField]
			internal string name;
			[SerializeField]
			internal bool enabled;
		
		}
		class ElementComparer: IComparer
		{
			public int Compare( object x, object y)
			{
				
			}
		}
		
		[SerializeField]
		Element[] directories;
	}
}
