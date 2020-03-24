
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
				new Element( "Animations", false),
				new Element( "AnimatorControllers", false),
				new Element( "Materials", true),
				new Element( "Models", false),
				new Element( "Prefabs", true),
				new Element( "Scripts", false),
				new Element( "Scenes", false),
				new Element( "Shaders", false),
				new Element( "Textures", true),
			};
		}
		public bool IsValid()
		{
			return (directories?.Length ?? 0) > 0;
		}
		public void Verify()
		{
			System.Array.Sort( directories, new ElementComparer());
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
		class ElementComparer: NaturalSortOrder.Comparer
		{
			public override int Compare( object x, object y)
			{
				string src1 = null;
				string src2 = null;
				
				if( x is Element element1)
				{
					src1 = element1.name;
				}
				if( y is Element element2)
				{
					src2 = element2.name;
				}
				return InternalCompare( src1, src2);
			}
		}
		
		[SerializeField]
		Element[] directories;
	}
}
