using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebBehiavor : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<LifeManager>())
        {
            collision.gameObject.GetComponent<LifeManager>().KnockBack(collision.gameObject, 10, this.gameObject);
            GetComponent<SoundContainer>().PlaySound("WebSound", 1);
        }
    }
}
