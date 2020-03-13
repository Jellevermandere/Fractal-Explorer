using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float initMaxSpeed = 10f;
    [HideInInspector]
    public float maxSpeed = 10f;
    public float maxMaxSpeed = 50f;
    public float rotationSpeed = 20;
    public float acceleration = 100;
        

        //private float roll = 0;
    private float pitch;
    private float yaw;
    
    private bool giveForce;
    private AudioSource collectSound;
    private Rigidbody rb;
    private Animator anim;
    
        // Start is called before the first frame update
    void Start()
    {
        anim = GameObject.FindGameObjectWithTag("PlayerMesh").GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

        // Update is called once per frame
    void Update()
    {
        CheckControls();
    }

    private void FixedUpdate()
    {
        ApplyRot();
        if (giveForce)
        {
            ApplyForce();
        }
    }

        //checks the controlls and rotates the player
    void CheckControls()
    {
        if (Input.GetKey("space"))
        {
            giveForce = true;
            anim.SetBool("IsFlying", true);
            //Debug.Log(anim.GetBool("IsFlying"));

        }
        else
        {
            giveForce = false;
            anim.SetBool("IsFlying", false);
        }

        //roll = Input.GetAxis("Roll") * rotationSpeed;
        pitch = Input.GetAxis("Pitch") * rotationSpeed;
        yaw = Input.GetAxis("Yaw") * rotationSpeed;


    }

        //Applies the rotation to the player
    void ApplyRot()
    {

        //type1 pitch and roll
        //transform.SetPositionAndRotation(transform.position, transform.rotation * Quaternion.Euler(pitch, 0, -yaw));

        //type2 pitch yaw and roll
        transform.SetPositionAndRotation(transform.position, transform.rotation * Quaternion.Euler(-pitch, yaw, -Mathf.Abs(pitch) * yaw / 2));

        //type3 pitch and yaw
        //transform.SetPositionAndRotation(transform.position, transform.rotation * Quaternion.Euler(pitch, yaw, 0 ));

        //type4 - pitch yaw and roll
        //transform.SetPositionAndRotation(transform.position, transform.rotation * Quaternion.Euler(pitch, yaw, -Mathf.Abs(pitch) * yaw / 2));

        //type5 - pitch and yaw
        //transform.SetPositionAndRotation(transform.position, transform.rotation * Quaternion.Euler(pitch, yaw, 0 ));

    }


    //applies a forwards force
    void ApplyForce()
    {
        
        //Debug.Log(giveForce);
        rb.AddForce(transform.forward * (Time.deltaTime * acceleration * (rb.velocity.magnitude+1)), ForceMode.Impulse);

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    }


}
