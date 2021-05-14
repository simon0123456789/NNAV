using OpenCvSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class LineDrawer
{
    const int emergencyBreakLimit = 5000;

    #region 2D On Textures
    public static void Draw2DOnTexture(ref Texture2D texture, LineSegmentPoint line, Color color, float thicc = 1f, float smooth = 0.01f)
    {
        Draw2DOnTexture(ref texture, new Vector2(line.P1.X, line.P1.Y), new Vector2(line.P2.X, line.P2.Y), color, thicc, smooth);
    }

    public static void Draw2DOnTexture(ref Texture2D texture, int x1, int y1, int x2, int y2, Color color, float thicc = 1f, float smooth = 0.01f)
    {
        Draw2DOnTexture(ref texture, new Vector2(x1, y1), new Vector2(x2, y2), color, thicc, smooth);
    }
    
    public static void Draw2DOnTexture(ref Texture2D texture, Vector2 start, Vector2 end, Color color, float thicc = 1f, float smooth = 0.01f)
    {
        start.y *= -1;
        end.y *= -1;

        float k = (end.y - start.y) / (end.x - start.x);
        float m = start.y - (k * start.x);

        // X
        int lineStart, lineEnd;
        if(start.x < end.x)
        {
            lineStart = Mathf.RoundToInt(start.x);
            lineEnd = Mathf.RoundToInt(end.x);
        }
        else
        {
            lineStart = Mathf.RoundToInt(end.x);
            lineEnd = Mathf.RoundToInt(start.x);
        }

        for(int x = lineStart; x <= lineEnd; x++)
        {
            float y = k * x + m;
            //if (double.IsNaN(y) || double.IsInfinity(y))
            //  y = start.y;
            texture.SetPixel(x: Mathf.RoundToInt(x), y: Mathf.RoundToInt(y), color);
        }


        // Y
        if (start.y < end.y)
        {
            lineStart = Mathf.RoundToInt(start.y);
            lineEnd = Mathf.RoundToInt(end.y);
        }
        else
        {
            lineStart = Mathf.RoundToInt(end.y);
            lineEnd = Mathf.RoundToInt(start.y);
        }
        for(float i=-thicc; i<thicc; i++)
            for (int y = lineStart; y <= lineEnd; y++)
            {
                float x = (y - (m+i)) / k;
                if (double.IsNaN(x) || double.IsInfinity(x))
                    x = start.x;
                texture.SetPixel(x: Mathf.RoundToInt(x), y: y, color);
            }
        texture.Apply();

    }
    #endregion

    #region 2D On GUI
    public static void DrawGUI2D(int x1, int y1, int x2, int y2, Color color, float thicc = 5f, float smooth = 0.01f)
    {
        DrawGUI2D(new Vector2(x1, y1), new Vector2(x2, y2), color, thicc, smooth);
    }

    public static void DrawGUI2D(Vector2 start, Vector2 end, Color color, float thicc = 5f, float smooth = 0.01f)
    {
        Texture2D texture = new Texture2D(width: 1, height: 1);
        texture.SetPixel(x: 1, y: 1, color);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.Apply();

        float x1, y1, x2, y2;

        if (start.x < end.x)
        {
            x1 = start.x;
            y1 = start.y;
            x2 = end.x;
            y2 = end.y;
        }
        else
        {
            x1 = end.x;
            y1 = end.y;
            x2 = start.x;
            y2 = start.y;
        }

        if (x1 == x2)
        {
            GUI.DrawTexture(new UnityEngine.Rect(x1, y1, thicc, y2 - y1), texture);
        }
        else if (y1 == y2)
        {
            GUI.DrawTexture(new UnityEngine.Rect(x1, y1, x2 - x1, thicc), texture);
        }
        else
        {
            float k = (y2 - y1) / (x2 - x1);
            float m = y1 - (k * x1);

            int emergencyBreakCounter = 0;
            for (float x = x1; x < x2; x += smooth /*k*/)
            {
                emergencyBreakCounter++;
                if (emergencyBreakCounter > emergencyBreakLimit)
                {
                    Debug.LogWarning($"Emergency Break was pulled! | x={x}, x1={x1}, x2={x2}, smooth={smooth}");
                    //EditorApplication.ExecuteMenuItem("Edit/Play");

                }
                float y = k * x + m;
                GUI.DrawTexture(new UnityEngine.Rect(x, y, thicc, thicc), texture);
            }
        }
    }
    #endregion

    public static void Draw3D(Vector3 start, Vector3 end, GameObject lineObject)
    {
        LineRenderer line = lineObject.GetComponent<LineRenderer>();
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    public static void DrawDistanceSensor(Vector3 start, Vector3 end, int sensorID)
    {

        GameObject parent = GameObject.Find("Distance Sensors");
        if(parent == null)
        {
            parent = new GameObject("Distance Sensors");
            parent.layer = 8; 
        }

        if(parent.transform.childCount <= sensorID)
        {
            GameObject newSensor = new GameObject($"Sensor #{sensorID}");
            newSensor.layer = parent.layer;
            newSensor.transform.SetParent(parent.transform);
            LineRenderer newSensorLine = newSensor.AddComponent<LineRenderer>();
            newSensorLine.startWidth = 0.01f;
            newSensorLine.endWidth = 0.02f;
            newSensorLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            newSensorLine.generateLightingData = true;

            Texture2D texture = new Texture2D(width: 1, height: 1);
            texture.SetPixel(x: 1, y: 1, Color.blue);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.Apply();

            newSensor.GetComponent<Renderer>().material.mainTexture = texture;
        }

        LineRenderer sensor = parent.transform.GetChild(sensorID).GetComponent<LineRenderer>();
        sensor.SetPosition(0, start);
        sensor.SetPosition(1, end);


        //LineRenderer line = lineObject.GetComponent<LineRenderer>();
        //line.SetPosition(0, start);
        //
        /*

        lineObjects = new List<GameObject>();
        for (int i = 0; i < outputs.Length; i++)
        {
            lineObjects.Add(new GameObject($"Line #{i}"));
            LineRenderer line = lineObjects[i].AddComponent<LineRenderer>();
            lineObjects[i].layer = 8; // 8 is car layer
            //lineObject[i].GetComponent<LineRenderer>();
            line.startWidth = 0.01f;
            line.endWidth = 0.02f;
            line.startColor = Color.green;
            line.endColor = Color.green;
            line.positionCount = 2;
        }
        */
    }
}
