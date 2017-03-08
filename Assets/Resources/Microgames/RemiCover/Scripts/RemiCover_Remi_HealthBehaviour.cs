
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemiCover_Remi_HealthBehaviour : MonoBehaviour
{

    [SerializeField]
    private float HP = 1;                           // Remilia's Health Points.
    public float burnSpeed;                         // How much will HP decrease when Remilia's collider is exposed to sunlight?

    [SerializeField]
    private int collidersOutside = 0;               // How many colliders are outside of Umbrella's shadow?

    private GameObject remiliaSprite = null;
    public ParticleSystem smokeParticles;
    private ParticleSystem smokeInstance;
    private SpriteRenderer remiSpriteRenderer;

    private bool inmunnity = false;

    // Use this for initialization
    void Start()
    {
        HP = 1;
        remiliaSprite = transform.FindChild("Remilia_Sprite").gameObject;
        smokeInstance = (ParticleSystem)Instantiate(smokeParticles, remiliaSprite.transform.position, smokeParticles.transform.rotation);
        collidersOutside = 0;
        remiSpriteRenderer = remiliaSprite.GetComponent<SpriteRenderer>();
    }


    // Update is called once per frame
    void Update()
    {
        if (!MicrogameController.instance.getVictoryDetermined() && !inmunnity)
        {
            updateHP();
            if (HP <= 0) GameOver();
        }

        manageEmission();

        if (MicrogameController.instance.getVictory())
            manageSpriteColor();

    }

    private void manageSpriteColor()
    {
        //Color lerps from white to light red based on HP, then pure red like normal on failure
        changeSpriteColor(new Color(1f, Mathf.Lerp(.5f, 1f, HP), Mathf.Lerp(.5f, 1f, HP)));
    }

    private void manageEmission()
    {
        var emission = smokeInstance.emission;

        smokeInstance.transform.position = remiliaSprite.transform.position + (Vector3.up * .5f);
        smokeInstance.startSize = (((1 - HP) * 90) / 25)
            * (MicrogameController.instance.getVictory() ? 1f : 1.25f);	//Particle size intensifies on death

        emission.rateOverTime = (((1 - HP) * 2000) / 10)
            * (MicrogameController.instance.getVictory() ? 1f : 1.5f);  //Particle rate intensifies on death

    }


    /// <summary>
    /// Decrease HP value if some colliders are outside of Umbrella's Shadow. Increase HP a little if all colliders are inside.
    /// </summary>
    private void updateHP()
    {
        if (MicrogameTimer.instance.beatsLeft < .5f)
            return;
        if (collidersOutside == 0)
            this.HP = Mathf.Min(this.HP + (burnSpeed * Time.deltaTime * .65f), 1f);
        else
            this.HP -= burnSpeed * Time.deltaTime * collidersOutside;
    }

    /// <summary>
    /// End the game with a defeat.
    /// </summary>
    private void GameOver()
    {
        MicrogameController.instance.setVictory(false, true);
        GetComponent<Animator>().SetTrigger("Defeat");
    }

    /// <summary>
    /// Set the inmunnity of the character (Useful for teleport movement).
    /// </summary>
    /// <param name="inmunnity"> If character HP must decrease/increase or not</param>
    public void setInmunnity(bool inmunnity)
    {

        if (inmunnity)
        {
            smokeInstance.enableEmission = false;
        }

        else
        {
            smokeInstance.transform.position = remiliaSprite.transform.position + (Vector3.up * .5f);
            smokeInstance.enableEmission = true;
        }

        this.inmunnity = inmunnity;

    }

    /// <summary>
    /// Change Remilia's sprite color
    /// </summary>
    /// <param name="color">Color for setting</param>
    private void changeSpriteColor(Color color)
    {
        remiSpriteRenderer.color = color;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.name == "UmbrellaShadow")
        {
            collidersOutside += 1;
        }
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "UmbrellaShadow" && collidersOutside != 0)
        {
            collidersOutside -= 1;
        }
    }





}
