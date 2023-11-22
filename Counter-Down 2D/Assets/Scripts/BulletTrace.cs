using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrace : MonoBehaviour
{
    [SerializeField] Rigidbody2D _RigidBody;

    bool _Stop = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) return;
        var impact = Instantiate(Server.Instance.BulletParticle[0], transform.position - transform.up, transform.rotation);

        Destroy(impact, 2f);
        Destroy(gameObject);
    }

    void StopMoving()
    {
        _Stop = true;
    }

    IEnumerator Start()
    {
        _RigidBody.velocity = transform.up * 30;
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
        yield return null;
    }

    void Update()
    {
        if (_Stop)
        {
            _RigidBody.velocity = Vector2.zero;
            return;
        }
        _RigidBody.AddForce(transform.up * Time.fixedDeltaTime * 100f, ForceMode2D.Impulse);
    }
}
