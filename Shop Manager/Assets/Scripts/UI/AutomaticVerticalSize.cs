using UnityEngine;
using System.Collections;

public class AutomaticVerticalSize : MonoBehaviour {

    public float childheight = 30f;

    void Start ()
    {

    }

	public void AdjustSize ()
    {
		Vector2 size = this.GetComponent<RectTransform>().sizeDelta;
		size.y = this.transform.childCount * childheight;
        this.GetComponent<RectTransform>().sizeDelta = size;
    }
}
