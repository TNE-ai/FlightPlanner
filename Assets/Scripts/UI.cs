using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    public GameObject myPrefab;
    public GameObject[] viewPoints = new GameObject[0];

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
            if (viewPoints.Length > 0)
            {
                for (int i=0; i<viewPoints.Length; i++)
                {
                    Destroy(viewPoints[i]);
                }
                viewPoints = new GameObject[0];
            }
        };
        btnPlan.clicked += () =>
        {
            if (viewPoints.Length == 0)
            {
                Debug.Log("btnPlan.clicked");
                viewPoints = new GameObject[1];
                viewPoints[0] = Instantiate(myPrefab, new Vector3(0, 2, 0), Quaternion.identity);
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
