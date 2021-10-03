using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
