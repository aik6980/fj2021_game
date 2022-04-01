using UnityEngine;

public class Audio_Horizon : MonoBehaviour
{
    [SerializeField] GameObject listener, source, sphere;
    [SerializeField] LayerMask sphereMask;
    Plane plane;

    void OnDrawGizmos() 
    {
        Vector3 listenerPos = listener.transform.position;
        Vector3 sourcePos = source.transform.position;
        Vector3 spherePos = sphere.transform.position;
        
        //calculate plane and disc for horizon tangent direction
        plane.Set3Points(listenerPos, sourcePos, spherePos);
        Vector3 normal = plane.normal;
        Vector3 centre = spherePos;
        float radius = sphere.transform.localScale.x * sphere.GetComponent<SphereCollider>().radius;

        //calculate tangents
        Vector3 tanA = Vector3.zero;
        Vector3 tanB = Vector3.zero;
        CircleTangent3D(centre, radius, listenerPos, plane, ref tanA, ref tanB);

        //select closest tangent point
        Vector3 tangentPos;
        if(Vector3.Distance(tanA, sourcePos) < Vector3.Distance(tanB, sourcePos))
            tangentPos = tanA;
        else tangentPos = tanB;
        
        //extend vector distance
        Vector3 heading = tangentPos - listenerPos;
                heading = heading.normalized * (Vector3.Distance(listenerPos, sourcePos) - Vector3.Distance(listenerPos, tangentPos));
        Vector3 pSource = heading + tangentPos;

        //check if source and listener can see each other
        if(!Physics.Linecast(listenerPos, sourcePos, layerMask: sphereMask))
            Debug.DrawLine(listenerPos, sourcePos, Color.green, 0.1f);
        else
        {
            //draw circle, tangent, and extended vector
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawSolidDisc(centre, normal, radius);
            Gizmos.DrawSphere(tangentPos, 1f);
            Debug.DrawLine(listenerPos, pSource, Color.green, 0.1f);
        }
    }

    void CircleTangent3D(Vector3 c, float r, Vector3 p, Plane plane, ref Vector3 tanPosA, ref Vector3 tanPosB) 
    {
        Vector3 n = plane.normal;
        p -= c;

        float P = p.magnitude;
        
        float a = r * r                             / P;    
        float q = r * Mathf.Sqrt((P * P) - (r * r)) / P;
        
        if(Mathf.Sign(n.z) == -1f)
            plane.Flip();
        
        Vector3 pN  = p / P;
        Vector3 pNP = Vector3.Cross(n, pN);
        Vector3 va  = pN * a;

        tanPosA = va + pNP * q;
        tanPosB = va - pNP * q;

        tanPosA += c;
        tanPosB += c;
  }
}
