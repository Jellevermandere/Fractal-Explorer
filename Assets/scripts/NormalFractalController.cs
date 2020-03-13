using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalFractalController : MonoBehaviour
{
    public GameObject playerCam;
    public float AnimSpeed;

    private RaymarchCam camScript;
    private float randColor;
    //private Animation anim;


    // Start is called before the first frame update
    void Start()
    {
        camScript = GetComponent<RaymarchCam>();
        playerCam.SetActive(false);
        

        SetRandColor();
        /*
         *  // plays specific animation of that level
         *  anim = GetComponent<Animation>();
         *  anim.Play("Level"+SceneManager.GetActiveScene().buildIndex.ToString()); 
        */

        Invoke("ActivatePlay", 1 / AnimSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ActivatePlay()
    {
        playerCam.SetActive(true);
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
