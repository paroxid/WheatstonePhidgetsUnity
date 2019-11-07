using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetTextPhidgetDebug : MonoBehaviour
{
    public GameObject PhidgetsManager;
    public Text mytext;

    public PhidgetManager localPhidgetsmanager;

    // Start is called before the first frame update
    void Start()
    {
        localPhidgetsmanager = PhidgetsManager.GetComponent<PhidgetManager>(); // get reference of remote script
        mytext = gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        mytext.text = localPhidgetsmanager.horizontalWeight.ToString() + "   " + localPhidgetsmanager.verticalWeight.ToString();
    }
}
