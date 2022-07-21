using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SimpleJSON;
public class SubdomainScript : MonoBehaviour
{
    //This is a template for subdomains.
    //Insert a HallTemplate(disabled) as the first child and an empty gameobject to hold all halls as the second child.

    GameObject HallTemplate;
    Transform HallsParentTransform;
    List<Hall> Halls;

    [Tooltip("Max distance upto which a canvas is visible. Lower it to improve performance.")]
    public int canvasTurnoffDistance = 30;

    [Tooltip("Base url of the subdomain. Eg. \nhttps://buysellnft.cryptoshoppingstore.com")]
    public string API_URL = @"https://buysellnft.cryptoshoppingstore.com";

    [Tooltip("path to get all categories. Eg. \n/wp-admin/admin-ajax.php?action=getCategories")]
    public string _GET_CATEGORIES = @"/wp-admin/admin-ajax.php?action=getCategories";

    [Tooltip("Template to get Categoriy data. Make sure to replace the category name with \"___CATEGORY___\" Eg. \n/wp-admin/admin-ajax.php?action=getProducts&category_name=___CATEGORY___")]
    public string _GET_PRODUCTS_CATEGORY = @"/wp-admin/admin-ajax.php?action=getProducts&category_name=___CATEGORY___";

    [Tooltip("Wait for this number of seconds between calls made for categories. Decrease this if server speed improves.")]
    public float CategoryWaitTime = 3;


    bool[,] _filledPositions;
    static int _SubDomainCount=0;

    //These multipliers place each subdomain in it's own quadrant.
    int _multiplierX, _multiplierZ;
    private void Awake()
    {
        Halls = new List<Hall>();
        _SubDomainCount++;
        if (_SubDomainCount == 1)
        {
            _multiplierX = 1;
            _multiplierZ = 1;
        }
        else if (_SubDomainCount == 2)
        {
            _multiplierX = -1;
            _multiplierZ = -1;
        }
        else if (_SubDomainCount == 3)
        {
            _multiplierX = -1;
            _multiplierZ = 1;
        }
        else if (_SubDomainCount == 4)
        {
            _multiplierX = 1;
            _multiplierZ = -1;
        }
        else Destroy(gameObject);
        HallTemplate = transform.GetChild(0).gameObject;
        HallsParentTransform = transform.GetChild(1);
        _filledPositions = new bool[100, 100];
    }
    private void Start()
    {
        GetAllCategoriesForSubdomain();
    }

    #region create rooms for subdomain and populate them   
    /// <summary>
    /// gets all categories in the subdomain.
    /// </summary>
    public void GetAllCategoriesForSubdomain()
    {
        string url = API_URL + _GET_CATEGORIES;
        StartCoroutine(APIControllerScript.instance.GET(url, response => {
            Debug.Log(response["success"],gameObject);
            if (response["success"])
            {
                StartCoroutine(ProcessAllCategories(response["response"].AsArray));
            }
            else
            {
                Debug.Log("Failed getting categories", gameObject);
            }

        }, "getting categories"));
    }

    /// <summary>
    /// Starts processing of the categories based on the data received from the server.
    /// </summary>
    /// <param name="categories"></param>
    /// <returns></returns>
    private IEnumerator ProcessAllCategories(JSONArray categories)
    {

        for (int i = 0; i < categories.Count; i++)
        {
            string current_category = categories[i]["slug"];
            
            ProcessCategory(current_category);
            yield return new WaitForSeconds(CategoryWaitTime);
        }
    }
    /// <summary>
    /// Gets a position for the hall. Use this function in the production environment to get a position for hall based on the NFT's map.
    /// </summary>
    /// <returns></returns>
    private Vector3 getHallPosition()
    {
        Vector2 v = new Vector2(100, 100);
        for (int i = 0; i < 99; i++)
        {
            if (_filledPositions[i + 1, 0] == true)
            { }
            else
            {
                for (int j = 0; j < i; j++)
                {
                    if (_filledPositions[i - j, j] == true)
                    {
                    }
                    else
                    {
                        _filledPositions[i - j, j] = true;
                        v = new Vector2(i - j + 1, j + 1);
                        return new Vector3(HallTemplate.transform.position.x + (_multiplierX * v.x * 20), 10, HallTemplate.transform.position.z + (_multiplierZ * v.y * 15));
                    }
                }
                _filledPositions[i + 1, 0] = true;
                v = new Vector2(i + 2, 1);
                break;
            }
        }
        var x = HallTemplate.transform.position.x + (_multiplierX * v.x * 20);
        var y = 10;
        var z = HallTemplate.transform.position.z + (_multiplierZ * v.y * 15);
        return new Vector3(x, y, z);

    }
    
