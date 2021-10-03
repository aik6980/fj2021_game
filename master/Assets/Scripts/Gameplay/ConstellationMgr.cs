using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ConstellationMgr : MonoSingleton<ConstellationMgr>
{
    List<Constellation> m_constellation_list;
    bool m_enable_constellation_canvas;

    Constellation m_curr_constellation;
    LineRenderer m_tmp_line;
    public Material line_material;

    public GameObject constellation_prefab;

    public GameObject activate_button;
    public GameObject canvas_panel;
    public TMPro.TMP_InputField comp_name_inputfield;


    Dictionary<Starpicking, List<ParticleSystem.Particle>> star_pickers = new Dictionary<Starpicking, List<ParticleSystem.Particle>>();

    bool process_undo = false;

    // Start is called before the first frame update
    void Start()
    {
        m_constellation_list = new List<Constellation>();
        m_enable_constellation_canvas = false;

        canvas_panel.SetActive(false);
        activate_button.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1) || process_undo)
        {
            process_undo = false;

            if (m_curr_constellation != null)
            {
                if (m_curr_constellation.lines.Count > 0)
                {
                    var line = m_curr_constellation.lines.Last<LineRenderer>();
                    if (line)
                    {
                        Destroy(line.gameObject);
                        m_curr_constellation.lines.RemoveAt(m_curr_constellation.lines.Count - 1);
                    }
                }
            }
        }


        if (Input.GetMouseButtonDown(0))
        {
            var kvp = get_first_inside_list();
            var star_picking = kvp.Key;
            var inside = kvp.Value;

            if(inside != null)
            {
                if (inside.Count > 0)
                {
                    var go = new GameObject();
                    go.transform.SetParent(star_picking.transform, false);
                    go.transform.localPosition = inside[0].position;
                    m_tmp_line = go.AddComponent<LineRenderer>();
                    m_tmp_line.material = line_material;
                    m_tmp_line.startWidth = m_tmp_line.endWidth = 2.0f;
                    m_tmp_line.useWorldSpace = false;
                    m_tmp_line.positionCount = 2;
                    m_tmp_line.SetPosition(0, Vector3.zero);
                }
            }

        }

        if (Input.GetMouseButtonUp(0))
        {
            var kvp = get_first_inside_list();
            var star_picking = kvp.Key;
            var inside = kvp.Value;

            if (inside != null)
            {
                if (m_tmp_line && inside.Count > 0)
                {
                    m_tmp_line.SetPosition(1, m_tmp_line.transform.InverseTransformPoint(star_picking.transform.TransformPoint(inside[0].position)));

                    m_curr_constellation.lines.Add(m_tmp_line);
                    m_tmp_line = null;
                }
            }

            if (m_tmp_line)
            {
                Destroy(m_tmp_line.gameObject);
                m_tmp_line = null;
            }

        }

        if (m_tmp_line)
        {
            var d = (m_tmp_line.transform.TransformPoint(m_tmp_line.GetPosition(0)) - Camera.main.transform.position).magnitude;
            var mouse_pos = Input.mousePosition;
            var ray = Camera.main.ScreenPointToRay(mouse_pos);

            var pos = ray.origin + ray.direction * d;

            m_tmp_line.SetPosition(1, m_tmp_line.transform.InverseTransformPoint(pos));
        }
    }

    public KeyValuePair<Starpicking, List<ParticleSystem.Particle>> get_first_inside_list()
    {
        foreach(var kvp in star_pickers)
        {
            if(kvp.Value.Count > 0)
            {
                return kvp;
            }
        }

        return new KeyValuePair<Starpicking, List<ParticleSystem.Particle>>();
    }

    public void register_star_picker(Starpicking picker, List<ParticleSystem.Particle> li)
    {
        star_pickers[picker] = li;
    }

    public void enable_canvas_mode(bool val)
    {
        m_enable_constellation_canvas = val;

        if(m_enable_constellation_canvas == true)
        {
            m_curr_constellation = new Constellation();

            canvas_panel.SetActive(true);
            activate_button.SetActive(false);
        }
        else // turning canvas off
        {
            // add the curr one into the global list
            if(m_curr_constellation.is_valid())
            {
                var name = comp_name_inputfield.text;
                m_curr_constellation.name = name;
                m_constellation_list.Add(m_curr_constellation);

                // create a Constellation GO with name tag
                var go = Instantiate(constellation_prefab);
                var bound = m_curr_constellation.create_bounds();
                go.GetComponent<SphereCollider>().radius = bound.extents.magnitude;

                go.transform.SetParent(transform, false);
                go.transform.position = bound.center;
                go.GetComponentInChildren<TMPro.TMP_Text>().text = name;
                
            }

            // destroy the temp one
            m_curr_constellation = null;
            m_tmp_line = null;

            canvas_panel.SetActive(false);
            activate_button.SetActive(true);
        }
    }

    const string glyphs = " abcdefghijklmnop qrstuvwxyz0123456789_ "; //add the characters you want
    const int minCharAmount = 20;
    const int maxCharAmount = 30;
    public string random_name()
    {
        string myString = "";
        int charAmount = Random.Range(minCharAmount, maxCharAmount); //set those to the minimum and maximum length of your string
        for(int i=0; i<charAmount; i++)
        {
            myString += glyphs[Random.Range(0, glyphs.Length)];
        }

        return myString;
    }

    public bool is_canvas_mode_enabled()
    {
        return m_enable_constellation_canvas;
    }


    public void disable_canvas()
    {
        enable_canvas_mode(false);
    }


    public void undo_action()
    {
        process_undo = true;
    }
}
