using System.Collections;
using System.Collections.Generic;
using GameAssets.Scripts.Player;
using GameAssets.Scripts.server;
using UnityEngine;

public class PlayerCreator : MonoBehaviour
{
    [SerializeField]
    private GameObject player;

    [SerializeField] private GeometryDashServerHandler server;
    private List<GameObject> players;
    public void createPlayers(int n)
    {
        players = new List<GameObject>(n);
        players.Add(player);
        for (int i = 0; i < n - 1; i++)
        {
            GameObject instance = Instantiate(player, player.transform.position - Vector3.left * (i + 1) * 1.5f, player.transform.rotation);
            instance.GetComponent<PlayerController>().initCopy();
            // Set a unique ID for each instance
            players.Add(instance);
        }

        List<bool> actions = new List<bool>();
        for (int i = 0; i < n; i++)
        {
            actions.Add(false);
        }
        
        doActions(actions);
        
    }
    

    public void doActions(List<bool> actions)
    { 
        List<List<float>> inputs = new List<List<float>>();
        List<float> fit = new List<float>();
        for (int i = 0; i < actions.Count; i++)
        {
            fit.Add(0);
        }
        //Debug.Log("actions count:");
        //Debug.Log(actions.Count);
        var alldie = true;
        for (int i = 0; i < players.Count; i++)
        {
            
            var controller = players[i].GetComponent<PlayerController>();
            alldie &= controller.die;
            var inp = controller.Do(actions[i]);
            inputs.Add( inp);
            fit[i] = controller.fitness;
        }
        //Debug.Log(alldie);
        Inputs toServer = new Inputs();
        Fitnesses f = new Fitnesses();
        f.fitnesses = fit;
        toServer.input = inputs;
        if (!alldie) StartCoroutine(server.GetActions(toServer));
        else StartCoroutine(server.SendFitness(f));
    }
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
