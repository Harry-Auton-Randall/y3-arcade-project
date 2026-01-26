using UnityEngine;

public class cutterStatSet : MonoBehaviour
{
    public float speed = 8.0f;
    public float rotate = 90.0f;
    public int maxHealth = 20;

    boatMove bm;

    void Awake()
    {
        bm = GetComponent<boatMove>();
    }

    void Start()
    {
        bm.SetStats(speed, rotate);
    }
}
