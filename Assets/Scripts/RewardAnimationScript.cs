using UnityEngine;
using System.Collections;

public class RewardAnimationScript : MonoBehaviour {

    private int rewardIndex;
    private SpriteRenderer spriteRenderer;
    public bool isAnimated = false;
    public bool isRotating = false;
    public bool isFloating = false;
    public bool isScaling = false;

    public bool scaleUpOnly = false;

    public Vector3 rotationAngle;
    public float rotationSpeed;

    public float floatSpeed;
    private bool goingUp = true;
    public float floatRate;
    private float floatTimer;
   
    public Vector3 startScale;
    public Vector3 endScale;

    private bool scalingUp = true;
    public float scaleSpeed;
    public float scaleRate;
    private float scaleTimer;

	// Use this for initialization
	void Start () 
    {
        rewardIndex = GetComponent<SpawnRewardLocation>().rewardIndex;
        spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {

        //scaleUpOnly = GameController.control.scaleUpReward[rewardIndex];
        isScaling = GameController.control.scaleUpReward[rewardIndex];


        if (isAnimated)
        {
            if(isRotating)
            {
                transform.Rotate(rotationAngle * rotationSpeed * Time.deltaTime);
            }

            if(isFloating)
            {
                floatTimer += Time.deltaTime;
                Vector3 moveDir = new Vector3(0.0f, 0.0f, floatSpeed);
                transform.Translate(moveDir);

                if (goingUp && floatTimer >= floatRate)
                {
                    goingUp = false;
                    floatTimer = 0;
                    floatSpeed = -floatSpeed;
                }

                else if(!goingUp && floatTimer >= floatRate)
                {
                    goingUp = true;
                    floatTimer = 0;
                    floatSpeed = +floatSpeed;
                }
            }

            if(isScaling)
            {
                scaleTimer += Time.deltaTime;
                spriteRenderer.sortingLayerName = "ExplodingReward";

                if (scalingUp)
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, endScale, scaleSpeed * Time.deltaTime);
                }
                else if (!scalingUp)
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, startScale, scaleSpeed * Time.deltaTime);
                }

                if(scaleTimer >= scaleRate)
                {
                    if (scalingUp) { scalingUp = false; }
                    else if (!scalingUp) { scalingUp = true; }
                    scaleTimer = 0;
                }
            }
            /*
            if (scaleUpOnly)
            {
                // move the reward to the front layer
                spriteRenderer.sortingLayerName = "ExplodingReward";
                Debug.Log("sprite should be scaling up now");

                // make the reward scale up to be big on the canvas
                scaleTimer += Time.deltaTime;

                transform.localScale = Vector3.Lerp(transform.localScale, endScale, scaleSpeed * Time.deltaTime);
            }
            */           
        }
	}
}
