using UnityEngine;

public class CamsFollow : MonoBehaviour
{
    public Transform CamPosition;
    void Update()
    {
        transform.position = CamPosition.transform.position;
    }
}
