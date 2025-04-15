// AsteroidSpin.cs
using UnityEngine;

public class AsteroidSpin : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 30f, 0);

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
