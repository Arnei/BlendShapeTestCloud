using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpressionMixingDemoPlayablesController : MonoBehaviour
{
    public GameObject claire1;
    public GameObject claire2;
    public GameObject claire3;

    private ExpressionMixingDemoPlayables animator1;
    private ExpressionMixingDemoPlayables animator2;
    private ExpressionMixingDemoPlayables animator3;

    // Start is called before the first frame update
    void Start()
    {
        animator1 = claire1.GetComponent<ExpressionMixingDemoPlayables>();
        animator2 = claire2.GetComponent<ExpressionMixingDemoPlayables>();
        animator3 = claire3.GetComponent<ExpressionMixingDemoPlayables>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void exampleOne()
    {
        clearAll();
        animator1.weightInterested = 1.0f;
        animator2.weightInterested2 = 1.0f;
        animator3.weightInterested = 1.0f;
        animator3.weightInterested2 = 1.0f;
    }

    public void exampleTwo()
    {
        clearAll();
        animator1.weightInterested = 1.0f;
        animator2.weightEntertained = 1.0f;
        animator3.weightInterested = 1.0f;
        animator3.weightEntertained = 1.0f;
    }

    public void exampleThree()
    {
        clearAll();
        animator1.weightConfused = 1.0f;
        animator2.weightUncomfortable = 1.0f;
        animator3.weightConfused = 1.0f;
        animator3.weightUncomfortable = 1.0f;
    }

    private void clearAll()
    {
        animator1.setToZero();
        animator2.setToZero();
        animator3.setToZero();
    }


}
