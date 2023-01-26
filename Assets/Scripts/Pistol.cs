using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : MonoBehaviour
{
    Transform camPos;
    [SerializeField] float damage;
    [SerializeField] float reloadTime;
    float timeBeforeShoot;
    [SerializeField] float fireRate;
    [SerializeField] int magazineCount;
    [SerializeField] int bulletsLeft;

    [SerializeField] bool isInterrupted;
    [SerializeField] bool canShoot;
    void Start()
    {
        bulletsLeft = magazineCount;
        camPos = Camera.main.transform;
        timeBeforeShoot = fireRate;
    }

    // Update is called once per frame
    void Update()
    {
        timeBeforeShoot -= Time.deltaTime;
        canShoot = timeBeforeShoot <= 0;
        if (!isInterrupted)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && bulletsLeft > 0 && canShoot)
            {
                RaycastHit hit;
                if (Physics.Raycast(camPos.position, camPos.forward, out hit, Mathf.Infinity))
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.GetComponent<Enemy>())
                        {
                            hit.collider.GetComponent<Enemy>().TakeDamage(damage);
                        }
                    }
                }
                bulletsLeft--;
                if (bulletsLeft <= 0)
                    StartCoroutine(Reload());
                timeBeforeShoot = fireRate;
            }
            if (Input.GetKeyDown(KeyCode.R) && bulletsLeft != magazineCount)
                StartCoroutine(Reload());
        }
        
    }

    IEnumerator Reload()
    {
        isInterrupted = true;
        yield return new WaitForSeconds(reloadTime);
        bulletsLeft = magazineCount;
        isInterrupted = false;
        yield return null;
    }
}


