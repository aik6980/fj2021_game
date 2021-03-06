using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;


using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public class ConstellationMgr : MonoSingleton<ConstellationMgr>
{
    List<Constellation> m_constellation_list;
    List<GameObject> m_constellation_name_list;

    bool m_enable_constellation_canvas;

    Constellation m_curr_constellation;
    LineRenderer m_tmp_line;
    public Material line_material;

    public GameObject constellation_prefab;

    public Button activate_button;
    public float fade_time = 0.4f;
    private float _fade_velocity;
    public Button exit_button;
    public Button save_button;
    public GameObject canvas_panel;
    public TMPro.TMP_InputField comp_name_inputfield;

    public float line_width = 2.0f;

    Dictionary<Starpicking, List<ParticleSystem.Particle>> star_pickers = new Dictionary<Starpicking, List<ParticleSystem.Particle>>();

    bool process_undo = false;

    public float pitchThresholdCosine = 0.6f;


    // Start is called before the first frame update
    void Start()
    {
		if (!activate_button) { this.enabled = false; return; }

        m_constellation_list = new List<Constellation>();
        m_constellation_name_list = new List<GameObject>();

        m_enable_constellation_canvas = false;

        canvas_panel.SetActive(false);
        activate_button.enabled = Camera.main.transform.forward.y > pitchThresholdCosine;
        var color = activate_button.image.color;
        color.a = activate_button.enabled ? 1f : 0f;
        activate_button.image.color = color;

        // load the existing canvas
        load_constellation_data();
    }

    public void Exit()
    {
        enable_canvas_mode(false);
    }

    public void Save()
    {
        enable_canvas_mode(false);
    }

    // Update is called once per frame
    void Update()
    {
        /// test save/load
        if(Input.GetKeyDown(KeyCode.Y) && Input.GetKey(KeyCode.LeftControl))
        {
            save_constellation_data();
        }

        if(Input.GetKeyDown(KeyCode.T) && Input.GetKey(KeyCode.LeftControl))
        {
            reset_constellation_data();
            load_constellation_data();
        }

        if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftControl))
        {
            reset_constellation_data();
        }
        ///

        if (!is_canvas_mode_enabled())
        {
            bool activation_button_enabled = Camera.main.transform.forward.y > pitchThresholdCosine;
            float opacity_target = (activate_button.enabled ? 1f : 0f);

            activate_button.enabled = activation_button_enabled;
            var color = activate_button.image.color;
            color.a = Mathf.SmoothDamp(color.a, opacity_target, ref _fade_velocity, fade_time);
            activate_button.image.color = color;
        }
        else
        {
            bool hasName = !string.IsNullOrEmpty(comp_name_inputfield.text);
            save_button.gameObject.SetActive(hasName && m_curr_constellation.lines.Count > 0);
            exit_button.gameObject.SetActive(!hasName || m_curr_constellation.lines.Count == 0);
        }

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

            if (inside != null)
            {
                if (inside.Count > 0)
                {
                    var go = new GameObject();
                    go.transform.SetParent(star_picking.transform, false);
                    go.transform.localPosition = inside[0].position;
                    m_tmp_line = go.AddComponent<LineRenderer>();
                    m_tmp_line.material = line_material;
                    m_tmp_line.startWidth = m_tmp_line.endWidth = line_width;
                    m_tmp_line.useWorldSpace = false;
                    m_tmp_line.positionCount = 2;
                    m_tmp_line.SetPosition(0, Vector3.zero);
                    Audio_SFX.StarDrawingStart();
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
                    Audio_SFX.StarDrawingEnd(true);
                }
            }

            if (m_tmp_line)
            {
                Destroy(m_tmp_line.gameObject);
                m_tmp_line = null;
                Audio_SFX.StarDrawingEnd(false);
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
        foreach (var kvp in star_pickers)
        {
            if (kvp.Value.Count > 0)
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

        if (m_enable_constellation_canvas == true)
        {
            m_curr_constellation = new Constellation();

            canvas_panel.SetActive(true);
            activate_button.gameObject.SetActive(false);

            comp_name_inputfield.text = "";
        }
        else // turning canvas off
        {
            // add the curr one into the global list
            bool hasName = !string.IsNullOrEmpty(comp_name_inputfield.text);
            if (m_curr_constellation.is_valid() && hasName)
            {
                var name = comp_name_inputfield.text;
                m_curr_constellation.name = name;

                Audio_SFX.StarDrawingConstellation();

                // clone then add
                var new_constellation = new Constellation();
                new_constellation.name = name;
                foreach (var line in m_curr_constellation.lines)
                {
                    // var go_clone = Instantiate(line.gameObject, line.gameObject.transform.parent);
                    var go_clone = Instantiate(line.gameObject, transform.parent);
                    new_constellation.lines.Add(go_clone.GetComponent<LineRenderer>());
                }

                // create a Constellation GO with name tag
                var go = Instantiate(constellation_prefab);
                var bound = m_curr_constellation.create_bounds();
                go.GetComponent<SphereCollider>().radius = bound.extents.magnitude;

                go.transform.SetParent(transform, false);
                go.transform.position = bound.center;
                go.GetComponentInChildren<TMPro.TMP_Text>().text = new_constellation.name;

                m_constellation_list.Add(new_constellation);
                m_constellation_name_list.Add(go);

            }

            // destroy the temp one
            foreach (var line in m_curr_constellation.lines)
            {
                Destroy(line.gameObject);
            }
            m_curr_constellation = null;
            m_tmp_line = null;

            canvas_panel.SetActive(false);
            activate_button.gameObject.SetActive(true);

            // save canvas on every new drawing
            save_constellation_data();
        }
    }

    const string glyphs = " abcdefghijklmnop qrstuvwxyz0123456789_ "; //add the characters you want
    const int minCharAmount = 20;
    const int maxCharAmount = 30;
    public string random_name()
    {
        string myString = "";
        int charAmount = Random.Range(minCharAmount, maxCharAmount); //set those to the minimum and maximum length of your string
        for (int i = 0; i < charAmount; i++)
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

    const string file_name = "/my_constellation.dat";
    public void save_constellation_data()
    {
        FileStream fs = new FileStream(Application.persistentDataPath + file_name, FileMode.Create);

        BinaryFormatter bf = new BinaryFormatter();

        //var test_data = new ConstellationData();
        //test_data.name = "hello world";
        //test_data.beg_pos = new List<Vector3S>();
        //test_data.end_pos = new List<Vector3S>();
        //
        //test_data.beg_pos.Add(new Vector3(0.4f, 0.5f, 0.6f));
        //test_data.end_pos.Add(new Vector3(3.4f, 4.5f, 5.6f));

        var data = new List<ConstellationData>();
        foreach(var obj in m_constellation_list)
        {
            var obj_data = Constellation.convert(obj);
            data.Add(obj_data);
        }


        bf.Serialize(fs, data);

        //Debug.Log(Application.persistentDataPath);

        fs.Close();

        Debug.Log("data saved");
    }

    public void load_constellation_data()
    {
        Debug.Log("data loading");
        if (File.Exists(Application.persistentDataPath + file_name))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + file_name, FileMode.Open);

            var data = (List<ConstellationData>)bf.Deserialize(file);
            file.Close();

            foreach(var obj_data in data)
            {
                var obj = Constellation.convert(obj_data);

                // create a Constellation GO with name tag
                var go = Instantiate(constellation_prefab);
                var bound = obj.create_bounds();
                go.GetComponent<SphereCollider>().radius = bound.extents.magnitude;

                go.transform.SetParent(transform, false);
                go.transform.position = bound.center;
                go.GetComponentInChildren<TMPro.TMP_Text>().text = obj.name;

                m_constellation_list.Add(obj);
                m_constellation_name_list.Add(go);
            }

            Debug.Log("data loaded");
        }
    }

    public void reset_constellation_data()
    {
        foreach(var a in m_constellation_list)
        {
            foreach(var b in a.lines)
            {
                Destroy(b.gameObject);
            }
        }
        m_constellation_list.Clear();

        foreach(var a in m_constellation_name_list)
        {
            Destroy(a);
        }
        m_constellation_name_list.Clear();
    }

}
