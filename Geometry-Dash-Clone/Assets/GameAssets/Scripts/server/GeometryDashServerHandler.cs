using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameAssets.Scripts.server;
using GameAssets.Scripts.Utils;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class GeometryDashServerHandler : MonoBehaviour
{
    private const string GetActionURL = "http://127.0.0.1:8000/getActions";
    private const string GetPopulationSizeURL = "http://127.0.0.1:8000/populationSize";
    private const string SendFitnessesURL = "http://127.0.0.1:8000/sendFitnesses";
    public int iteration;
    public int populationSize;
    public List<float> fitnesses;
    public bool actionReady  ;
    public List<bool> actions;
    [SerializeField]
    private PlayerCreator creator;


    private void Start()
    {
        StartCoroutine(GetPopulationSize());
    }

    public IEnumerator GetPopulationSize()
    {
        
        UnityWebRequest request = UnityWebRequest.Get(GetPopulationSizeURL);
        
        yield return  request.SendWebRequest();
    
        PopulationSize fromServer = JsonUtility.FromJson<PopulationSize>(request.downloadHandler.text);
        populationSize = fromServer.n;
        creator.createPlayers(populationSize);
    }
    
    public IEnumerator GetActions(Inputs inputs)
    {
        WWWForm wwwForm = new WWWForm();
        
        UnityWebRequest request = UnityWebRequest.Post(GetActionURL, wwwForm);

        var body = JsonConvert.SerializeObject(inputs);
        byte[] rawMessage = Encoding.UTF8.GetBytes(body);
        UploadHandler uploadHandler = new UploadHandlerRaw(rawMessage);
        request.uploadHandler = uploadHandler;
        request.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
        yield return request.SendWebRequest();
        //Debug.Log(request.downloadHandler.text);
        Actions fromServer = JsonConvert.DeserializeObject<Actions>(request.downloadHandler.text);
        this.actions = fromServer.needJumping;
        creator.doActions(this.actions);
    }

    public IEnumerator SendFitness(Fitnesses fitnesses)
    {
        WWWForm wwwForm = new WWWForm();

        UnityWebRequest request = UnityWebRequest.Post(SendFitnessesURL, wwwForm);
        var body = JsonUtility.ToJson(fitnesses);
        byte[] rawMessage = Encoding.UTF8.GetBytes(body);
        UploadHandler uploadHandler = new UploadHandlerRaw(rawMessage);
        request.uploadHandler = uploadHandler;
        request.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
        yield return request.SendWebRequest();
        SceneLoader.ReloadScene();
    }
}
