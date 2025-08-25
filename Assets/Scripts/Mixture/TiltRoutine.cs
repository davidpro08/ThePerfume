using System.Collections;
using UnityEngine;

public class TiltRoutine : MonoBehaviour
{
    [SerializeField] private Transform liquidTransform;
    private Quaternion liquidInitialRotation;

    void Start()
    {
        liquidInitialRotation = liquidTransform.rotation;
    }

    IEnumerator Tilt(float targetZ, float duration)
    {
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(0, 0, targetZ);

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            transform.rotation = Quaternion.Lerp(startRot, endRot, t);

            liquidTransform.rotation = liquidInitialRotation;

            yield return null;
        }
    }
}
