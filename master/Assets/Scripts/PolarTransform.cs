using UnityEngine;


[System.Serializable]
public class PolarCoordinate
{
    [SerializeField]
    private Transform pivot;
    [SerializeField]
    private Quaternion rotation;
    [SerializeField]
    private Vector3 offset;

    public Transform Pivot
    {
        get => pivot;
        set => pivot = value;
    }

    public Quaternion Rotation
    {
        get => rotation;
        set => rotation = value;
    }

    public Vector3 Offset
    {
        get => offset;
        set => offset = value;
    }

    private Quaternion PivotRotation
    {
        get => this.pivot == null ? Quaternion.identity : this.pivot.rotation;
    }

    public Vector3 CarteseanPosition
    {
        get
        {
            Vector3 pivot_position = this.pivot == null ? Vector3.zero : this.pivot.position;
            return (PivotRotation * (this.rotation * this.offset)) + pivot_position;
        }
    }

    public Vector3 Up
    {
        get => PivotRotation * Rotation * Vector3.up;
    }

    public Vector3 Forward
    {
        get => PivotRotation * Rotation * Vector3.forward;
    }

    public Vector3 Right
    {
        get => PivotRotation * Rotation * Vector3.right;
    }

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

    public Vector3 InverseTransformPoint(Vector3 cartesean_position)
    {
        return Quaternion.Inverse(rotation) *
            Quaternion.Inverse(pivot.rotation) *
            (cartesean_position - pivot.position);
    }

    public static PolarCoordinate operator* (PolarCoordinate coord, Quaternion rotation)
    {
        var new_coord = new PolarCoordinate(coord);
        new_coord.rotation *= rotation;
        return new_coord;
    }

    /// <summary>
    /// Creates a PolarCoordinator that represents the position of the object around the pivot, while respecting
    /// any local rotation of the object.
    /// </summary>
    /// <remarks>
    /// Note that this is useful for manipulating the position of objects in polar space because it maintains any
    /// local rotation for the object.
    /// </remarks>
    /// <param name="obj">The object whose PolarCoordinate is sought.</param>
    /// <param name="pivot">The pivot point around which the PolarCoordinate will be constructed.</param>
    /// <returns>
    /// A new PolarCoordinate representing the position of obj in polar space with respect to pivot.
    /// </returns>
    public static PolarCoordinate FromTransform(Transform obj, Transform pivot)
    {
        // In order to work out the offset we need to reverse the Cartesean calculation, which is:
        //      cartesian_pos = (pivot.rotation * (obj.rotation * offset)) + pivot.position
        return new PolarCoordinate(
            pivot,
            obj.rotation,
            Quaternion.Inverse(obj.rotation) * (Quaternion.Inverse(pivot.rotation) * (obj.position - pivot.position)));
    }

    /// <summary>
    /// Creates a PolarCoordinate that represents the rotation from the pivot point ignoring any rotation
    /// and offset that might have been applied to the object.
    /// </summary>
    /// <remarks>
    /// Note that because this ignores any rotation of the object it is unsuitable for manipulating the object.
    /// However, it is appropriate for comparison of objects in polar space, since this represents their position
    /// relative to each other much better than with offsets.
    /// </remarks>
    /// <param name="obj">The object whose PolarCoordinate is sought.</param>
    /// <param name="pivot">The pivot point around which the PolarCoordinate will be constructed.</param>
    /// <returns>
    /// A new PolarCoordinate representing the position of obj in polar space with respect to pivot.
    /// </returns>
    public static PolarCoordinate FromLookAt(Transform obj, Transform pivot)
    {
        Vector3 position_delta = (obj.position - pivot.position);
        return new PolarCoordinate(
            pivot,
            Quaternion.LookRotation(position_delta, Vector3.up),
            Vector3.forward * position_delta.magnitude);
    }
}

[ExecuteInEditMode]
public class PolarTransform : MonoBehaviour
{
    public PolarCoordinate PolarCoord;
 
    private void Awake()
    {
        var pivot = transform.parent;
        PolarCoord = PolarCoordinate.FromTransform(transform, pivot);
    }

    public void Update()
    {
        transform.position = PolarCoord.CarteseanPosition;
        transform.rotation = PolarCoord.Rotation;
    }

    public void UpdateFromTransform(Transform new_transform)
    {
        transform.position = new_transform.position;
        transform.rotation = new_transform.rotation;

        PolarCoord = PolarCoordinate.FromTransform(transform, PolarCoord.Pivot);
    }
}
