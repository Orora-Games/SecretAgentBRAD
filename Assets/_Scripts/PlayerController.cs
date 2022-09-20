using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 6f; 
    public float turnSmoothTime = 0.1f;
    public ArrayList  Materials;
    private float startHeight;

    private GameObject guardDogObject;

    private float turnSmoothVelocity;

    // Start is called before the first frame update
    void Start()
    {
        startHeight = gameObject.transform.position.y;

    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        /*This next one is supposed to move Player back to the ground if they manage to bug themselves to a floating position. */
        if (Mathf.Abs(transform.position.y - startHeight) >= 0.001)
        {
            transform.position = new Vector3(transform.position.x, startHeight, transform.position.z);
        }
        /* This block makes Player Turn the way they're moving. */
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg - 90; // Remove when new model comes in.
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            controller.Move(direction * speed * Time.deltaTime);
        } 
    }

}
