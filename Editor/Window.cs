
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
			activeWindow.titleContent = new GUIContent( "Folder Preset");
			activeWindow.ShowUtility();
		}
		void OnEnable()
		{
			Undo.undoRedoPerformed += Repaint;
			
			if( presets == null)
			{
				string[] paths = AssetDatabase.FindAssets( kSettingsFileName);
				for( int i0 = 0; i0 < paths.Length; ++i0)
				{
					presets = AssetDatabase.LoadAssetAtPath<PresetDirectories>( 
						AssetDatabase.GUIDToAssetPath( paths[ i0])) as PresetDirectories;
					if( (presets?.IsValid() ?? false) != false)
					{
						break;
					}
				}
			}
			if( presets == null)
			{
				presets = ScriptableObject.CreateInstance<PresetDirectories>();
				string path = AssetDatabase.GetAssetPath( MonoScript.FromScriptableObject( presets));
				path = path.Substring( 0, path.LastIndexOf( "/"));
				AssetDatabase.CreateAsset( presets, path + "/" + kSettingsFileName + ".asset");
				AssetDatabase.SaveAssets();
			}
			presets.Verify();
			
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
		void OnDisable()
		{
			Undo.undoRedoPerformed -= Repaint;
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
			scrollPosition = EditorGUILayout.BeginScrollView( scrollPosition);
			{
				EditorGUI.BeginDisabledGroup( true);
				EditorGUILayout.TextField( current);
				EditorGUI.EndDisabledGroup();
				
				presets.OnGUI();
				
				if( GUILayout.Button( "Create") != false)
				{
					presets.CreateDirectories( current);
					Close();
				}
			}
			EditorGUILayout.EndScrollView();
		}
		const string kSettingsFileName = "FolderPresets";
		
		static Window activeWindow = null;
		
		[SerializeField]
		string current;
		[SerializeField]
		Vector2 scrollPosition;
		[SerializeField]
		PresetDirectories presets;
	}
}


