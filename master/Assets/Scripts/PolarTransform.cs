using UnityEditor;
using UnityEngine;


[System.Serializable]
public class PolarCoordinate
{
    public Transform pivot;
    public Quaternion rotation;
    public Vector3 offset;

    public Vector3 pivot_position { get => (pivot == null ? Vector3.zero : pivot.position); }
    public Vector3 cartesean_position { get => (pivot.rotation * (rotation * offset)) + pivot_position; }

    public PolarCoordinate(Transform pivot, Quaternion rotation, Vector3 offset)
    {
        this.pivot = pivot;
        this.rotation = rotation;
        this.offset = offset;
    }

    public PolarCoordinate(PolarCoordinate other)
    {
        this.pivot = other.pivot;
        this.rotation = other.rotation;
        this.offset = other.offset;
    }

    public static PolarCoordinate operator* (PolarCoordinate coord, Quaternion rotation)
    {
        var new_coord = new PolarCoordinate(coord);
        new_coord.rotation *= rotation;
        return new_coord;
    }

    public static PolarCoordinate FromTransform(Transform obj, Transform pivot)
    {
        // In order to work out the offset we need to reverse the Cartesean calculation, which is:
        //      cartesian_pos = (pivot.rotation * (obj.rotation * offset)) + pivot.position
        return new PolarCoordinate(
            pivot,
            obj.rotation,
            Quaternion.Inverse(obj.rotation) * (Quaternion.Inverse(pivot.rotation) * (obj.position - pivot.position)));
    }
}


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
                    var offset_pos = Handles.PositionHandle(rotation_tool.polar_coord.cartesean_position, rotation_tool.transform.rotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Offset from pivot");
                        var offset_delta = rotation_tool.transform.InverseTransformPoint(offset_pos);
                        rotation_tool.polar_coord.offset += offset_delta;
                        rotation_tool.Update();
                    }

                    var previous_colour = Handles.color;
                    var polar_up = rotation_tool.transform.TransformDirection(Vector3.up);
                    var polar_right = rotation_tool.transform.TransformDirection(Vector3.right);
                    var polar_forward = rotation_tool.transform.TransformDirection(Vector3.forward);
                    var offset = rotation_tool.polar_coord.offset;

                    EditorGUI.BeginChangeCheck();
                    Handles.color = Color.red;
                    Vector3 disc_centre = rotation_tool.transform.position - (offset.y * polar_up) - (offset.z * polar_forward);
                    var disc_rotation_x = Handles.Disc(rotation_tool.transform.rotation, disc_centre, rotation_tool.transform.right, Mathf.Sqrt(offset.y * offset.y + offset.z * offset.z), false, 0f);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Rotate around pivot X");
                        rotation_tool.polar_coord.rotation = disc_rotation_x;
                        rotation_tool.Update();
                    }

                    EditorGUI.BeginChangeCheck();
                    Handles.color = Color.green;
                    disc_centre = rotation_tool.transform.position - (offset.x * polar_right) - (offset.z * polar_forward);
                    var disc_rotation_y = Handles.Disc(rotation_tool.transform.rotation, disc_centre, rotation_tool.transform.up, Mathf.Sqrt(offset.x * offset.x + offset.z * offset.z), false, 0f);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Rotate around pivot Y");
                        rotation_tool.polar_coord.rotation = disc_rotation_y;
                        rotation_tool.Update();
                    }

                    EditorGUI.BeginChangeCheck();
                    Handles.color = Color.blue;
                    disc_centre = rotation_tool.transform.position - (offset.x * polar_right) - (offset.y * polar_up);
                    var disc_rotation_z = Handles.Disc(rotation_tool.transform.rotation, disc_centre, rotation_tool.transform.forward, Mathf.Sqrt(offset.x * offset.x + offset.y * offset.y), false, 0f);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Rotate around pivot Z");
                        rotation_tool.polar_coord.rotation = disc_rotation_z;
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

[ExecuteInEditMode]
public class PolarTransform : MonoBehaviour
{
    public PolarCoordinate polar_coord;

    private void Awake()
    {
        var pivot = transform.parent;
        polar_coord = PolarCoordinate.FromTransform(transform, pivot);
    }

    public void Update()
    {
        transform.position = polar_coord.cartesean_position;
        transform.rotation = polar_coord.rotation;
    }

    public void UpdateFromTransform(Transform new_transform)
    {
        transform.position = new_transform.position;
        transform.rotation = new_transform.rotation;

        polar_coord = PolarCoordinate.FromTransform(transform, polar_coord.pivot);
    }
}
