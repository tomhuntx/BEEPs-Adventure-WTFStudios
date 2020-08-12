using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BrokenLines : MonoBehaviour
{
    private enum ShapeBase { Square, Circle }
    [SerializeField] private ShapeBase shapeType = ShapeBase.Square;
    [SerializeField] private Color color = Color.white;
    private Color currentColor;
    private LineRenderer line;
    private Material currentLineMaterial;
    private const string SQUARE_BASE = "Lines Resources/Square Base";
    private const string CIRCLE_BASE = "Lines Resources/Circle Base";

    public LineRenderer LineRendererComponent { get { return line; } set { line = value; } }
    public Color LineColor { get { return color; } set { color = value; } }

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        currentLineMaterial = SetLineType(shapeType);
        line.material = currentLineMaterial;
        currentColor = color;
    }

    void Update()
    {
        //Vector2 newPosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 newPosition = line.GetPosition(1);
        float distance = Vector3.Distance(line.GetPosition(0), line.GetPosition(1));
        line.material.SetTextureOffset("_MainTex", new Vector2(Time.timeSinceLevelLoad * 4f, 0f));
        line.material.SetTextureScale("_MainTex", new Vector2(distance, 1));

        if (currentLineMaterial != SetLineType(shapeType))
        {
            currentLineMaterial = SetLineType(shapeType);
            line.material = currentLineMaterial;
        }

        if (currentColor != color)
        {
            currentColor = color;
            line.material.color = currentColor;
        }
    }

    private Material SetLineType(ShapeBase type)
    {
        switch (type)
        {
            case ShapeBase.Circle:
                return Resources.Load(CIRCLE_BASE) as Material;

            case ShapeBase.Square:
                return Resources.Load(SQUARE_BASE) as Material;

            default:
                return Resources.Load(SQUARE_BASE) as Material;
        }
    }
}
