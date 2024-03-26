using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteract : MonoBehaviour
{
    public void HelloWorld()
    {
        Debug.Log("Hello World");
    }

    public void TakeParam(string message)
    {
        Debug.Log(message);
    }
}
