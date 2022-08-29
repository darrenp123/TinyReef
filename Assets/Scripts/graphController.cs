using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;

public class graphController : MonoBehaviour
{
    [SerializeField] private Sprite dotSprite;
    private RectTransform graphContainer;

    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;

    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;

    private List<GameObject> gameObjectList;

    private List<int> valueList;

    private bool useBarChart;

    private void Awake()
    {
        //getting references...
        graphContainer = transform.Find("GraphBackground").GetComponent<RectTransform>();

        labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();

        dashTemplateX = graphContainer.Find("dashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("dashTemplateY").GetComponent<RectTransform>();

        gameObjectList = new List<GameObject>();

        //random values to be displayed on the graph...
        valueList = new List<int>() { UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), };

        //foreach (int rValue in valueList)
        //{
        //    Debug.Log("random values = " + rValue.ToString());
        //}

        IGraphVisual lineGraphVisual = new LineGraphVisual(graphContainer, dotSprite, Color.blue, new Color(0, 0, 1, 0.2f));
        IGraphVisual barChartVisual = new BarChartVisual(graphContainer, Color.blue, 0.9f);

        //create and display the graph with the values set in valueList above...
        ShowGraph(valueList, lineGraphVisual, -1, (int _i) => "Day " + (_i + 1), (float _f) => Mathf.RoundToInt(_f) + "");
    }

    private void Start()
    {
        StartCoroutine(changeGraphTester(5f));
    }

    private IEnumerator changeGraphTester(float delayValue)
    {
        if (useBarChart)
        {
            useBarChart = false;

            valueList.Clear();

            valueList = new List<int>() { UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), };

            //foreach (int rValue in valueList)
            //{
            //    Debug.Log("random values = " + rValue.ToString());
            //}

            IGraphVisual lineGraphVisual = new LineGraphVisual(graphContainer, dotSprite, Color.blue, new Color(0, 0, 1, 0.2f));
            IGraphVisual barChartVisual = new BarChartVisual(graphContainer, Color.blue, 0.9f);

            ShowGraph(valueList, barChartVisual, -1, (int _i) => "" + (_i + 1), (float _f) => "" + Mathf.RoundToInt(_f));
        }
        else if (!useBarChart)
        {
            useBarChart = true;

            valueList.Clear();

            valueList = new List<int>() { UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), UnityEngine.Random.Range(0, 500), };

            //foreach (int rValue in valueList)
            //{
            //    Debug.Log("random values = " + rValue.ToString());
            //}

            IGraphVisual lineGraphVisual = new LineGraphVisual(graphContainer, dotSprite, Color.blue, new Color(0, 0, 1, 0.2f));
            IGraphVisual barChartVisual = new BarChartVisual(graphContainer, Color.blue, 0.9f);

