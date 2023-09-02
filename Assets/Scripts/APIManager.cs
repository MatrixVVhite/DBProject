using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class APIManager : MonoBehaviour
{
    const string API_URL = "https://localhost:7004/api/";

    void Start()
    {
        
    }
    
    

    IEnumerator GetQuestion(int id)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL + "Questions/" + id.ToString()))
        {
            yield return request.SendWebRequest();
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    Debug.Log(request.downloadHandler.text);
                    break;
                default:
                    Debug.Log(request.result); 
                    break;
            }
        }
    }

    public void GetPlayerName(int id)
    {
        StartCoroutine(GetPlayerNameCor(id));
    }

    IEnumerator GetPlayerNameCor(int id)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL + "Questions/" + id.ToString()))
        {
            yield return request.SendWebRequest();
            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    Debug.Log(request.downloadHandler.text);
                    break;
                default:
                    Debug.Log(request.result);
                    break;
            }
        }
    }
}