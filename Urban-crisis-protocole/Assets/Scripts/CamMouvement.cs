using UnityEngine;
using UnityEngine.InputSystem;


public class CamMouvement : MonoBehaviour
{
    private InputAction mouse;
    void Start()
    {

        mouse = InputSystem.actions.FindAction("Look");
    }


    void Update()
    {
        MoveAroundWithMouse();
    }

    // !! can change :  right click to move around, scroll button to rotate

    // zomming variable
    [SerializeField] private float minHeightCam = 4f;
    [SerializeField] private float maxHeightCam = 8f;
    [SerializeField] private float zommingSpeed = 2;

    // player mouvement variable
    [SerializeField] private float movingSpeed = 2;

    // player rotation variable
    [SerializeField] private float rotateSpeed = 2;



    void MoveAroundWithMouse()
    {
        // Action : zoom will scrolling 

        float scrollValue = Mouse.current.scroll.ReadValue().y;

        if (scrollValue != 0)
        {

            Vector3 mouvementMade = scrollValue > 0 ? transform.forward : -transform.forward;
            mouvementMade *= Time.deltaTime * zommingSpeed;
            Vector3 testVector = mouvementMade + transform.position;

            if (testVector.y >= minHeightCam && testVector.y <= maxHeightCam)
            {
                transform.position = testVector;
            }
        }


        Vector2 mouseMouvement = mouse.ReadValue<Vector2>();


        // Action : right click to move around  
        // !! min and max to prevent the player from going to far , optional : add a timer to prevent the player from click without stopping

        Vector2 nextMove = mouseMouvement;
        nextMove = -nextMove.normalized;
        nextMove *= movingSpeed * Time.deltaTime;

        if (Mouse.current.rightButton.IsPressed())
        {
            transform.position += nextMove.x * transform.right + nextMove.y * new Vector3(transform.forward.x, 0, transform.forward.z);

        }



        // Action : rotate aroung while pressing the scrolling button
        Vector2 nextRotation = mouseMouvement;
        if (Mouse.current.middleButton.IsPressed())
        {
            transform.Rotate(0, -nextRotation.x * Time.deltaTime * rotateSpeed, 0, Space.World);
        }
    }
}
