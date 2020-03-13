using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionPlaneController : MonoBehaviour
{
    public float maxDistance = 20.0f;
    public float lifeTime;
    private Transform player;
    private float timeAlive;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        transform.LookAt(2 * transform.position - player.position); // rotates the plane towards the player
        timeAlive = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timeAlive += Time.deltaTime;
        if (DistanceToPlayer()> maxDistance || timeAlive > lifeTime)
        {
            Destroy(gameObject); // destroy after a certain time
        }
    }

    float DistanceToPlayer()
    {
        return Vector3.Distance(player.position, transform.position);
    }
}
