using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCommands : MonoBehaviour
{
    [SerializeField] bool debugEnabled = true;
    [SerializeField] GameObject playerGO, shipGO;

    void Update()
    {
        if (debugEnabled)
        {
            DebugInput();
        }
    }

    void DebugInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
            MoveToIsland(0);
        if (Input.GetKeyDown(KeyCode.Alpha1))
            MoveToIsland(1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            MoveToIsland(2);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            MoveToIsland(3);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            MoveToIsland(4);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            MoveToIsland(5);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            MoveToIsland(6);
        if (Input.GetKeyDown(KeyCode.Alpha7))
            MoveToIsland(7);
        if (Input.GetKeyDown(KeyCode.Alpha8))
            MoveToIsland(8);
        if (Input.GetKeyDown(KeyCode.Alpha9))
            MoveToIsland(9);
        if (Input.GetKeyDown(KeyCode.X))
            MoveToIsland(10);
    }

    void MoveToIsland(int islandNumber)
    {
        //got magic numbers here by just placing theplayer and ship at each of the islands

        Vector3[] playerIslandCoords = new Vector3[]
        {
            new Vector3(-0.511338055f,40.0161133f,195.991333f),
            new Vector3(-42.222435f,68.5941467f,183.084427f),
            new Vector3(38.8473625f,120.733902f,154.650879f),
            new Vector3(-151.88916f,128.131073f,23.7044945f),
            new Vector3(-152.922882f,-51.1757202f,118.481293f),
            new Vector3(136.378952f,16.5526886f,145.590942f),
            new Vector3(195.414291f,42.3295135f,-4.32335949f),
            new Vector3(-155.520203f,-11.7903595f,-125.407074f),
            new Vector3(-156.621033f,-66.7739868f,-104.9384f),
            new Vector3(-117.942917f,-37.7828217f,-157.081467f),
            new Vector3(46.743782f,147.522888f,-126.74501f)
        };

        Vector3[] shipIslandCoords = new Vector3[]
        {
            new Vector3(-0.463f,39.2039986f,196.104004f),
            new Vector3(-41.4880486f,68.6289673f,183.217926f),
            new Vector3(38.200592f,121.087997f,154.526398f),
            new Vector3(-151.334244f,128.531067f,24.0357151f),
            new Vector3(-153.185638f,-50.6556702f,118.187019f),
            new Vector3(136.725037f,16.1896973f,145.066055f),
            new Vector3(195.404175f,42.4728851f,-3.64219928f),
            new Vector3(-155.032043f,-11.8228455f,-125.798637f),
            new Vector3(-157.054001f,-66.4231262f,-104.508469f),
            new Vector3(-118.510574f,-37.9012604f,-156.58461f),
            new Vector3(47.0691032f,147.900452f,-126.134682f)
        };

        playerGO.transform.localPosition = playerIslandCoords[islandNumber];
        shipGO.transform.localPosition = shipIslandCoords[islandNumber];
    }
}
