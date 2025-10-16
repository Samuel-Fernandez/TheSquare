using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotionsBehiavor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<ObjectAnimation>().PlayAnimation("emotionAnimation");
        Destroy(gameObject, 1);
    }

}
