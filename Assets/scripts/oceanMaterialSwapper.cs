using UnityEngine;

public class oceanMaterialSwapper : MonoBehaviour
{
    MeshRenderer mr;
    Material material;

    public Texture[] textures;

    public float timeLoop;
    public float sineVal;
    public float normalV0, normalV1, normalVAlt;

    public int normalS0, normalS1, normalM0, normalM1;

    public int currentTex;
    bool MainsTurn;

    void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        mr.material.EnableKeyword("_NORMALMAP");
        mr.material.EnableKeyword("_DETAIL_MULX2");

        normalS0 = Shader.PropertyToID("_BumpScale");
        normalS1 = Shader.PropertyToID("_DetailNormalMapScale");
        normalM0 = Shader.PropertyToID("_BumpMap");
        normalM1 = Shader.PropertyToID("_DetailNormalMap");

        timeLoop = 0;
        sineVal = 0;

        currentTex = 0;
    }

    void PickTexture()
    {
        currentTex = currentTex + 1;
        if (currentTex > 7)
        {
            currentTex = 0;
        }
    }

    void Update()
    {
        //make wave wave based off time
        timeLoop = timeLoop + (Time.deltaTime / 5);
        while (timeLoop >= 1)
        {
            timeLoop = timeLoop - 1;
        }

        //sineVal = (Mathf.Cos(360 * Mathf.Deg2Rad * timeLoop) / 2) + 0.5f; //sine wave
        sineVal = (2 * Mathf.Abs(timeLoop - 0.5f)); //triangle wave

        //calculate normal scale value for each map
        //VAlt gives a slight boost to scale as it gets closer to 0.5

        //normalVAlt = 0.1f * (1 - (2 * Mathf.Abs(sineVal - 0.5f))); //triangle wave
        normalVAlt = 0.1f * Mathf.Abs(Mathf.Cos(0.5f * ((360 * Mathf.Deg2Rad * sineVal) - 180 * Mathf.Deg2Rad))); //absoluted sine wave

        normalV0 = (sineVal * 0.5f) + normalVAlt;
        normalV1 = ((1 - sineVal) * 0.5f) + normalVAlt;

        //-----------------------

        material = mr.material;

        //change main/secondary normal map values
        material.SetFloat(normalS0, normalV0);
        material.SetFloat(normalS1, normalV1);

        //changes whichever normal map is lower once it reaches 0 scale
        if(MainsTurn == true && timeLoop >= 0.5f)
        {
            PickTexture();
            MainsTurn = false;
            material.SetTexture(normalM0, textures[currentTex]);

        }
        if (MainsTurn == false && timeLoop < 0.5f)
        {
            PickTexture();
            MainsTurn = true;
            material.SetTexture(normalM1, textures[currentTex]);

        }

        mr.material = material;

    }
}
