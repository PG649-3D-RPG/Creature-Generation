using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MarchingCubesProject;
using UnityEngine;

public class LSystemEditor : MonoBehaviour
{
    [Header("Default Settings")]
    public int m_Distance = 10;
    public short m_Angle = 90;

    public float m_Thickness = 2f;

    public int m_CrossSections = 4;
    public int m_CrossSectionDivisions = 2;

    [Header("L-System")]
    public string m_StartString = "FF[+F][-F]F(42)";
    public uint m_Iterations = 0;
    public string[] m_Rules = { "F=F+F-F-F+F" };

    //TODO change parsing to be context sensitive
    private Dictionary<char, string> ParseRuleInput(string[] rules)
    {
        var nt = new Dictionary<char, string>();
        foreach (var r in rules)
        {
            //check syntax
            //TODO allow only valid symbols
            if (r.Length < 3) throw new ArgumentException("Rule is in a wrong format");
            if (!r.Contains('=')) throw new ArgumentException("Rule needs to include an =");
            // search index of '='
            var end = r.IndexOf('=');
            var non_terminal = r[..end];
            var replacement = r[(end + 1)..];
            if (nt.ContainsKey(non_terminal[0])) throw new ArgumentException("Cannot add another rule with the same non-terminal " + non_terminal[0]);
            else nt.Add(non_terminal[0], replacement);
        }
        return nt;
    }
    // Start is called before the first frame update
    void Start()
    {
        LSystem l = new(m_Distance, m_Angle, m_CrossSections, m_CrossSectionDivisions);
        // var s = l.Parse("F", 1, NT);
        // Debug.Log(s);
        // var t = l.Tokenize(m_StartString);
        // var v = l.Turtle3D(t);
        // LSystem.PrintList(v);
        var rules = ParseRuleInput(m_Rules);
        Debug.Log(string.Join(Environment.NewLine, rules.Select(kvp => kvp.Key + ": " + kvp.Value.ToString())));
        var output = l.Evaluate(m_StartString, m_Iterations, rules, true);

        Segment[] segments = new Segment[output.Count];
        for (int i = 0; i < output.Count; i++)
        {
            segments[i] = new Segment(output[i].Item1, output[i].Item2, m_Thickness);
            Debug.Log(segments[i].startPoint + ", " + segments[i].endPoint);
        }


        // Vector3 startPoint = new Vector3(-1, -3, 0);
        // Vector3 endPoint = new Vector3(14, 0, 1.5f);
        // Segment[] segments = new Segment[] { new Segment(startPoint, endPoint, 2) };
        Metaball m = Metaball.BuildFromSegments(segments);


        //generate mesh from the metaball
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        //attributes such as the size and grid resolution can be set via component or through meshGen.gridResolution
        meshGen.size = 25;

        meshGen.Generate(m);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
