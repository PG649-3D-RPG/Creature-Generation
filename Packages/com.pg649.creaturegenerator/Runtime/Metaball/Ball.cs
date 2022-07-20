using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball
{

    public float R;
    public Vector3 position;
    public MetaballFunction function;
    public bool inverted;

    public Ball(float r, Vector3 pos, MetaballFunction function, bool inverted = false)
    {
        R = r;
        position = pos;
        this.function = function;
        this.inverted = inverted;
    }

    public virtual float Value(float x, float y, float z)
    {
        float v = 0f;
        switch(function) {
            case MetaballFunction.Polynomial2:
                v = Polynomial(x, y, z, 2);
                break;
            case MetaballFunction.Polynomial3:
                v = Polynomial(x, y, z, 3);
                break;
            case MetaballFunction.Exponential:
                v = Exponential(x, y, z);
                break;
            case MetaballFunction.ExponentialThin:
                v = Exponential(x, y, z, 1.2f);
                break;
            case MetaballFunction.Boolean:
                v = Boolean(x, y, z);
                break;
            default:
                throw new Exception("Unrecognized metaball function!");
        }
        if (inverted)
            return -v;
        else
            return v;
    }

    internal float Polynomial(float x, float y, float z, int p = 2) {
        float r = Mathf.Sqrt(Mathf.Pow(x - position.x, 2) + Mathf.Pow(y - position.y, 2) + Mathf.Pow(z - position.z, 2));
        return Mathf.Pow(R, p)/Mathf.Pow(r, p);
    }
    
    internal float Exponential(float x, float y, float z, float b = 0.5f) {
        return Mathf.Exp(b - (b * (Mathf.Pow(x - position.x, 2) + Mathf.Pow(y - position.y, 2) + Mathf.Pow(z - position.z, 2))) / (R*R));
    }

    internal float Boolean(float x, float y, float z)
    {
        float r = Mathf.Sqrt(Mathf.Pow(x - position.x, 2) + Mathf.Pow(y - position.y, 2) + Mathf.Pow(z - position.z, 2));
        return r <= R ? 2f: 0f;
    }

    public static int GetMinimumNumBalls(MetaballFunction function, float segmentLength, float segmentThickness) {
        switch(function) {
            case MetaballFunction.Polynomial2:
                return Mathf.CeilToInt(segmentLength / (Mathf.Pow(2, 1 / 2.0f) * segmentThickness * 2));
            case MetaballFunction.Polynomial3:
                return Mathf.CeilToInt(segmentLength / (Mathf.Pow(2, 1 / 3.0f) * segmentThickness * 2));
            case MetaballFunction.Exponential:
                return Mathf.CeilToInt(segmentLength / (2.97f * segmentThickness  ));
            default:
                throw new Exception("Unrecognized metaball function!");
        }
    }

}
