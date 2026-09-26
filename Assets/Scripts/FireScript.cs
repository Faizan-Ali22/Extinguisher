using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Water"))
        {
            Invoke("destroyFire", 3f);
        }
    }
    private void destroyFire()
    {
        GameManager.Instance.AddScore(5);
        Destroy(this.gameObject);
    }
}
