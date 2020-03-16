
using System.IO;
using UnityEngine;
using UnityEditor;

namespace FolderPreset
{
	public class Window : EditorWindow
	{
		[MenuItem ("Assets/Create/Folder Preset", false, 20)]	
		static void ShowWindow()
		{
			if( activeWindow != null)
			{
				activeWindow.Close();
				activeWindow = null;
			}
			activeWindow = ScriptableObject.CreateInstance<Window>();
			activeWindow.ShowUtility();
		}
		void OnEnable()
		{
			if( current == null)
			{
				if( Selection.activeObject != null)
				{
					current = AssetDatabase.GetAssetPath( Selection.activeObject);
				}
				if( string.IsNullOrEmpty( current) != false)
				{
					Close();
					return;
				}
				if( AssetDatabase.IsValidFolder( current) == false)
				{
					current = Path.GetDirectoryName( current);
				}
				
				if( AssetDatabase.IsValidFolder( current) == false)
				{
					Close();
					return;
				}
			}
		}
		void OnDestroy()
		{
			if( activeWindow != null)
			{
				activeWindow = null;
			}
		}
		void OnGUI()
		{
			EditorGUI.BeginDisabledGroup( true);
			EditorGUILayout.TextField( current);
			EditorGUI.EndDisabledGroup();
			
			for( int i0 = 0; i0 < directories.Length; ++i0)
			{
				PresetDirectory directory = directories[ i0];
				
				bool newEnabled = EditorGUILayout.Toggle( 
					((i0 == directories.Length - 1)? "└─" : "├─") + 
					directory.name, directory.enabled);
				if( directory.enabled != newEnabled)
				{
					Undo.RecordObject( this, "Change Enabled");
					directory.enabled = newEnabled;
				}
			}
			if( GUILayout.Button( "Create") != false)
			{
				for( int i0 = 0; i0 < directories.Length; ++i0)
				{
					PresetDirectory directory = directories[ i0];
					
					if( directory.enabled != false)
					{
						CreateDirectory( current + "/" + directory.name);
					}
				}
				Close();
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
		internal class PresetDirectory
		{
			public PresetDirectory( string name, bool enabled)
			{
				this.name = name;
				this.enabled = enabled;
			}
			internal string name;
			internal bool enabled;
		}
		static PresetDirectory[] directories = new PresetDirectory[]
		{
			new PresetDirectory( "AnimatorControllers", false),
			new PresetDirectory( "Animations", false),
			new PresetDirectory( "Materials", true),
			new PresetDirectory( "Models", false),
			new PresetDirectory( "Textures", true),
			new PresetDirectory( "Prefabs", true),
			new PresetDirectory( "Scripts", false),
			new PresetDirectory( "Scenes", false),
			new PresetDirectory( "Shaders", false),
		};
		static Window activeWindow = null;
		
		[SerializeField]
		string current;
	}
}


