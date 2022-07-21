using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SimpleJSON;
using TMPro;
public class CanvasInteractionScript : MonoBehaviour
{
    public static CanvasInteractionScript instance;
    public Transform target;
    public GraphicRaycaster canvasRaycaster;
    public List<RaycastResult> list;
    public Vector2 screenPoint;
    public GameObject ProductPageTemplate, CurrentProductPage=null;
    public StarterAssets.StarterAssetsInputs ControllerInputAsset;

    private void Start()
    {
        if (CanvasInteractionScript.instance == null)
            instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        list = new List<RaycastResult>();
        //screenPoint = Camera.main.WorldToScreenPoint(target.position);
        screenPoint = new Vector2(Screen.width / 2, Screen.height / 2);

        PointerEventData ed = new PointerEventData(EventSystem.current);
        ed.position = screenPoint;
        canvasRaycaster.Raycast(ed, list);

        if (list != null && list.Count > 0)
        {
            if(list[0].gameObject.GetComponent<Button>()!=null)
            {
                list[0].gameObject.GetComponent<Button>().Select();
                if (Mouse.current.leftButton.isPressed)
                {
                    list[0].gameObject.GetComponent<Button>().onClick.Invoke();
                }
            }
                
        }

    }

    public void AssignClosestCanvasForRaycast(Hall hall)
    {
        Debug.Log("Assigning raycaster to", hall._Canvas);
        canvasRaycaster = hall._Canvas.gameObject.GetComponent<GraphicRaycaster>();
    }


    public void createProductPage(JSONNode Product)
    {
        if (CurrentProductPage == null)
        {
            Time.timeScale = 0;
            ControllerInputAsset.cursorLocked = false;
            ControllerInputAsset.cursorInputForLook = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            CurrentProductPage = Instantiate(ProductPageTemplate, transform);
            CurrentProductPage.SetActive(true);
            var currentImage = CurrentProductPage.transform.GetChild(0).GetComponent<RawImage>();
            StartCoroutine(APIControllerScript.instance.GET_Texture(Product["image_url"], tex =>
            {
                currentImage.texture = tex;
            }, "Getting image for a product"));
            if (Product.HasKey("price"))
                CurrentProductPage.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "$" + Product["price"] + "\nBUY NOW!!!";
            else CurrentProductPage.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);

            if (Product.HasKey("name"))
                CurrentProductPage.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = Product["name"];
            else
                CurrentProductPage.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);

            if (Product.HasKey("description"))
                CurrentProductPage.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = SanitizeHTMLString(Product["description"]);
            else
                CurrentProductPage.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(false);
        }
    }
    string SanitizeHTMLString(string inpt)
    {
        char[] inputArray = inpt.ToCharArray();
        System.Text.StringBuilder output = new System.Text.StringBuilder();
        bool shouldAdd = true;
        for(int i = 0; i < inputArray.Length; i++)
        {
            if (inputArray[i] == '<')
                shouldAdd = false;
            if (inputArray[i] == '>')
            {
                shouldAdd = true;
                continue;
            }
            if (shouldAdd && i<inputArray.Length) output.Append(inputArray[i]);

        }
        return output.ToString();
    }
    public void closeProductPage()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
        Cursor.visible = false;
        ControllerInputAsset.cursorLocked = true;
        ControllerInputAsset.cursorInputForLook = true;
        Destroy(CurrentProductPage);
        CurrentProductPage = null;
    }
}
