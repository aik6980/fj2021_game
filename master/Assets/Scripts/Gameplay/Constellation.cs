using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ConstellationData
{
    public List<Vector3S> go_position;
    public List<Vector3S> beg_pos;
    public List<Vector3S> end_pos;
    public string name;
}

public class Constellation
{
    public List<LineRenderer> lines;
    public string name;

    public Constellation()
    {
        lines = new List<LineRenderer>();
        name = "";
    }

    public bool is_valid()
    {
        // name should not be empty as well
        return (lines.Count > 0);
    }

    public Bounds create_bounds()
    {
        Bounds bounds = lines[0].bounds;

        foreach (LineRenderer line in lines)
        {
            bounds.Encapsulate(line.bounds);
        }

        return bounds;
    }


    public static ConstellationData convert(Constellation src)
    {
        var dest = new ConstellationData();
        dest.name = src.name;
        dest.go_position = new List<Vector3S>();
        dest.beg_pos = new List<Vector3S>();
        dest.end_pos = new List<Vector3S>();

        foreach(var line in src.lines)
        {
            dest.go_position.Add(line.gameObject.transform.position);
            dest.beg_pos.Add(line.GetPosition(0));
            dest.end_pos.Add(line.GetPosition(1));
        }

        return dest;
    }

    public static Constellation convert(ConstellationData src)
    {
        var dest = new Constellation();
        dest.name = src.name;
        dest.lines = new List<LineRenderer>();

        for(var i=0; i< src.beg_pos.Count; ++i)
        {
            var go = new GameObject();
            go.transform.SetParent(ConstellationMgr.Instance.transform.parent, false);
            go.transform.position = src.go_position[i];

            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.material = ConstellationMgr.Instance.line_material;
            line.startWidth = line.endWidth = ConstellationMgr.Instance.line_width;
            line.SetPosition(0, src.beg_pos[i]);
            line.SetPosition(1, src.end_pos[i]);

            dest.lines.Add(line);
        }

        return dest;
    }
}
