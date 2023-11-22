using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse : MonoBehaviour
{
    [SerializeField] SpriteRenderer _Renderer;

    bool _Fade = false;
    IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);
        _Fade = true;
        Debug.Log("Fade effect");
        yield return null;
    }

    void Update()
    {
        if (!_Fade) return;
        var color = _Renderer.color;
        color.a -= Time.deltaTime / 2;
        _Renderer.color = color;
        if (color.a <= 0) Destroy(gameObject);
    }
}
