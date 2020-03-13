using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerRayMarchCollider : MonoBehaviour
{
    
    public int nrPoints = 18;
    public float colliderOffset = 1.2f;
    public GameObject DistanceSphere;
    public GameObject colPlane;

    private DistanceFunctions Df;
    private PlayerController playCon;
    private float colRadius;
    private RaymarchCam camScript;
    private Vector3 ro;
    Vector3[] colliders;
    GameObject[] lines;
    GameObject modLine;

    // Start is called before the first frame update
    void Start()
    {
        camScript = Camera.main.GetComponent<RaymarchCam>();
        colRadius = GetComponent<SphereCollider>().radius;
        Df = GetComponent<DistanceFunctions>();
        playCon = GetComponent<PlayerController>();
        colliders = PointsOnSphere(nrPoints, colRadius * colliderOffset);
        //lines = new GameObject[nrPoints];
        //AddLines();

    }

    // Update is called once per frame
    void Update()
    {

        if (IsClose())
        {
            RayMarch(colliders);
        }
    }

    // the distancefunction from the player
    public float DistanceField(Vector3 p)
    {
        float dist = Camera.main.farClipPlane + 1;

        // check symmetry
        if (camScript._useSymmetry)
        {
            p = Df.sdSymXYZ(p);
        }
        // check modulor
        if (camScript._useModulor)
        {
            p.x = Df.mod(p.x, camScript._modInterval.x * camScript._GlobalScale * 2);
            p.y = Df.mod(p.y, camScript._modInterval.y * camScript._GlobalScale * 2);
            p.z = Df.mod(p.z, camScript._modInterval.z * camScript._GlobalScale * 2);
        }
        //menger sponge
        if (camScript._drawMergerSponge)
        {
            dist = Df.sdMerger(p, camScript._GlobalScale, camScript._iterations, camScript._modOffsetPos, camScript._iterationTransform.inverse, camScript._globalTransform.inverse, camScript._smoothRadius, camScript._scaleFactor);

        }
        // menger cylinder
        else if (camScript._drawMergerCylinder)
        {
            dist = Df.sdMergerCyl(p, camScript._GlobalScale, camScript._iterations, camScript._modOffsetPos, camScript._iterationTransform.inverse, camScript._globalTransform.inverse, camScript._smoothRadius, camScript._scaleFactor);

        }
        // terpinski triangle menger pyramid
        else if (camScript._drawMergerPyramid)
        {
            dist = Df.sdMergerPyr(p, camScript._GlobalScale, camScript._iterations, camScript._modOffsetPos, camScript._iterationTransform.inverse, camScript._globalTransform.inverse, camScript._smoothRadius, camScript._scaleFactor);

        }
        // negative sphere
        else if (camScript._drawNegativeSphere)
        {
            dist = Df.sdNegSphere(p, camScript._GlobalScale, camScript._iterations, camScript._modOffsetPos, camScript._iterationTransform.inverse, camScript._globalTransform.inverse, camScript._innerSphereRad, camScript._scaleFactor);
        }
        // sphere
        else if (camScript._drawSphere)
        {
            dist = Df.sdSphere(p - camScript._globalPosition, camScript._GlobalScale);
        }
        // box
        else if (camScript._drawBox)
        {
            dist = Df.sdBox(p - camScript._globalPosition, Vector3.one * camScript._GlobalScale);
        }
        
        
        // check section plane
        if (camScript._useSectionPlane)
        {
            float plane = Df.sdPlane(p, camScript._sectionTransform.inverse);
            return Mathf.Max(dist, plane);
        }
        
        return dist;

        /* debug modulor line
       //modLine.GetComponent<LineRenderer>().SetPosition(0, p);
       //modLine.GetComponent<LineRenderer>().SetPosition(1, p - dist);
       */
    }

    // the raymarcher from the player
    void RayMarch(Vector3[] rd)
    {
        ro = transform.position;
        int nrHits = 0;

        for (int i = 0; i < rd.Length; i++)
        {
            Vector3 p = ro + Vector3.Normalize(rd[i]) * colRadius;
            //check hit
            float d = DistanceField(p);


            if (d < 0.001) //hit
            {
                Debug.Log("hit" + i);
                nrHits++;
                //collision
                SetColPlane(rd[i]);

            }
            // resets player position if stuck with to many collisions at once
            if (nrHits > rd.Length * 0.45f)
            {
                transform.position = new Vector3(0, 0, camScript._GlobalScale * 1.6f);
            }
            
        }
    }
    // sets the collision plane
    private void SetColPlane(Vector3 hitPoint)
    {
        Instantiate(colPlane, hitPoint + transform.position, Quaternion.identity);
    }

    // checks if the player is close
    bool IsClose()
    {

        float d = DistanceField(transform.position);
        //Debug.Log(d);
        // DistanceSphere.transform.localScale = Vector3.one * d * 2; // debug distance sphere
        playCon.maxSpeed = Mathf.Min(playCon.maxMaxSpeed, Mathf.Sqrt(d) * playCon.initMaxSpeed); // player speed is regulated by distance
        return d - (colRadius * colliderOffset) < 0.001;

    }

    //creates a fixed number of points on a sphere
    Vector3[] PointsOnSphere(int n, float b)
    {
        List<Vector3> upts = new List<Vector3>();
        float inc = Mathf.PI * (3 - Mathf.Sqrt(5));
        float off = 2.0f / n;
        float x = 0;
        float y = 0;
        float z = 0;
        float r = 0;
        float phi = 0;

        for (var k = 0; k < n; k++)
        {
            y = k * off - 1 + (off / 2);
            r = Mathf.Sqrt(1 - y * y);
            phi = k * inc;
            x = Mathf.Cos(phi) * r;
            z = Mathf.Sin(phi) * r;

            upts.Add(new Vector3(x, y, z) * b);
        }
        Vector3[] pts = upts.ToArray();
        return pts;
    }

    // creates lines for debugging
    void AddLines()
    {
        modLine = new GameObject("modLine");
        modLine.AddComponent<LineRenderer>();
        modLine.GetComponent<LineRenderer>().startWidth = 0.05f;
        modLine.GetComponent<LineRenderer>().endWidth = 0.05f;

        for (int i = 0; i < colliders.Length; i++)
        {
            lines[i] = new GameObject("line " + i);
            lines[i].transform.parent = transform;
            lines[i].AddComponent<LineRenderer>();
            lines[i].GetComponent<LineRenderer>().startWidth = 0.05f;
            lines[i].GetComponent<LineRenderer>().endWidth = 0.05f;
            lines[i].GetComponent<LineRenderer>().SetPosition(0, transform.position);
            lines[i].GetComponent<LineRenderer>().SetPosition(1, transform.position + colliders[i]);
            lines[i].GetComponent<LineRenderer>().useWorldSpace = false;


        }
    }

}
