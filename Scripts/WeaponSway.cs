using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    private Vector3 initpos;

    public float Amount = 0.1f;
    public float MaxAmount = 0.3f;
    public float SmoothAmount = 6.0f;

    private void Start()
    {
        initpos = transform.localPosition;
    }

    private void Update()
    {
        float moveX = -Input.GetAxis("Mouse X") * Amount;
        float moveY = -Input.GetAxis("Mouse Y") * Amount;
        moveX = Mathf.Clamp(moveX, -MaxAmount, MaxAmount);
        moveY = Mathf.Clamp(moveY, -MaxAmount, MaxAmount);

        Vector3 FinalPosToMove = new Vector3(moveX, moveY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, FinalPosToMove + initpos, Time.deltaTime * SmoothAmount);
    }
}
