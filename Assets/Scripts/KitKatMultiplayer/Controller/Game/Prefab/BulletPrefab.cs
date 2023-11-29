using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPrefab : MonoBehaviour
{
    public BulletInfo BulletInfo;
    long PlayerAccountId;
    float bulletSpeed;
    int bulletDamage;
    float bulletLifeTime = 1;

    // private void FixedUpdate()
    // {
    //     transform.position += transform.forward * Time.fixedDeltaTime * bulletSpeed;
    //     bulletLifeTime -= Time.fixedDeltaTime;

    //     if (bulletLifeTime <= 0)
    //     {
    //         Destroy(gameObject);
    //     }
    // }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.gameObject.tag == "Player")
    //     {
    //         var player = other.GetComponent<GamePlayerPrefab>();
    //         player?.OnPlayerGetBullet(bulletDamage);
    //         Debug.Log("OnPlayerGetBullet");
    //     }
    // }

    public void InitBullet(long playerAccountId, float speed, int damage)
    {
        PlayerAccountId = playerAccountId;
        bulletSpeed = speed;
        bulletDamage = damage;
    }

    public List<object> SerializeObject()
    {
        List<object> retv = new List<object>();
        retv.Add(PlayerAccountId);
        retv.Add(bulletDamage);

        return retv;
    }
}
