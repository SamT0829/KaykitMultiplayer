using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BowPrefab : MonoBehaviour
{
    [SerializeField] private BulletPrefab gameArrowPrefab;
    [SerializeField] private Vector3 BulletSpreadVariance = new Vector3(0.01f, 0.01f, 0.01f);
    [SerializeField] private ParticleSystem ShootingSystem;
    [SerializeField] private Transform BulletSpawnPoint;
    [SerializeField] private ParticleSystem ImpactParticleSystem;
    [SerializeField] private bool AddBulletSpread = true;
    [SerializeField] private float ShootDelay = 0.5f;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private int BulletDamage = 1;
    [SerializeField] private LayerMask Mask;
    [SerializeField] private LayerMask PlayerMask;

    [SerializeField] private GamePlayerPrefab gamePlayerPrefab;
    private float LastShootTime;

    private void Awake()
    {
        gamePlayerPrefab = GetComponentInParent<GamePlayerPrefab>();
    }
    public void Shoot(Vector3 aimForwardVector)
    {
        if (LastShootTime + ShootDelay < Time.time)
        {
            gamePlayerPrefab.Animator.SetBool("IsShooting", true);
            ShootingSystem.Play();

            FireBullet(aimForwardVector, null);
            LastShootTime = Time.time;
        }
    }
    public BulletPrefab FireBullet(Vector3 aimForwardVector, BulletInfo bulletInfo)
    {
        var gameArrow = Instantiate(gameArrowPrefab, BulletSpawnPoint.position + aimForwardVector * 1, Quaternion.LookRotation(aimForwardVector));
        gameArrow.InitBullet(gamePlayerPrefab.GamePlayerRoomInfo.AccountId, bulletSpeed, BulletDamage);
        StartCoroutine(SpawnTrail(gameArrow, aimForwardVector));
        return gameArrow;
    }
    public Vector3 GetDirection(Vector3 aimForwardVector)
    {
        Vector3 direction = aimForwardVector;
        direction.Normalize();

        if (AddBulletSpread)
        {
            direction += new Vector3(
                Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
                Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
                Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
            );

            direction.Normalize();
        }

        return direction;
    }
    private IEnumerator SpawnTrail(BulletPrefab gameBullet, Vector3 direction)
    {
        var trail = gameBullet.GetComponent<TrailRenderer>();

        float time = 0;
        float hitDistance = 1000;
        Vector3 startPosition = trail.transform.position;
        RaycastHit hit;

        if (Physics.Raycast(BulletSpawnPoint.position, direction, out hit, hitDistance, PlayerMask))
        {
            Debug.DrawLine(transform.position, hit.point, Color.blue);
            while (Vector3.Distance(trail.transform.position, startPosition) <= Vector3.Distance(hit.transform.position, startPosition))
            {
                time += Time.deltaTime / trail.time;
                trail.transform.position += trail.transform.forward * time * bulletSpeed;
                yield return new WaitForFixedUpdate();
            }
            trail.transform.position = hit.transform.position;
            Instantiate(ImpactParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));

            var gamePlayer = hit.collider.GetComponent<GamePlayerPrefab>();
            if (gamePlayer)
            {
                if (gamePlayerPrefab.GamePlayerRoomInfo.Team != gamePlayer.GamePlayerRoomInfo.Team || gamePlayer.GamePlayerRoomInfo.Team == Team.None)
                    gamePlayer?.OnPlayerGetBullet(gameBullet);
            }
            else
            {
                Debug.Log("CantFind gamePlayer");
            }

        }
        else if (Physics.Raycast(BulletSpawnPoint.position, direction, out hit, hitDistance, Mask))
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);
            while (Vector3.Distance(trail.transform.position, startPosition) <= Vector3.Distance(hit.transform.position, startPosition))
            {
                time += Time.deltaTime / trail.time;
                trail.transform.position += trail.transform.forward * time * bulletSpeed;
                yield return new WaitForFixedUpdate();
            }

            trail.transform.position = hit.transform.position;
            Instantiate(ImpactParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));
        }
        else
        {
            Debug.DrawLine(transform.position, trail.transform.forward * hitDistance, Color.black);

            while (Vector3.Distance(trail.transform.position, startPosition) <= hitDistance)
            {
                time += Time.deltaTime / trail.time;
                trail.transform.position += trail.transform.forward * time * bulletSpeed;
                yield return new WaitForFixedUpdate();
            }

            trail.transform.position = trail.transform.forward * hitDistance;
        }

        gamePlayerPrefab.Animator.SetBool("IsShooting", false);
        Destroy(trail.gameObject, trail.time);
    }
}
