using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractalCreator : MonoBehaviour
{
    [Range(0, 1)]
    public float color;

    [Range(1, 6)]
    public int fractalType;

    private RaymarchCam camScript;
    private GameObject player;
    private PlayerRayMarchCollider col;
    private bool playerPause;

    // Start is called before the first frame update
    void Start()
    {
        camScript = GetComponent<RaymarchCam>();
        player = GameObject.FindGameObjectWithTag("Player");
        col = player.GetComponent<PlayerRayMarchCollider>();
    }
    private void Update()
    {
        while(col.DistanceField(player.transform.position) < 3 && playerPause == true)
        {
            player.transform.position += new Vector3(0, 0, -1);
        }
    }

    public void PlayerPause(bool pause)
    {
        playerPause = pause;
    }

    //mod Settings

    public void UseMod(bool use)
    {
        camScript._useModulor = use;
    }

    public void ModX (string x)
    {
        camScript._modInterval.x = float.Parse(x);
    }
    public void ModY(string y)
    {
        camScript._modInterval.y = float.Parse(y);
    }
    public void ModZ(string z)
    {
        camScript._modInterval.z = float.Parse(z);
    }

//Fractal Settings

    public void SetFractal(int nr)
    {


        camScript._drawMergerSponge = false;
        camScript._drawMergerCylinder = false;
        camScript._drawMergerPyramid = false;
        camScript._drawNegativeSphere = false;
        camScript._drawSphere = false;
        camScript._drawBox = false;

        switch (nr)
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
                Debug.Log("inversesphere");
                break;

            case 5:
                camScript._drawSphere = true;
                Debug.Log("Sphere");
                break;

            case 6:
                camScript._drawBox = true;
                Debug.Log("box");
                break;

            case 0:
                Debug.Log("none of the above");
                break;
        }
    }

    public void SetIterations(string nr)
    {
        camScript._iterations = int.Parse(nr);
    }

    public void SetScaleFactor (string factor)
    {
        camScript._scaleFactor = float.Parse(factor);
    }


    public void ModOffsetX(string x)
    {
        camScript._modOffsetPos.x = float.Parse(x);
    }
    public void ModOffsetY(string y)
    {
        camScript._modOffsetPos.y = float.Parse(y);
    }
    public void ModOffsetZ(string z)
    {
        camScript._modOffsetPos.z = float.Parse(z);
    }


    public void ItOffsetPosX(string x)
    {
        camScript._iterationOffsetPos.x = float.Parse(x);
    }
    public void ItOffsetPosY(string y)
    {
        camScript._iterationOffsetPos.y = float.Parse(y);
    }
    public void ItOffsetPosZ(string z)
    {
        camScript._iterationOffsetPos.z = float.Parse(z);
    }


    public void ItOffsetRotX(string x)
    {
        camScript._iterationOffsetRot.x = float.Parse(x);
    }
    public void ItOffsetRotY(string y)
    {
        camScript._iterationOffsetRot.y = float.Parse(y);
    }
    public void ItOffsetRotZ(string z)
    {
        camScript._iterationOffsetRot.z = float.Parse(z);
    }

//Transform Settings

    public void GobalPosX(string x)
    {
        camScript._globalPosition.x = float.Parse(x);
    }
    public void GobalPosY(string y)
    {
        camScript._globalPosition.y = float.Parse(y);
    }
    public void GobalPosZ(string z)
    {
        camScript._globalPosition.z = float.Parse(z);
    }

    public void GobalRotX(string x)
    {
        camScript._GlobalRotation.x = float.Parse(x);
    }
    public void GobalRotY(string y)
    {
        camScript._GlobalRotation.y = float.Parse(y);
    }
    public void GobalRotZ(string z)
    {
        camScript._GlobalRotation.z = float.Parse(z);
    }

    public void SetScale (string scale)
    {
        camScript._GlobalScale = float.Parse(scale);
    }

    public void SetInnerRad (string rad)
    {
        camScript._innerSphereRad = float.Parse(rad);
    }

//plane Settings

    public void SetSectionPlane (bool use)
    {
        camScript._useSectionPlane = use;
    }

    public void SetSymmetry(bool use)
    {
        camScript._useSymmetry = use;
    }

    public void SectionPosX(string x)
    {
        camScript._sectionPos.x = float.Parse(x);
    }
    public void SectionPosY(string y)
    {
        camScript._sectionPos.y = float.Parse(y);
    }
    public void SectionPosZ(string z)
    {
        camScript._sectionPos.z = float.Parse(z);
    }

    public void SectionRotX(string x)
    {
        camScript._sectionRot.x = float.Parse(x);
    }
    public void SectionRotY(string y)
    {
        camScript._sectionRot.y = float.Parse(y);
    }
    public void SectionRotZ(string z)
    {
        camScript._sectionRot.z = float.Parse(z);
    }

    public void SetColor(float randColor)
    {
        
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
