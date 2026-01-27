using UnityEngine;

public class boatCombatCutter : MonoBehaviour
{
    public float speed = 8.0f;
    public float rotate = 90.0f;
    public int maxHealth = 20;
    public float reload = 3.0f;
    public float reloadSpecial = 15.0f;

    boatMove bm;

    void Awake()
    {
        bm = GetComponent<boatMove>();
    }

    //speed and rotate initialised in this script and written to boatMove, so each ship can use the same boatMove script
    void Start()
    {
        bm.SetStats(speed, rotate);
    }
}
