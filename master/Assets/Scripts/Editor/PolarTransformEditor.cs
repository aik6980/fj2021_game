using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PolarTransform))]
//[CanEditMultipleObjects] // Technically this works, but it looks a total mess.
public class PolarTransformEditor : Editor
{
    private Tool last_tool = Tool.None;

    private void OnEnable()
    {
        // Disable the default Unity gizmos, to prevent mucking around
        // with the transform directly.
        last_tool = Tools.current;
        if (last_tool == Tool.Move || last_tool == Tool.Rotate)
        {
            Tools.current = Tool.None;
        }
    }

    private void OnDisable()
    {
        // Re-enable the default Unity gizmos.
        Tools.current = last_tool;
    }

    public void OnSceneGUI()
    {
        if (Tools.current != Tool.None)
        {
            last_tool = Tools.current;
        }

        var rotation_tool = target as PolarTransform;
        switch (last_tool)
        {
            case Tool.Move:
                {
                    EditorGUI.BeginChangeCheck();
                    var offset_pos = Handles.PositionHandle(
                        rotation_tool.PolarCoord.CarteseanPosition,
                        rotation_tool.PolarCoord.Rotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Offset from pivot");
                        rotation_tool.PolarCoord.Offset = rotation_tool.PolarCoord.InverseTransformPoint(offset_pos);
                        rotation_tool.Update();
                    }

                    var previous_colour = Handles.color;
                    Handles.color = Color.red; Handles.DrawLine(rotation_tool.PolarCoord.Pivot.position, rotation_tool.PolarCoord.Pivot.position + (rotation_tool.PolarCoord.Right * rotation_tool.PolarCoord.Offset.magnitude));
                    Handles.color = Color.green; Handles.DrawLine(rotation_tool.PolarCoord.Pivot.position, rotation_tool.PolarCoord.Pivot.position + (rotation_tool.PolarCoord.Up * rotation_tool.PolarCoord.Offset.magnitude));
                    Handles.color = Color.blue; Handles.DrawLine(rotation_tool.PolarCoord.Pivot.position, rotation_tool.PolarCoord.Pivot.position + (rotation_tool.PolarCoord.Forward * rotation_tool.PolarCoord.Offset.magnitude));

                    EditorGUI.BeginChangeCheck();
                    Handles.color = Color.red;
                    var disc_rotation_x = Handles.Disc(
                        rotation_tool.PolarCoord.Pivot.rotation * rotation_tool.PolarCoord.Rotation,
                        rotation_tool.PolarCoord.Pivot.position,
                        rotation_tool.PolarCoord.Right,
                        rotation_tool.PolarCoord.Offset.magnitude,
                        false,
                        0f);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Rotate around pivot X");
                        rotation_tool.PolarCoord.Rotation = Quaternion.Inverse(rotation_tool.PolarCoord.Pivot.rotation) * disc_rotation_x;
                        rotation_tool.Update();
                    }

                    EditorGUI.BeginChangeCheck();
                    Handles.color = Color.green;
                    var disc_rotation_y = Handles.Disc(
                        rotation_tool.PolarCoord.Pivot.rotation * rotation_tool.PolarCoord.Rotation,
                        rotation_tool.PolarCoord.Pivot.position + (rotation_tool.PolarCoord.Up * rotation_tool.PolarCoord.Offset.y),
                        rotation_tool.PolarCoord.Up,
                        Mathf.Sqrt(Mathf.Pow(rotation_tool.PolarCoord.Offset.x, 2) + Mathf.Pow(rotation_tool.PolarCoord.Offset.z, 2)),
                        false,
                        0f);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Rotate around pivot Y");
                        rotation_tool.PolarCoord.Rotation = Quaternion.Inverse(rotation_tool.PolarCoord.Pivot.rotation) * disc_rotation_y;
                        rotation_tool.Update();
                    }

                    EditorGUI.BeginChangeCheck();
                    Handles.color = Color.blue;
                    var disc_rotation_z = Handles.Disc(
                        rotation_tool.PolarCoord.Pivot.rotation * rotation_tool.PolarCoord.Rotation,
                        rotation_tool.PolarCoord.Pivot.position,
                        rotation_tool.PolarCoord.Forward,
                        rotation_tool.PolarCoord.Offset.magnitude,
                        false,
                        0f);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Rotate around pivot Z");
                        rotation_tool.PolarCoord.Rotation = Quaternion.Inverse(rotation_tool.PolarCoord.Pivot.rotation) * disc_rotation_z;
                        rotation_tool.Update();
                    }

                    Handles.color = previous_colour;
                    Tools.current = Tool.None;
                }
                break;

            case Tool.Rotate:
                {
                    EditorGUI.BeginChangeCheck();
                    var local_rotation = Handles.RotationHandle(rotation_tool.transform.rotation, rotation_tool.transform.position);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Rotate");
                        rotation_tool.transform.rotation = local_rotation;
                        rotation_tool.UpdateFromTransform(rotation_tool.transform);
                    }
                    Tools.current = Tool.None;
                }
                break;
        }
    }
}
