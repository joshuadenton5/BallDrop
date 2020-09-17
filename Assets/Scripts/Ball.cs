using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody rb;
    public float speed;
    private GameManager gm;
    private AudioManager audioManager;
    int mobileSpeed = 5;
    float startX;
    float startY;
    
    void Awake()
    {
        speed = 3f;
        rb = GetComponent<Rigidbody>();
        gm = FindObjectOfType<GameManager>();
        audioManager = FindObjectOfType<AudioManager>();
        startY = Input.acceleration.y;
        startX = Input.acceleration.x;
    }
   
    public Rigidbody GetRigidbody()
    {
        return rb;
    }

    void FixedUpdate()
    {
        if (UI.inGame)
        {
            if (SystemInfo.deviceType == DeviceType.Desktop) //for cross platform PC
            {
                float hor = Input.GetAxis("Horizontal");
                float vert = Input.GetAxis("Vertical");
                Vector3 move = new Vector3(hor, 0.0f, vert);
                rb.AddForce(move * speed);
            }
            else //for the mobile apk
            {               
                Vector3 movement = new Vector3(-(startX - Input.acceleration.x), 0.0f, -(startY - Input.acceleration.y));
                //movement = Quaternion.Euler(90, 0, 0) * movement;
                rb.AddForce(movement * (speed + mobileSpeed));
            }
        }       
    }  

    private void OnTriggerEnter(Collider other) //tirigger collisions
    {
        if (other.tag == "End")
        {          
            gm.OnSpawn();
            rb.AddForce(-transform.up);
            audioManager.Stop("CountDown");
        }      
        else if(other.tag == "PlusTime")
        {
            StartCoroutine(gm.UpdateTime(gm.TimeToAdd()));
            Destroy(other.gameObject);
        }
        else if(other.tag == "Points")
        {
            StartCoroutine(gm.UpdateScore(25));
            Destroy(other.gameObject);
        }
        else if (other.tag == "Pillar")
        {
            Destroy(other.gameObject);
            Debug.Log(other + "What");
        }
    }
}
