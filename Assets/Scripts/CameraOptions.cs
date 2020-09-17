using UnityEngine;

public class CameraOptions : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "End")
        {
            Destroy(other.gameObject);
        }
    }
}
