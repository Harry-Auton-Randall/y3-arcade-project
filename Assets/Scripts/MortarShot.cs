using UnityEngine;

public class MortarShot : MonoBehaviour
{
    int spawnerID = -1;

    int totalDam = 15;
    float time = -1.25f;
    float damDelay = 0.2f;
    int damSoFar = 0;
    Material[] materials;
    Renderer rend;

    float radius = 8;
    Collider[] nearbyDamagers;
    LayerMask damagerMask;
    int nearbyDamagerCount;

    void Awake()
    {
        rend = transform.Find("dangerCircle16").GetComponent<Renderer>();
        damagerMask = (1 << LayerMask.NameToLayer("boat"));
    }
    public void Init(int idIn, bool friendly)
    {
        spawnerID = idIn;

        this.materials = rend.materials;
        materials[0] = Resources.Load("SolidMaterials/red40", typeof(Material)) as Material;
        if (friendly)
        {
            materials[1] = Resources.Load("SolidMaterials/whiteSolid", typeof(Material)) as Material;
        }
        else
        {
            materials[1] = Resources.Load("SolidMaterials/redSolid", typeof(Material)) as Material;
        }
        rend.materials = this.materials;
    }

    void Start()
    {
        nearbyDamagers = new Collider[GameObject.Find("/RoundManager").GetComponent<RoundManager>().totalShips + 10]; //+10 temporary
    }
    void Update()
    {
        time += Time.deltaTime;

        for (int i=damSoFar;i<totalDam;i++)
        {
            if (time > (damDelay * damSoFar))
            {
                damSoFar = i + 1;
                DealDamage();

                if (damSoFar == totalDam)
                {
                    Destroy(this.gameObject);
                }
            }
            else
            {
                break;
            }
        }
    }

    void DealDamage()
    {
        for (int i=0;i<nearbyDamagers.Length;i++)
        {
            nearbyDamagers[i] = null;
        }
        nearbyDamagerCount = Physics.OverlapSphereNonAlloc(this.transform.position, radius, nearbyDamagers, damagerMask);

        for (int i = 0; i < nearbyDamagerCount; i++)
        {
            nearbyDamagers[i].transform.parent.GetComponent<boatCombat>().TakeDamage(1, false, false, spawnerID);
        }
    }
}
