using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public Vector3 targetPos;
    private Vector3 startPos;
    public float speed = 2f;

    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float pingPong = Mathf.PingPong(Time.time * speed, 1);
        transform.position = Vector3.Lerp(startPos, targetPos, pingPong);
    }
}
