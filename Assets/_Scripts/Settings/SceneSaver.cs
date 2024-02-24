using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * When saving this needs to go in the file

Player data
Scene name
Date, Time
screenshot of save
From the level:
	need to save existing enemies
	pickups
	progress in level???? (this needs to be added)
	
*/


public class SceneSaver : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

/*
 * using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class SceneSerializer : MonoBehaviour
{
    [System.Serializable]
    public class CubeData
    {
        public float x;
        public float y;
        public float z;
    }

    public List<GameObject> cubes;

    public void SaveScene()
    {
        List<CubeData> cubeDataList = new List<CubeData>();

        foreach (GameObject cube in cubes)
        {
            CubeData cubeData = new CubeData();
            cubeData.x = cube.transform.position.x;
            cubeData.y = cube.transform.position.y;
            cubeData.z = cube.transform.position.z;
            cubeDataList.Add(cubeData);
        }

        string json = JsonConvert.SerializeObject(cubeDataList);
        File.WriteAllText(Application.persistentDataPath + "/scene.json", json);
    }

    public void LoadScene()
    {
        string json = File.ReadAllText(Application.persistentDataPath + "/scene.json");
        List<CubeData> cubeDataList = JsonConvert.DeserializeObject<List<CubeData>>(json);

        foreach (CubeData cubeData in cubeDataList)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(cubeData.x, cubeData.y, cubeData.z);
        }
    }
}*/