using UnityEngine;
using System.Collections;

public class AutomaticHorizontalSize : MonoBehaviour {

    public float childwidth = 100f;

	public void AdjustSize ()
    {
        Vector2 size = this.GetComponent<RectTransform>().sizeDelta;
		size.x = this.transform.childCount * childwidth;
        this.GetComponent<RectTransform>().sizeDelta = size;
    }
}
