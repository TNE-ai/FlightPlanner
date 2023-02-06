using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    public GameObject myPrefab;
    public GameObject viewPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        Debug.Log("onEnable");
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Button btnClear = root.Q<Button>("ButtonClear");
        Button btnPlan = root.Q<Button>("ButtonPlan");
        btnClear.clicked += () =>
        {
            Debug.Log("btnClear.clicked");
            if (viewPoint)
            {
                Destroy(viewPoint);
                viewPoint = null;
            }
        };
        btnPlan.clicked += () =>
        {
            Debug.Log("btnPlan.clicked");
            viewPoint = Instantiate(myPrefab, new Vector3(0, 2, 0), Quaternion.identity);
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
