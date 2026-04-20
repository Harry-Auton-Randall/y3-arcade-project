using UnityEngine;

public class WaypointInfo : MonoBehaviour
{
    public GameObject[] neighbours;
    //public float[] neighbourDists;
    //public WaypointInfo[] neighbourInfos;

    void Awake()
    {
        //neighbourDists = new float[neighbours.Length];
        //neighbourInfos = new WaypointInfo[neighbours.Length];

        for (int i=0;i<neighbours.Length;i++)
        {
            //neighbourDists[i] = Vector3.Distance(this.transform.position, neighbours[i].transform.position);
            //neighbourInfos[i] = neighbours[i].GetComponent<WaypointInfo>();
            Debug.DrawLine(this.transform.position + (Vector3.up * 6), neighbours[i].transform.position + Vector3.up, Color.yellow, Mathf.Infinity);
        }
    }
}
