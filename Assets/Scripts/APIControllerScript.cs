using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
public class APIControllerScript : MonoBehaviour

{
    static int currentProcession = 0;
    public static APIControllerScript instance;
    
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

    }
    private void OnApplicationQuit()
    {
      //do
    }


    #region API_Calls

    /// <summary>
    /// [Template]Performs get operations.
    /// </summary>
    /// <returns>JSONNode containing response. Check ["success"] for bool success. Then perform rest of operations.</returns>
    public IEnumerator GET(string actionurl, System.Action<JSONNode> callback, string actionDescription="performing action")
    {
        Debug.Log("GetFunction Called");
        using (UnityWebRequest uwr = UnityWebRequest.Get(actionurl))
        {
            yield return uwr.SendWebRequest();
            if (!string.IsNullOrEmpty(uwr.error) || uwr.responseCode != 200)
            {
                Debug.Log("Error while "+ actionDescription + "erc" + uwr.responseCode);
                JSONNode ret = new JSONObject();
                ret.Add("success", false);
                ret.Add("message", uwr.error);
                callback(ret);
                yield break;
            }
            else
            {
                Debug.Log(actionDescription + " successful\n"+uwr.downloadHandler.text);
                JSONNode ret = new JSONObject();
                ret.Add("response", JSON.Parse(uwr.downloadHandler.text));
                ret.Add("success", true);
                callback.Invoke(ret);
            }
        }
    }


    /// <summary>
    /// [Template]Performs get operations for Images/textures.
    /// </summary>
    /// <returns>JSONNode containing response. Check ["success"] for bool success. Then perform rest of operations.</returns>
    public IEnumerator GET_Texture(string actionurl, System.Action<Texture2D> callback, string actionDescription = "performing action")
    {
        Debug.Log("GetTexture Called");
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(actionurl))
        {
            yield return uwr.SendWebRequest();
            if (!string.IsNullOrEmpty(uwr.error) || uwr.responseCode != 200)
            {
                //Debug.Log("Error while " + actionDescription + "erc" + uwr.responseCode);
                callback(null);
                yield break;
            }
            else
            {
                //Debug.Log(actionDescription + " successful\n");
                var ReturnedTexture = DownloadHandlerTexture.GetContent(uwr);
                callback.Invoke(ReturnedTexture);
            }
        }
    }
    #endregion
}

#region templates
//public void get()
//{
//    string url = API_URL + _GET_;
//    StartCoroutine(GET(url, response =>
//    {
//        if (response["success"])
//        {
            
//        }
//        else
//        {

//        }
//    }, "getting shit"));
//}
#endregion
