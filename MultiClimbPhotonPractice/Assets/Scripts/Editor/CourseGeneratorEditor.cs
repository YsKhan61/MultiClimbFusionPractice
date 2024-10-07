using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CourseGenerator))]
public class CourseGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        if (GUILayout.Button("Generate All"))
            ((CourseGenerator)target).Generate();

        if (GUILayout.Button("Generate Blocks"))
            ((CourseGenerator)target).GenerateBlocks();

        if (GUILayout.Button("Generate Jump Pads"))
            ((CourseGenerator)target).GenerateJumpPads();

        if (GUILayout.Button("Add Block"))
            ((CourseGenerator)target).GenerateBlock(true);
        
        if (GUILayout.Button("Add Jump Pad"))
            ((CourseGenerator)target).GenerateJumpPad(true);
    }
}
