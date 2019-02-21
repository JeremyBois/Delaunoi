using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BreaklineText : MonoBehaviour
{
    private Text textWithBreaklines;

    void Start()
    {
        textWithBreaklines = GetComponent<Text>();

        if (textWithBreaklines)
        {
            textWithBreaklines.text = textWithBreaklines.text.Replace("\\n ", "\n"); ;
        }
    }

}
