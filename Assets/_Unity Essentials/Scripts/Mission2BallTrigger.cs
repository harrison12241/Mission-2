using UnityEngine;

public class Mission2BallTrigger : MonoBehaviour
{
    public Rigidbody[] blocks;
    bool released;

    void Start()
    {
        var body = GetComponent<Rigidbody>();
        body.linearVelocity = new Vector3(3.2f, 0f, 0f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (released || !collision.gameObject.name.StartsWith("Tower Block")) return;
        released = true;
        foreach (var block in blocks)
        {
            if (block != null)
            {
                block.isKinematic = false;
                block.WakeUp();
            }
        }
    }
}
