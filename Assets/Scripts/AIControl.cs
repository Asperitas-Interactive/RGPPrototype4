using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
public class AIControl : MonoBehaviour
{
    public GameObject dest;
    public NavMeshAgent agent;
    public AudioDetection audioDetection;
    public ViewDetection viewDetection;
    Vector3 defaultPos;
    Quaternion defaultRot;
    public float patrolDistance= 2.0f;
    public bool death = false;
    public float alarmTimer = 2.0f;
    public enum State
    {
        follow,
        patrol,
        idle,
    };

    public State state = State.patrol;
    public Vector3 followDir;
    float followTimer;
    public float maxFollowTimer;

    float idleTimer;
    bool sw = false;
    public float[] maxIdleTimer = { 3.0f, 5.0f };
    bool rotate = false;

    float watchTimer = 5.0f;
    bool quit;

    public DialougeP1Control dialouge;

    // Start is called before the first frame update
    void Start()
    {
           
        defaultPos = new Vector3();
        defaultRot = new Quaternion();
        defaultRot = transform.rotation;
        defaultPos = transform.position;
        agent = gameObject.GetComponent<NavMeshAgent>();
        agent.SetDestination((transform.forward * patrolDistance + transform.position));
    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.tag == "Dead")
        {
            agent.enabled = false;
            transform.GetChild(2).GetComponent<Animator>().SetBool("death", true);
            return;
        }

        watchTimer -= Time.deltaTime;

        if (quit && watchTimer < 0.0f)
            SceneManager.LoadScene(2);

        followTimer -= Time.deltaTime;
        idleTimer -= Time.deltaTime;

        if(state == State.follow && followTimer < 0.0f && sw)
        {
            state = State.patrol;
            agent.SetDestination(defaultPos);
            sw = false;
        }


        if (state == State.idle && idleTimer < 0.0f && sw)
        {
            sw = true;
            state = State.patrol;
           // agent.SetDestination(defaultPos);

        }

        for (int i = 0;  i < audioDetection.HeardObjects.Count; i++)
        {
            if(audioDetection.HeardObjects[i].tag == "Player")
            {
               // dest = audioDetection.HeardObjects[i];
            }
        }

        for(int i = 0; i < viewDetection.SightedObjects.Count; i++)
        {
            if (audioDetection.HeardObjects[i].tag == "Player")
            {
                Debug.Log("On sight");
            }
        }

        if (dest != null)
        {
        //    agent.SetDestination(dest.transform.position);
        }


        if (state == State.patrol)
        {
            if (agent.remainingDistance < 0.1f)
            {
                if (rotate)
                {
                    agent.transform.rotation = defaultRot;
                    rotate = false;
                }
                agent.transform.Rotate(0f, 180f, 0f);
                agent.SetDestination((transform.forward * patrolDistance + transform.position));
                state = State.idle;
                sw = true;
                idleTimer = Random.Range(maxIdleTimer[0], maxIdleTimer[1]);
            }
        }


        else if (state == State.follow)
        {
            agent.SetDestination(dest.transform.GetChild(0).position);
        }

        if(state == State.idle)
        {
            agent.velocity = Vector3.zero;
        }

        transform.GetChild(2).GetComponent<Animator>().SetFloat("speed", agent.velocity.magnitude);
    }


    private void OnTriggerEnter(Collider other)
    {




    }

    public void collision(Collider other)
    {

        if (gameObject.tag == "Dead")
        {
            return;
        }
        if (other.gameObject.tag == "Player")
        {
            SceneManager.LoadScene(2);
        }

        if (other.CompareTag("Diversion"))
        {
            state = State.follow;
            sw = true;
            followDir = other.transform.position;
            followTimer = maxFollowTimer;
            dialouge.AssignText(5);
        }

        if (other.gameObject.tag == "Dead")
        {
            quit = true;
            watchTimer = alarmTimer;
        }
    }
}
