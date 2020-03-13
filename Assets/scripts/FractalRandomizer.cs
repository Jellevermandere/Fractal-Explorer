using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractalRandomizer : MonoBehaviour
{
    public float animationSpeed;

    public float positionRandom;
    public float rotationRandom;
    public float globalRotationRandom;
    public float scaleRandom;

    public GameObject fractalCam;
    public GameObject playerCam;
    public Transform player;
    private RaymarchCam camScript;
    
    
    private Vector3 posRand;
    private Vector3 globRotRand;
    private Vector3 rotRand;
    private float scaleRand;
    private float startTime;
    private float randColor;
    private float sphereRand;
    private float sphereScaleRand;
    private float globScaleRand;
    
    
    

    // Start is called before the first frame update
    void Start()
    {
        camScript = GetComponent<RaymarchCam>();
        playerCam.SetActive(false);

        SetFractal();

        SetRandColor();

        posRand = new Vector3(Random.Range(-positionRandom, positionRandom), Random.Range(-positionRandom, positionRandom), Random.Range(-positionRandom, positionRandom));
        rotRand = new Vector3(Random.Range(-rotationRandom, rotationRandom), Random.Range(-rotationRandom, rotationRandom), Random.Range(-rotationRandom, rotationRandom));
        globRotRand = new Vector3(Random.Range(-globalRotationRandom, globalRotationRandom), Random.Range(-globalRotationRandom, globalRotationRandom), Random.Range(-globalRotationRandom, globalRotationRandom));
        scaleRand = 1f + Random.Range(-scaleRandom, scaleRandom);
        sphereScaleRand = 1f - Random.Range(0, scaleRandom);
        sphereRand = Random.Range(1.0f, 3.0f);
        globScaleRand = Random.Range(15.0f, 500.0f);

        if (globScaleRand > 400)
        {
            camScript._iterations = 5;
        }
        else if (globScaleRand > 200)
        {
            camScript._iterations = 4;
        }
        else if (globScaleRand > 50)
        {
            camScript._iterations = 3;
        }
        else
        {
            camScript._iterations = 2;
        }

        fractalCam.transform.position = new Vector3(-globScaleRand * 4, globScaleRand * 4, -globScaleRand * 4);
        player.position = new Vector3(0, 0, -globScaleRand * 1.5f);

    }

    // Update is called once per frame
    void Update()
    {
        

        if (startTime < 1)
        {
            moveFractal();
        }

    }

    void moveFractal()
    {
       
            camScript._iterationOffsetPos = Vector3.Slerp(Vector3.zero, posRand, startTime);
            camScript._iterationOffsetRot = Vector3.Slerp(Vector3.zero, rotRand, startTime);
            camScript._GlobalRotation = Vector3.Slerp(Vector3.zero, globRotRand, startTime);
            camScript._scaleFactor = Mathf.Lerp(1, scaleRand, startTime);
            camScript._GlobalScale = Mathf.Lerp(15, globScaleRand, startTime);
            camScript._innerSphereRad = Mathf.Lerp(1, sphereScaleRand, startTime);

        startTime += Time.deltaTime * animationSpeed;

            if (startTime >= 1)
            {
                startTime = 1;
                camScript._iterationOffsetPos = posRand;
                camScript._iterationOffsetRot = rotRand;
                camScript._GlobalRotation = globRotRand;
                camScript._scaleFactor = scaleRand;
                camScript._GlobalScale = globScaleRand;

                playerCam.SetActive(true);
            }
        
        
    }

    void SetFractal()
    {
        camScript._drawMergerSponge = false;
        camScript._drawMergerCylinder = false;
        camScript._drawMergerPyramid = false;
        camScript._drawNegativeSphere = false;
        camScript._drawSphere = false;
        camScript._drawBox = false;

        int fractalnr = Random.Range(1, 5);

        switch (fractalnr)
        {
            case 1:
                camScript._drawMergerSponge = true;
                
                Debug.Log("MengerSponge");
                break;
            case 2:
                camScript._drawMergerCylinder = true;
                Debug.Log("MengerCylinder");
                break;
            case 3:
                camScript._drawMergerPyramid = true;
                Debug.Log("MengerPyramid");
                break;
            case 4:
                camScript._drawNegativeSphere = true;
                camScript._useModulor = true;
                Debug.Log("inversesphere");
                break;

                
            case 5:
                camScript._drawSphere = true;
                camScript._useModulor = true;
                Debug.Log("Sphere");
                break;

                
            case 6:
                camScript._drawBox = true;
                camScript._useModulor = true;
                Debug.Log("box");
                break;
            case 0:
                Debug.Log("none of the above");
                break;
        }
    }

    void SetRandColor()
    {
        randColor = Random.Range(0.0f, 1.0f);
        camScript._mainColor = Color.HSVToRGB(randColor, 0.95f, 0.9f);
        if (randColor > 2f / 3f)
        {
            camScript._secColor = Color.HSVToRGB(randColor - 1f / 3f, 0.9f, 0.9f);
            camScript._skyColor = Color.HSVToRGB(randColor - 2f / 3f, 0.3f, 0.95f);
            Camera.main.backgroundColor = Color.HSVToRGB(randColor - 2f / 3f, 0.2f, 0.95f);
        }
        else if (randColor < 1f / 3f)
        {
            camScript._secColor = Color.HSVToRGB(randColor + 2f / 3f, 0.9f, 0.9f);
            camScript._skyColor = Color.HSVToRGB(randColor + 1f / 3f, 0.3f, 0.95f);
            Camera.main.backgroundColor = Color.HSVToRGB(randColor + 1f / 3f, 0.2f, 0.95f);
        }
        else
        {
            camScript._secColor = Color.HSVToRGB(randColor - 1f / 3f, 0.9f, 0.9f);
            camScript._skyColor = Color.HSVToRGB(randColor + 1f / 3f, 0.3f, 0.95f);
            Camera.main.backgroundColor = Color.HSVToRGB(randColor + 1f / 3f, 0.2f, 0.95f);
        }
        RenderSettings.ambientLight = Camera.main.backgroundColor;
        RenderSettings.fogColor = camScript._skyColor;
    }
}
