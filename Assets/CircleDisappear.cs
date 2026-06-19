using UnityEngine;
using UnityEngine.UI;


public class CircleDisappear : MonoBehaviour
{

    public Image circle;

    public float duration = 5f;

    float timer;


    void Start()
    {
        timer = duration;
        circle.fillAmount = 1;
    }


    void Update()
    {

        timer -= Time.deltaTime;


        float value = timer / duration;


        circle.fillAmount = value;


        if(value <= 0)
        {
            circle.fillAmount = 0;
        }

    }

}