using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceFunctions : MonoBehaviour
{
    // ****************** Distance Functions ****************** 

    // Sphere
    // s: radius
    public float sdSphere(Vector3 p, float s)
    {
        return p.magnitude - s;
    }

    // Box
    // b: size of box in x/y/z
    public float sdBox(Vector3 p, Vector3 b)
    {
        Vector3 d = Abs(p) - b;
        return Mathf.Min(Mathf.Max(d.x, Mathf.Max(d.y, d.z)), 0.0f) + (Max(d, Vector3.zero).magnitude);
    }
    //triangle prism
    public float sdTriPrism(Vector2 p, Vector2 h)
    {
        p.y = -p.y;
        p.y += h.x;
        float k = Mathf.Sqrt(3.0f);
        h.x *= 0.5f * k;
        p /= h.x;
        p.x = Mathf.Abs(p.x) - 1.0f;
        p.y += 1.0f / k;
        if (p.x + k * p.y > 0.0) p = new Vector2(p.x - k * p.y, -k * p.x - p.y) / 2.0f;
        p.x -=  Mathf.Clamp(p.x, -2.0f, 0.0f);
        float d1 = p.magnitude * Mathf.Sign(-p.y) * h.x;
        float d2 = -h.y;
        return Max2(new Vector2(d1, d2),Vector2.zero).magnitude + Mathf.Min(Mathf.Max(d1, d2), 0.0f);
    }

    // InfBox
    // b: size of box in x/y/z
    float sd2DBox(Vector2 p, Vector2 b)
    {
        Vector2 d = Abs2(p) - b;
        return Mathf.Sqrt( Vector2.SqrMagnitude(Max2(d, Vector2.zero))) + Mathf.Min(Mathf.Max(d.x, d.y), 0.0f);
    }
    float sd2DCylinder(Vector2 p, float c)
    {
        return p.magnitude - c;
    }

    // plane
    public float sdPlane(Vector3 p, Matrix4x4 _globalTransform)
    {

        float plane = _globalTransform.MultiplyPoint(p).x;
        return plane;
        
    }

    // Cross
    // s: size of cross
    float sdCross(in Vector3 p, float b)
    {
        float da = sd2DBox(new Vector2(p.x, p.y), Vector2.one * b);
        float db = sd2DBox(new Vector2(p.y, p.z), Vector2.one * b);
        float dc = sd2DBox(new Vector2(p.x, p.z), Vector2.one * b);
        return Mathf.Min(da, Mathf.Min(db, dc));
    }
    float sdCylinderCross(in Vector3 p, float b)
    {
        float da = sd2DCylinder(new Vector2(p.x, p.y), b);
        float db = sd2DCylinder(new Vector2(p.y, p.z), b);
        float dc = sd2DCylinder(new Vector2(p.x, p.z), b);
        return Mathf.Min(da, Mathf.Min(db, dc));
    }
    //trianglecross
    public float sdtriangleCross( Vector3 p, float b)
    {
        float da = sdTriPrism(new Vector2(p.x,p.y),new Vector2(b, b * 0.2f));
        float db = sdTriPrism(new Vector2(p.z,p.y),new Vector2(b, b * 0.2f));

        return Mathf.Min(da, db);
    }
    //pyramid
    float sdPyramid(Vector3 p, float h)
    {
        float m2 = h * h + 0.25f;

        p.x = Mathf.Abs(p.x);
        p.z = Mathf.Abs(p.z);
        if(p.z > p.x)
        {
            float pTemp = p.z;
            p.z = p.x;
            p.x = pTemp;
        }
        p.z -= 0.5f;
        p.x -= 0.5f;

        Vector3 q = new Vector3(p.z, h * p.y - 0.5f * p.x, h * p.x + 0.5f * p.y);

        float s = Mathf.Max(-q.x, 0.0f);
        float t = Mathf.Clamp((q.y - 0.5f * p.z) / (m2 + 0.25f), 0.0f, 1.0f);

        float a = m2 * (q.x + s) * (q.x + s) + q.y * q.y;
        float b = m2 * (q.x + 0.5f * t) * (q.x + 0.5f * t) + (q.y - m2 * t) * (q.y - m2 * t);

        float d2 = Mathf.Min(q.y, -q.x * m2 - q.y * 0.5f) > 0.0f ? 0.0f : Mathf.Min(a, b);

        return Mathf.Sqrt((d2 + q.z * q.z) / m2) * Mathf.Sign(Mathf.Max(q.z, -p.y));
    }

    //mirror x plane
    public Vector3 sdSymX(Vector3 p)
    {
        p.x = Abs(p).x;
        return p;
    }

    //mirror xz plane
    public Vector3 sdSymXZ(Vector3 p)
    {
        p = new Vector3 (Mathf.Abs(p.x),p.y, Mathf.Abs(p.z));
        return p;
    }
    //mirror all 3
    public Vector3 sdSymXYZ(Vector3 p)
    {
        p = Abs(p);
        return p;
    }

    //Menger Sponge
    public float sdMerger(Vector3 p, float b, int _iterations, Vector3 _modOffsetPos, Matrix4x4 _iterationTransform, Matrix4x4 _globalTransform, float _smoothRadius, float _scaleFactor)
    {
        p = _globalTransform.MultiplyPoint(p);

        float d = sdBox(p, new Vector3 (b - _smoothRadius, b - _smoothRadius, b - _smoothRadius)) - _smoothRadius;

        float s = 1.0f;
        for (int m = 0; m < _iterations; m++)
        {
            p = _iterationTransform.MultiplyPoint(p);
            p.x = mod(p.x, b * _modOffsetPos.x *2/s);
            p.y = mod(p.y, b * _modOffsetPos.y *2/s);
            p.z = mod(p.z, b * _modOffsetPos.z *2/s);


            s *= _scaleFactor * 3;
            Vector3 r = (p) * s;
            float c = (sdCross(r, b - _smoothRadius) - _smoothRadius) / s;
            //d = max(d,-c);


            if (-c > d)
            {
                d = -c;
                
            }

        }

        return d;
    }
    //Menger Cylinder
    public float sdMergerCyl(Vector3 p, float b, int _iterations, Vector3 _modOffsetPos, Matrix4x4 _iterationTransform, Matrix4x4 _globalTransform, float _smoothRadius, float _scaleFactor)
    {
        p = _globalTransform.MultiplyPoint(p);

        float d = sdSphere(p, b - _smoothRadius) - _smoothRadius;

        float s = 1.0f;
        for (int m = 0; m < _iterations; m++)
        {
            p = _iterationTransform.MultiplyPoint(p);
            p.x = mod(p.x, b * _modOffsetPos.x *2/s);
            p.y = mod(p.y, b * _modOffsetPos.y *2/s);
            p.z = mod(p.z, b * _modOffsetPos.z *2/s);


            s *= _scaleFactor * 3;
            Vector3 r = (p) * s;
            float c = (sdCylinderCross(r, b - _smoothRadius) - _smoothRadius) / s;
            //d = max(d,-c);


            if (-c > d)
            {
                d = -c;

            }

        }

        return d;
    }
  

    //merger piramid
    public float sdMergerPyr( Vector3 p, float b, int _iterations, Vector3 _modOffsetPos, Matrix4x4 _iterationTransform, Matrix4x4 _globalTransform, float _smoothRadius, float _scaleFactor)
    {
        b = 2 * b;
        p = _globalTransform.MultiplyPoint(p);


        float d = (sdPyramid(p / b, Mathf.Sqrt(3) / 2) * b);

        float s = 1.0f;
        for (int m = 0; m < _iterations; m++)
        {
            p = _iterationTransform.MultiplyPoint(p);
            //p = abs(p);
            p.x = mod(p.x, b * _modOffsetPos.x * 0.5f / s);
            p.y = mod(p.y, b * _modOffsetPos.y * (Mathf.Sqrt(3) / 2) / s);
            p.z = mod(p.z, b * _modOffsetPos.z * 0.5f / s);

            s *= _scaleFactor * 2;
            Vector3 r = (p) * s;
            float c = (sdtriangleCross(r, b / Mathf.Sqrt(3))) / s;

       

            if (-c > d)
            {
                d = -c;
                

            }
        }
        return d;
    }
    //negative sphere

    public float sdNegSphere(Vector3 p, float b, int _iterations, Vector3 _modOffsetPos, Matrix4x4 _iterationTransform, Matrix4x4 _globalTransform, float _sphere1, float _scaleFactor)
    {
        p = _globalTransform.MultiplyPoint(p);


        float d = sdBox(p, new Vector3(b, b, b));

        float s = 1.0f;
        for (int m = 0; m < _iterations; m++)
        {
            p = _iterationTransform.MultiplyPoint(p);
            p.x = mod(p.x, b * _modOffsetPos.x * 2 / s);
            p.y = mod(p.y, b * _modOffsetPos.y * 2 / s);
            p.z = mod(p.z, b * _modOffsetPos.z * 2 / s);
            s *= _sphere1;
            Vector3 r = (p) * s;
            float c = sdSphere(r, b) / s;
            s *= _scaleFactor * 3;

            if (-c > d)
            {
                d = -c;


            }
        }
        return d;

    }


    //modulor operator
    public float mod(float a, float n)
    {
        float halfsize = n / 2;
        //float result;
        /*
        if (a < 0)
        {
            result = -((-a + halfsize) % n - halfsize);
        }
        else result = (a + halfsize) % n - halfsize;
        */
        a = (a + halfsize) % n - halfsize;
        a = (a - halfsize) % n + halfsize;

        return a;
    }

    //returs the absolute value of a vector
    public Vector3 Abs(Vector3 vec)
    {
        return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
    }

    //returs the absolute value of a vector
    public Vector2 Abs2(Vector2 vec)
    {
        return new Vector2(Mathf.Abs(vec.x), Mathf.Abs(vec.y));
    }

    //returns the Largest Vector
    public Vector3 Max(Vector3 vec1, Vector3 vec2)
    {
        return new Vector3(Mathf.Max(vec1.x, vec2.x), Mathf.Max(vec1.y, vec2.y), Mathf.Max(vec1.z, vec2.z));
    }

    //returns the Smallest Vector
    public Vector3 Min(Vector3 vec1, Vector3 vec2)
    {
        return new Vector3(Mathf.Max(vec1.x, vec2.x), Mathf.Max(vec1.y, vec2.y), Mathf.Max(vec1.z, vec2.z));
    }

    //returns the Largest Vector
    public Vector2 Max2(Vector2 vec1, Vector2 vec2)
    {
        return new Vector2(Mathf.Max(vec1.x, vec2.x), Mathf.Max(vec1.y, vec2.y));
    }

    //returns the Smallest Vector
    public Vector2 Min2(Vector2 vec1, Vector2 vec2)
    {
        return new Vector2(Mathf.Max(vec1.x, vec2.x), Mathf.Max(vec1.y, vec2.y));
    }



}

