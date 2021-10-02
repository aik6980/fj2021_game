using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 

[ExecuteInEditMode]
public class Starpicking : MonoBehaviour {
    ParticleSystem ps;

    // these lists are used to contain the particles which match
    // the trigger conditions each frame.
    List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
    List<ParticleSystem.Particle> exit = new List<ParticleSystem.Particle>();

    List<ParticleSystem.Particle> inside = new List<ParticleSystem.Particle>();


    // Constellation struct
    Constellation constellation;
    LineRenderer tmp_line;
    public Material line_material;

    private void Start()
    {
        constellation = new Constellation();
    }


    void OnEnable() 
    {
        ps = GetComponent<ParticleSystem>();
    }

    void OnParticleTrigger() 
    {
        // get the particles which matched the trigger conditions this frame
        ps.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, inside);
        int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
        int numExit = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Exit, exit);

        //Debug.Log("trigger beg" + numEnter);

        // iterate through the particles which entered the trigger and make them red
        for (int i = 0; i < numEnter; i++) {
            ParticleSystem.Particle p = enter[i];
            
            float h, s, v;
            Color.RGBToHSV(p.startColor,out h, out s, out v);
            v *= 2.0f;
            //p.startColor = Color.HSVToRGB(h, s, v);

            //p.startColor = new Color32(255, 0, 0, 255);
            p.startSize *= 2.0f;

            enter[i] = p;
        }

        // iterate through the particles which exited the trigger and make them green
        for (int i = 0; i < numExit; i++) {
            ParticleSystem.Particle p = exit[i];

            float h, s, v;
            Color.RGBToHSV(p.startColor, out h, out s, out v);
            v /= 2.0f;
            //p.startColor = Color.HSVToRGB(h, s, v);
            //p.startColor = new Color32(255, 255, 255, 255);
            p.startSize /= 2.0f;

            exit[i] = p;
        }

        // re-assign the modified particles back into the particle system
        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Exit, exit);

        //Debug.Log("trigger " + enter.Count);
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if(constellation.lines.Count > 0)
            {
                var line = constellation.lines.Last<LineRenderer>();
                if (line)
                {
                    Destroy(line.gameObject);
                    constellation.lines.RemoveAt(constellation.lines.Count - 1);
                }
            }
        }


        if (Input.GetMouseButtonDown(0))
        {
            if(inside.Count > 0)
            {
                var go = new GameObject();
                go.transform.SetParent(transform, false);
                go.transform.localPosition = inside[0].position;
                tmp_line = go.AddComponent<LineRenderer>();
                tmp_line.material = line_material;
                tmp_line.startWidth = tmp_line.endWidth = 2.0f;
                tmp_line.useWorldSpace = false;
                tmp_line.positionCount = 2;
                tmp_line.SetPosition(0, Vector3.zero);
            }
        }

        if(Input.GetMouseButtonUp(0))
        {
            if (tmp_line && inside.Count > 0)
            {
                tmp_line.SetPosition(1, tmp_line.transform.InverseTransformPoint(transform.TransformPoint(inside[0].position)));

                constellation.lines.Add(tmp_line);
                tmp_line = null;
            }
             
            
            if(tmp_line)
            {
                Destroy(tmp_line.gameObject);
                tmp_line = null;
            }
        }

        if (tmp_line)
        {
            var d = (transform.TransformPoint(tmp_line.GetPosition(0)) - Camera.main.transform.position).magnitude;
            var mouse_pos = Input.mousePosition;
            var ray = Camera.main.ScreenPointToRay(mouse_pos);

            var pos = ray.origin + ray.direction * d;

            tmp_line.SetPosition(1, tmp_line.transform.InverseTransformPoint(pos));
        }
    }
}