            ShowGraph(valueList, lineGraphVisual, -1, (int _i) => "" + (_i + 1), (float _f) => "" + Mathf.RoundToInt(_f));
        }

        yield return new WaitForSecondsRealtime(delayValue);

        StartCoroutine(changeGraphTester(delayValue));
    }

    private void ShowGraph(List<int> valueList, IGraphVisual graphVisual, int maxVisibleValueAmount = -1, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
    {
        if (getAxisLabelX == null)
        {
            getAxisLabelX = delegate (int _i) { return _i.ToString(); };
        }

        if (getAxisLabelY == null)
        {
            getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
        }

        if (maxVisibleValueAmount <= 0)
        {
            maxVisibleValueAmount = valueList.Count;
        }

        foreach (GameObject gameObject in gameObjectList)
        {
            Destroy(gameObject);
        }

        gameObjectList.Clear();

        float graphWidth = graphContainer.sizeDelta.x * -87f;
        float graphHeight = graphContainer.sizeDelta.y * 18f;

        //float graphHeight = graphContainer.sizeDelta.y * 18f; //this should just be graphContainer.sizeDelta.y; but because the UI is funky and im shite at this, this was my workaround lol......(i think sizeDelta measures from anchor point to anchor point but in debug it returns negative number from the UI element which is messing with other calculations for placement so multiplied by random number until it looked like it fitted...*needs attention*...)

        float yMaximum = valueList[0];
        float yMinimum = valueList[0];

        //sets min and max values displayed on graph depending on values in valuesList...
        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
        {
            int value = valueList[i];

            if (value > yMaximum)
            {
                yMaximum = value;
            }

            if (value < yMinimum)
            {
                yMinimum = value;
            }
        }

        float yDifference = yMaximum - yMinimum;

        if (yDifference <= 0)
        {
            yDifference = 5f;
        }

        //keeps 20% buffer even split between top and bottom...
        yMaximum = yMaximum + (yDifference * 0.2f);
        yMinimum = yMinimum - (yDifference * 0.2f);

        yMinimum = 0f; //starts graph at 0, remove this if wanting the graph to include negative values...

        float xSize = graphWidth / (maxVisibleValueAmount + 1);

        int xIndex = 0;

        //LineGraphVisual lineGraphVisual = new LineGraphVisual(graphContainer, dotSprite, Color.white, new Color(1, 1, 1, 0.25f));

        //BarChartVisual barChartVisual = new BarChartVisual(graphContainer, Color.blue, 0.9f);

        //GameObject lastDotGameObject = null;

        //creates and displays various elements on the graph - circles that represent trait/time value, line-connectors between the circles and a label for each value on the x-axis...
        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
        {
            //the circles...
            float xPosition = xSize + xIndex * (xSize);
            float yPosition = ((valueList[i] - yMinimum) / (yMaximum - yMinimum)) * -graphHeight;

            //GameObject barGameObject = CreateBar(new Vector2(xPosition, yPosition), xSize * 0.9f);
            gameObjectList.AddRange(graphVisual.AddGraphVisual(new Vector2(xPosition, yPosition), xSize));
            //gameObjectList.AddRange(barChartVisual.AddGraphVisual(new Vector2(xPosition, yPosition), xSize));

            //GameObject dotGameObject = CreateDot(new Vector2(xPosition, yPosition));
            //gameObjectList.Add(dotGameObject);

            ////the line-connectors...
            //if (lastDotGameObject != null)
            //{
            //    GameObject dotConnectionGameObject = CreateDotConnection(lastDotGameObject.GetComponent<RectTransform>().anchoredPosition, dotGameObject.GetComponent<RectTransform>().anchoredPosition);
            //    gameObjectList.Add(dotConnectionGameObject);
            //}

            //lastDotGameObject = dotGameObject;

            //the labels on the x-axis...
            RectTransform labelX = Instantiate(labelTemplateX);
            labelX.SetParent(graphContainer, false);
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(xPosition, -10f);
            labelX.GetComponent<Text>().text = getAxisLabelX(i);
            gameObjectList.Add(labelX.gameObject);

            //the background dashes on the x-axis...
            RectTransform dashX = Instantiate(dashTemplateX);
            dashX.SetParent(graphContainer, false);
            dashX.gameObject.SetActive(true);
            dashX.anchoredPosition = new Vector2(xPosition, -10f);
            gameObjectList.Add(dashX.gameObject);

            xIndex++;
        }

        //this determines how many values are shown on the y-axis...
        int separatorCount = 10;

        //creates and displays the y-axis labels and background dashes...
        for (int i = 0; i <= separatorCount; i++)
        {
            //the y-axis labels...
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer, false);
            labelY.gameObject.SetActive(true);
            float normalizedValue = i * 1f / separatorCount;
            labelY.anchoredPosition = new Vector2(10f, normalizedValue * -graphHeight);
            labelY.GetComponent<Text>().text = getAxisLabelY(yMinimum + (normalizedValue * (yMaximum - yMinimum)));
            gameObjectList.Add(labelY.gameObject);

            //the background dashes...
            RectTransform dashY = Instantiate(dashTemplateY);
            dashY.SetParent(graphContainer, false);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(-4f, normalizedValue * -graphHeight);
            gameObjectList.Add(dashY.gameObject);
        }
    }

    private interface IGraphVisual
    {
        List<GameObject> AddGraphVisual(Vector2 graphPosition, float graphPositionWidth);
    }

    private class BarChartVisual : IGraphVisual
    {
        private RectTransform graphContainer;
        private Color barColor;
        private float barWidthMultiplier;

        public BarChartVisual(RectTransform graphContainer, Color barColor, float barWidthMultiplier)
        {
            this.graphContainer = graphContainer;
            this.barColor = barColor;
            this.barWidthMultiplier = barWidthMultiplier;
        }

        public List<GameObject> AddGraphVisual(Vector2 graphPosition, float graphPositionWidth)
        {
            GameObject barGameObject = CreateBar(graphPosition, graphPositionWidth);
            return new List<GameObject>() { barGameObject };
        }

        private GameObject CreateBar(Vector2 graphPosition, float barWidth)
        {
            GameObject gameObject = new GameObject("bar", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().color = barColor;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(graphPosition.x, 0.0f);
            rectTransform.sizeDelta = new Vector2(barWidth * barWidthMultiplier, graphPosition.y);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            rectTransform.pivot = new Vector2(0.5f, 0.0f);

            return gameObject;
        }
    }

    private class LineGraphVisual : IGraphVisual
    {
        private RectTransform graphContainer;
        private Sprite dotSprite;
        private GameObject lastDotGameObject;
        private Color dotColor, dotConnectionColor;

        public LineGraphVisual(RectTransform graphContainer, Sprite dotSprite, Color dotColor, Color dotConnectionColor)
        {
            this.graphContainer = graphContainer;
            this.dotSprite = dotSprite;
            this.dotColor = dotColor;
            this.dotConnectionColor = dotConnectionColor;
            lastDotGameObject = null;
        }

        public List<GameObject> AddGraphVisual(Vector2 graphPosition, float graphPositionWidth)
        {
            List<GameObject> gameObjectList = new List<GameObject>();
            GameObject dotGameObject = CreateDot(graphPosition);
            gameObjectList.Add(dotGameObject);
            if (lastDotGameObject != null)
            {
                GameObject dotConnectionGameObject = CreateDotConnection(lastDotGameObject.GetComponent<RectTransform>().anchoredPosition, dotGameObject.GetComponent<RectTransform>().anchoredPosition);
                gameObjectList.Add(dotConnectionGameObject);
            }
            lastDotGameObject = dotGameObject;
            return gameObjectList;
        }

        private GameObject CreateDot(Vector2 anchoredPosition)
        {
            //creates the circle object to be used as visual representation of value when called...
            GameObject gameObject = new GameObject("dot", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().sprite = dotSprite;
            gameObject.GetComponent<Image>().color = dotColor;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(11, 11); //change back to 11, 11 once done fannying aboot....
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            return gameObject;
        }

        private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
        {
            //creates the lines that connects each circle on the graph...
            GameObject gameObject = new GameObject("dotConnection", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().color = dotConnectionColor;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            Vector2 dir = (dotPositionB - dotPositionA).normalized;
            float distance = Vector2.Distance(dotPositionA, dotPositionB);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(distance, 3f);
            rectTransform.anchoredPosition = dotPositionA + dir * distance * 0.5f;
            rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));

            return gameObject;
        }
    }
}