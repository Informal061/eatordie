using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float speed;
    public Transform[] moveSpots;
    private int randomSpot;
    private float waitTime;
    public float startWaitTime;
    public float direction;
    private EnemySpawner enemySpawner;
    private GameHandler gameHandler;
    public bool isScriptActive = true;
    private AudioSource audioSource;
    private SpriteRenderer sprite;
    private Color newColor;
    public bool MainMenu = false;
    

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        sprite = GetComponent<SpriteRenderer>();
        gameObject.GetComponent<EnemyAI>().enabled = true;
        enemySpawner = GameObject.Find("SpawnEnemy").GetComponent<EnemySpawner>();
        gameHandler = GameObject.Find("GameHandler").GetComponent<GameHandler>();
        
        
    }
    void Start()
    {
        
        waitTime = startWaitTime;
        randomSpot = Random.Range(0, moveSpots.Length);
    }

    
    void Update()
    {
       

        if (isScriptActive) {

        if(!MainMenu)
            {
                sprite.color = newColor;
            }
        

        transform.position = Vector2.MoveTowards(transform.position, moveSpots[randomSpot].position, speed * Time.deltaTime);
        

        if (Vector2.Distance(transform.position, moveSpots[randomSpot].position) < 0.2f)
        {
            if(waitTime <= 0)
            {
                randomSpot = Random.Range(0, moveSpots.Length);
                waitTime = startWaitTime;
            }else
            {
                waitTime -= Time.deltaTime;
            }
        }
       if(moveSpots[randomSpot].position.x < transform.position.x)
        {
            Flip(direction);

        } else if(moveSpots[randomSpot].position.x > transform.position.x) {

            Flip(-direction);
        }

       if(!MainMenu) { 
       if(transform.localScale.z < GameObject.FindGameObjectWithTag("FishPlayer").transform.localScale.z)
            {
                
                newColor = new Color(255f/255f, 255f/255f, 255f/255f, 255/255f);
                
            } else
            {
                
                newColor = new Color(128f / 255f, 127f / 255f, 127f / 255f, 255f / 255f);
                
            }


       }
        }

    }

    private void Flip(float direction)
    {
        Vector3 scale = transform.localScale;
        scale.x = direction;
        transform.localScale = scale;

    }

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isScriptActive)
        {
            if (collision.gameObject.tag == "EnemyFish")
            {
                randomSpot = Random.Range(0, moveSpots.Length);


            }
        }
     

        

        if (collision.gameObject.tag == ("FishPlayer") && transform.localScale.z < collision.gameObject.transform.localScale.z)
        {
            isScriptActive = false;
            
            audioSource.Play();
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            gameObject.GetComponent<Rigidbody2D>().gravityScale = 0.1f;
            gameObject.GetComponent<Animator>().enabled = false;
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            gameHandler.fishCounter += 1;
            Destroy(gameObject, 1f);


        }

    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isScriptActive)
        {
            if (collision.gameObject.tag == "EnemyFish")
            {
                randomSpot = Random.Range(0, moveSpots.Length);


            }

            if (collision.gameObject.tag == "MapBorders")
            {
                randomSpot = Random.Range(0, moveSpots.Length);

            }
        }

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Urchin")
        {

            Destroy(gameObject);

        }
    }




}














