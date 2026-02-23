using UnityEngine;

public class oceanAnimator : MonoBehaviour
{
    MeshRenderer mr;
    Material material;

    public Texture[] textures; //array of normal textures

    float timeLoop, sineVal; //for making the math wave
    float normalV0, normalV1, normalVAlt, valtMult; //the values the normals move by
    float waveHeight, waveRate; //tweakable multipliers

    int normalS0, normalS1, normalM0, normalM1; //IDs

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

        valtMult = 0.23f;
        waveHeight = 0.4f;
        waveRate = 0.4f;

        currentTex = 0;
    }

    void PickTexture()
    {
        currentTex = currentTex + 1;
        if (currentTex > textures.Length - 1)
        {
            currentTex = 0;
        }
    }

    void Update()
    {
        //make math wave based off time
        timeLoop = timeLoop + (Time.deltaTime * waveRate * 0.5f);
        while (timeLoop >= 1)
        {
            timeLoop = timeLoop - 1;
        }

        //sineVal = (Mathf.Cos(360 * Mathf.Deg2Rad * timeLoop) / 2) + 0.5f; //sine wave
        sineVal = (2 * Mathf.Abs(timeLoop - 0.5f)); //triangle wave

        //calculate normal scale value for each map
        //VAlt gives a slight boost to scale as it gets closer to 0.5

        //normalVAlt = valtMult * waveHeight * (1 - (2 * Mathf.Abs(sineVal - 0.5f))); //triangle wave
        normalVAlt = valtMult * waveHeight * Mathf.Abs(Mathf.Cos(0.5f * ((360 * Mathf.Deg2Rad * sineVal) - 180 * Mathf.Deg2Rad))); //absoluted sine wave

        normalV0 = (sineVal * waveHeight) + normalVAlt;
        normalV1 = ((1 - sineVal) * waveHeight) + normalVAlt;

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
