using TarodevController;
using UnityEngine;

public class Flip : MonoBehaviour
{
    public FrameInput _frameInput;

    private void FixedUpdate()
  
   {
        if (_frameInput.Move.x == 0) return;

        float scaleX = Mathf.Abs(transform.localScale.x) * Mathf.Sign(_frameInput.Move.x);
        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
    }

}

