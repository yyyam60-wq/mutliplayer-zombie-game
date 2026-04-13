using UnityEngine;

public class Character : MonoBehaviour
{
    public Animator animator;

    void Update()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.LeftShift))
        {
            Player.instance.CallPlayerMoveAnimation(2 , true);
        }
        else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S))
        {
            Player.instance.CallPlayerMoveAnimation(1 , true);
        }else Player.instance.CallPlayerMoveAnimation(1 , false);
    }
}