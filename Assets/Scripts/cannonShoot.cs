using UnityEngine;

public class cannonShoot : MonoBehaviour
{
    Transform model;
    Vector3 modelPos = new Vector3(0,0,0);
    float recoil;
    public float backSpeed = 0.9f;
    public float forwardSpeed = 0.2f;

    public GameObject cannonball;
    GameObject instance;
    Vector3 instStartPos;
    void Awake()
    {
        model = transform.Find("model");
    }
    public void Shoot(float speedIn, float durationIn, bool friendlyIn, int typeIn)
    {
        switch (typeIn)
        {
            default:
                instance = Instantiate(cannonball);
                instance.transform.rotation = transform.rotation;
                instStartPos = transform.position;
                instStartPos.y = 0;
                instance.transform.position = instStartPos;
                instance.transform.position += transform.forward * 0.5f;
                instance.GetComponent<cannonballMove>().Init(speedIn, durationIn, transform.position.y + 0.23f, friendlyIn, typeIn);
                break;
        }
        recoil = modelPos.z - 0.1f;
    }
    void Update()
    {
        if (modelPos.z > recoil)
        {
            modelPos.z -= backSpeed * Time.deltaTime;
            if (modelPos.z <= recoil)
            {
                modelPos.z = recoil;
                recoil = 0;
            }
        }
        else if (modelPos.z != 0)
        {
            modelPos.z += forwardSpeed * Time.deltaTime;
            if (modelPos.z > recoil)
            {
                modelPos.z = recoil;
            }
        }
        model.localPosition = modelPos;
    }
}
