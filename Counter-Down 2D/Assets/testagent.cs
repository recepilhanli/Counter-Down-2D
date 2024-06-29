using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class testagent : MonoBehaviour
{

    public NavMeshAgent agent;
    public Transform totransform;

    void Update()
    {
        agent.SetDestination(totransform.position);
    }


    void SetRandomPosition()
    {
        Vector3 randomPosition = new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
        agent.SetDestination(randomPosition);
    }
}
