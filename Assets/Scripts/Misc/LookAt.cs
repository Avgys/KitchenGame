using UnityEngine;

public class LookAt : MonoBehaviour
{
    private void LateUpdate()
    {
        var position = Camera.main.transform.position;
        position.x = transform.position.x;
        transform.LookAt(2 * transform.position - position);
    }
}
