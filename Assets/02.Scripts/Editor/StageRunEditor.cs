using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StageRunner))]
public class StageRunEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        var runner = (StageRunner)target;
        if (EditorApplication.isPlaying)
        {
            if (GUILayout.Button("시작"))
            {
                runner.StartRun();
            }   
        }
    }
}