    private void ProcessCategory(string CategorySlug)
    {
        string uri = API_URL + _GET_PRODUCTS_CATEGORY;
        uri = uri.Replace("___CATEGORY___", CategorySlug);
        StartCoroutine(APIControllerScript.instance.GET(uri, response => {
            if (response["success"])
            {
                var newRoom = Instantiate(HallTemplate, getHallPosition(), HallTemplate.transform.rotation, HallsParentTransform);
                newRoom.gameObject.SetActive(true);
                newRoom.transform.GetChild(0).GetComponent<TMP_Text>().text = CategorySlug.Replace("-", " ");
                createNewHall(CategorySlug, response["response"], newRoom);
            }
            else
            {
                Debug.Log("did not get " + CategorySlug + " data \n");
            }
        }, "getting " + CategorySlug + " data"));
    }

    public void createNewHall(string HallName, JSONNode data, GameObject hallGameObject)
    {
        Hall currentHall = new Hall(HallName, data, hallGameObject);
        Halls.Add(currentHall);
        populateHall(currentHall);
    }
    void populateHall(Hall thisHall)
    {
        Transform canvasTransform = thisHall._Canvas.transform;
        for (int i = 0; i < thisHall._Data.Count; i++)
        {
            if (i < canvasTransform.childCount)
            {
                var currentChild = thisHall._Data[i];
                RawImage currentImage = canvasTransform.GetChild(i).GetComponent<RawImage>();
                currentImage.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OnProductButtonClicked(currentChild);
                });
                if(currentChild.HasKey("price"))
                    currentImage.transform.GetChild(0).GetComponent<TMP_Text>().text = "$" + currentChild["price"];
                StartCoroutine(APIControllerScript.instance.GET_Texture(currentChild["image_url"], tex =>
                    {
                        currentImage.texture = tex;
                    }, "Getting image for a product"));
            }
            else break;
        }

    }
    #endregion
    private void Update()
    {
        disableDistantCanvasAndRotateHeaders();
    }

    /// <summary>
    /// Disables all canvases farther than canvasTurnoffDistance to save performance And Rotates header by a bit.
    /// </summary> 
    void disableDistantCanvasAndRotateHeaders()
    {
        Hall HallClosestToPlayer=null;
        float ClosestDistance = float.PositiveInfinity;
        Vector3 PlayerPosition = PlayerScript.instance.gameObject.transform.position;
        foreach (Hall hall in Halls)
        {
            float Dist = Vector3.Distance(hall._GameObject.transform.position, PlayerPosition);
            if (Dist > canvasTurnoffDistance) hall._Canvas.gameObject.SetActive(false);
            else
            {
                if (Dist < ClosestDistance)
                {
                    HallClosestToPlayer = hall;
                    ClosestDistance = Dist;
                }
                hall._Canvas.gameObject.SetActive(true);
            }
            //hall._CategoryNameText.transform.Rotate(0, 6 * Time.deltaTime, 0);

            
        }

        if (HallClosestToPlayer != null)
        {
            CanvasInteractionScript.instance.AssignClosestCanvasForRaycast(HallClosestToPlayer);
        }
    }

    void OnProductButtonClicked(JSONNode Product)
    {
        Debug.Log("Creating product page for "+ Product[""]);
        CanvasInteractionScript.instance.createProductPage(Product);
    }
}

public class Hall
{
    public JSONNode _Data;
    public TMP_Text _CategoryNameText;
    public GameObject _GameObject;
    public string _Name;
    public Canvas _Canvas;
    public Hall(string name, JSONNode data, GameObject gameObject)
    {
        this._Name = name;
        this._GameObject = gameObject;
        this._Data = data;
        if (data.HasKey("data"))
            this._Data = data["data"];
        this._CategoryNameText = gameObject.transform.GetChild(0).GetComponent<TMP_Text>();
        this._Canvas = gameObject.transform.GetChild(gameObject.transform.childCount - 1).GetComponent<Canvas>();
    }
}
