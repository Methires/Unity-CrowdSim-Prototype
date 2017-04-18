using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ProceduralSceneGenerator
{
    public class BuildScene : MonoBehaviour
    {
        public GameObject simulationOverseerPrefab;
        public GameObject groundPrefab;
        private GameObject ground;
        private List<GameObject> cityTiles;
        public GameObject xStreets;
        public GameObject zStreets;
        public GameObject croosRoads;
        public GameObject citySquareTile;
        public GameObject grassTile;
        public GameObject grassTileWithoutTrees;
        public GameObject cameraPrafab;
        public GameObject reflectionProbe;

        private BuildingTemplateMaterials btm;
        private GameObject scenePrefab;

        private float lookAtHeightSquare;
        private float lookAtHeightCrossRoad;

        public int mapSize = 10;
        private int citySquareSize = 4;
        private int mapWidth;
        private int mapHeight;
        float buildingFootprint = 4.15f;
        float[,] mapgrid;

        void SetCitySquareSize()
        {

            if (mapSize < 4)
            {
                throw new System.Exception("Scene too small");
            }
            else if (mapSize < 8)
            {
                citySquareSize = 2;
                lookAtHeightSquare = 0.7f;
                lookAtHeightCrossRoad = 1f;
            }

            else if (mapSize < 12)
            {
                citySquareSize = 4;
                lookAtHeightSquare = 0.15f;
                lookAtHeightCrossRoad = 5f;
            }
            else if (mapSize < 21)
            {
                citySquareSize = 6;
                lookAtHeightSquare = 0f;
                lookAtHeightCrossRoad = 6f;
            }
            else throw new System.Exception("Scene too big");
        }

        private struct Vec2Int
        {
            public int h;
            public int w;
            public Vec2Int(int width, int height)
            {
                h = height;
                w = width;
            }
            public override string ToString()
            {
                return "Wektor 2 Int: (" + w + "," + h + ")";
            }
        }

        void Start()
        {
            Generate();
        }

        public void Generate()
        {
            Transform renderer = new GameObject("Renderer").transform;

            Transform buildingsRend = new GameObject("Buildings Renderer").transform;
            buildingsRend.parent = renderer;

            Transform streetsRend = new GameObject("Streets Renderer").transform;
            streetsRend.parent = renderer;

            Transform miscRend = new GameObject("Miscellaneous Renderer").transform;
            miscRend.parent = renderer;
            Transform greenAreasRend = new GameObject("Green Areas Renderer").transform;
            greenAreasRend.parent = renderer;

            Transform camRend = new GameObject("Cameras Renderer").transform;
            camRend.parent = renderer;

            ground = (GameObject)Instantiate(groundPrefab, groundPrefab.transform.position, Quaternion.identity, renderer);

            btm = GetComponent<BuildingTemplateMaterials>();
            cityTiles = new List<GameObject>();
            mapHeight = mapSize;
            mapWidth = mapSize;
            SetCitySquareSize();

            grassTile.transform.localScale = new Vector3(buildingFootprint, 0.1f, buildingFootprint);
            citySquareTile.transform.localScale = new Vector3(buildingFootprint, 0.1f, buildingFootprint);
            int center = mapSize / 2;
            int start = center - citySquareSize / 2;
            int end = center + (center - start);
            float seed = Random.Range(0, 10);
            mapgrid = new float[mapSize, mapSize];

            //generate map data
            for (int h = 0; h < mapHeight; h++)
            {
                for (int w = 0; w < mapWidth; w++)
                {
                    float perlinNoise = Mathf.PerlinNoise(w / 10.0f * seed, h / 10.0f * seed) * 10;
                    mapgrid[w, h] = perlinNoise;
                }
            }
            //build square
            for (int h = start; h < end; h++)
            {
                for (int w = start; w < end; w++)
                {
                    mapgrid[w, h] = -4;
                }
            }
            ///build streets 
            //x streets     
            int x1 = Random.Range(0, start);
            for (int h = 0; h < mapHeight; h++)
            {
                mapgrid[x1, h] = -1;
            }

            int x2 = Random.Range(end, mapHeight);
            for (int h = 0; h < mapHeight; h++)
            {
                mapgrid[x2, h] = -1;
            }

            //z streets
            int z1 = Random.Range(0, start);
            for (int w = 0; w < mapWidth; w++)
            {
                if (mapgrid[w, z1] == -1)
                    mapgrid[w, z1] = -3;
                else
                    mapgrid[w, z1] = -2;
            }

            int z2 = Random.Range(end, mapHeight);
            for (int w = 0; w < mapWidth; w++)
            {
                if (mapgrid[w, z2] == -1)
                    mapgrid[w, z2] = -3;
                else
                    mapgrid[w, z2] = -2;
            }

            //Ensures cameras will be set right
            List<Vec2Int> grassWithoutTreesLocations = new List<Vec2Int>();
            if (x1 != 0 && z1 != 0)
            {
                grassWithoutTreesLocations.Add(new Vec2Int(x1 - 1, z1 - 1));
            }
            if (x1 != 0 && z2 != mapWidth - 1)
            {
                grassWithoutTreesLocations.Add(new Vec2Int(x1 - 1, z2 + 1));
            }
            if (x2 != mapHeight - 1 && z1 != 0)
            {
                grassWithoutTreesLocations.Add(new Vec2Int(x2 + 1, z1 - 1));
            }

            if (x2 != mapHeight - 1 && z2 != mapWidth - 1)
            {
                grassWithoutTreesLocations.Add(new Vec2Int(x2 + 1, z2 + 1));
            }

            //buildings
            CreateBuildings();



            float move = -(mapSize / 2 * buildingFootprint) + buildingFootprint / 2;
            renderer.position = new Vector3(-move, +citySquareTile.transform.lossyScale.y / 2, -move);

            //znajdowanie skrzyżowań przy rynku
            List<Vec2Int> points = new List<Vec2Int>();
            for (int h = 0; h < mapHeight; h++)
            {
                for (int w = 0; w < mapWidth; w++)
                {
                    if (mapgrid[w, h] == -3)
                    {
                        Vec2Int vec2 = new Vec2Int(w, h);
                        points.Add(vec2);
                    }
                }
            }

            ///Obracanie budynków wewnątrz rynku
            // Tworzenie listy heightów na których znajdują się te budynki
            List<int> hses = new List<int>();
            for (int h = points[0].h; h < points[3].h; h++)
            {
                for (int w = points[0].w; w < points[3].w; w++)
                {
                    if (mapgrid[w, h] == -4)
                    {
                        if (hses.Exists(g => g == h) == false)
                        {
                            hses.Add(h);
                        }
                    }
                }
            }
            //Sortowanie listy (w sumie niepotrzebne)
            hses.Sort();

            //Zmiana wartośći w tablicy, aby możliwe było wstawienie budynku z odpowiednią rotacja     
            for (int h = hses[0]; h <= hses[hses.Count - 1]; h++)
            {
                for (int w = points[0].w; w < points[3].w; w++)
                {
                    if (mapgrid[w, h] > 0)
                    {
                        mapgrid[w, h] += 10;
                    }
                }
            }

            ///Obracanie budynków na zewnątrz rynku
            //Przez skrzyżowaniami
            for (int h = 0; h < mapHeight; h++)
            {
                for (int w = 0; w < points[0].w; w++)
                {
                    if (mapgrid[w, h] > 0)
                    {
                        mapgrid[w, h] += 10;
                    }
                }
            }
            //Po skrzyżowaniach
            for (int h = 0; h < mapHeight; h++)
            {
                for (int w = points[3].w; w < mapWidth; w++)
                {
                    if (mapgrid[w, h] > 0)
                    {
                        mapgrid[w, h] += 10;
                    }
                }
            }

            //Replaces the tiles which might be coliding with cameras
            foreach (Vec2Int vec in grassWithoutTreesLocations)
            {
                mapgrid[vec.w, vec.h] = -5;
            }

            int squareStart = center - citySquareSize / 2;
            int squareEnd = center + (citySquareSize / 2) - 1;


            //Brzegi rynku
            List<Vec2Int> brzegowe = new List<Vec2Int>();

            brzegowe.Add(new Vec2Int(squareStart, squareStart));
            brzegowe.Add(new Vec2Int(squareStart, squareEnd));
            brzegowe.Add(new Vec2Int(squareEnd, squareEnd));
            brzegowe.Add(new Vec2Int(squareEnd, squareStart));

            //Offset kamer przy skrzyżowaniach
            float ofs = 3f;
            float camHeight0 = 2f;
            Vector3[] cameraOffset = new Vector3[4];
            cameraOffset[0] = new Vector3(-ofs, camHeight0, -ofs);
            cameraOffset[1] = new Vector3(ofs, camHeight0, -ofs);
            cameraOffset[2] = new Vector3(-ofs, camHeight0, ofs);
            cameraOffset[3] = new Vector3(ofs, camHeight0, ofs);

            ///Końcowa pętla wstawiająca (prawie) wszystkie obiekty            
            int k = 0;
            int index = 0;
            for (int h = 0; h < mapHeight; h++)
            {
                for (int w = 0; w < mapWidth; w++)
                {
                    float result = mapgrid[w, h];
                    Vector3 pos = new Vector3(w * buildingFootprint, 0, h * buildingFootprint);
                    if (result < -4)
                    {
                        GameObject grassWt = Instantiate(grassTileWithoutTrees, pos, grassTileWithoutTrees.transform.rotation, greenAreasRend) as GameObject;

                    }
                    else if (result < -3)
                    {
                        GameObject cst = Instantiate(citySquareTile, pos, citySquareTile.transform.rotation, streetsRend) as GameObject;
                        cst.transform.localScale = new Vector3(buildingFootprint, 0.1f, buildingFootprint);
                        cst.name = "Square Tile " + w + h;
                    }
                    else if (result < -2)
                    {
                       
                        float[] b = {0, -90f, 90f, -180f};
                        var rotto = Quaternion.Euler(Vector3.up * b[index]);
                        var cr = Instantiate(croosRoads, pos, rotto, streetsRend) as GameObject;
                        cr.transform.localScale = Vector3.one* (buildingFootprint / 30);
                        cr.transform.rotation = rotto;
                        Debug.Log("Object rotated by " + b[index]);
                        index++;
                        //Create cameras above cross roads
                        GameObject cam = Instantiate(cameraPrafab);//new GameObject("Camera" + w + h);
                        cam.name = "Camera" + w + h;                       
                        cam.AddComponent<SphereCollider>();
                        cam.GetComponent<SphereCollider>().radius = 0.3f;
                        cam.GetComponent<Camera>().fieldOfView = 30;
                        cam.transform.position = pos + cameraOffset[k];
                        cam.transform.LookAt(renderer.position - Vector3.up * lookAtHeightCrossRoad);
                        cam.transform.parent = camRend;

                        GameObject.Find("LookAt").GetComponent<CamerasController>()._cameras.Add(cam);
                        k++;
                    }
                    else if (result < -1)
                    {
                        var xs = Instantiate(xStreets, pos, xStreets.transform.rotation, streetsRend) as GameObject;
                        xs.transform.localScale = Vector3.one * (buildingFootprint / 30);
                    }
                    else if (result < 0)
                    {
                        var zs = Instantiate(zStreets, pos, zStreets.transform.rotation, streetsRend) as GameObject;
                        zs.transform.localScale = Vector3.one * (buildingFootprint / 30);
                    }
                    else
                    {
                        Transform tileParent;
                        for (int i = 1; i <= cityTiles.Count; i++)
                        {
                            //Change the parent renderer if it's a grass tile
                            if (cityTiles[i - 1].name == "GrassTile")
                            {
                                tileParent = greenAreasRend;
                            }
                            else
                            {
                                tileParent = buildingsRend;
                            }

                            //
                            if (result < (i * (10.0f / cityTiles.Count)))
                            {
                                Instantiate(cityTiles[i - 1], pos, randomQuat(), tileParent);
                                break;
                            }
                            else if (result > 10)
                            {
                                if (result < (10 + (i * (10.0f / cityTiles.Count))))
                                {
                                    Instantiate(cityTiles[i - 1], pos, randomQuat(), tileParent);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            //Instantiate benches and lamp posts
            GameObject bench = btm.bench;
            GameObject lamppost = btm.lamppost;
            GameObject trashbin = btm.trashbin;
            float lampoffset = 2.0f;
            float benchoffset = 1.0f;
            for (int h = squareStart; h <= squareEnd; h++)
            {
                Vector3 pos = new Vector3((squareStart) * buildingFootprint - citySquareTile.transform.lossyScale.x / 2 * 0.8f, citySquareTile.transform.lossyScale.y / 2, h * buildingFootprint);
                Instantiate(bench, pos, Quaternion.identity, miscRend);
                Instantiate(trashbin, new Vector3(pos.x, pos.y, pos.z - benchoffset), Quaternion.identity, miscRend);

                pos = new Vector3((squareEnd) * buildingFootprint + citySquareTile.transform.lossyScale.x / 2 * 0.8f, citySquareTile.transform.lossyScale.y / 2, h * buildingFootprint);
                Instantiate(bench, pos, Quaternion.identity, miscRend);
                Instantiate(trashbin, new Vector3(pos.x, pos.y, pos.z - benchoffset), Quaternion.identity, miscRend);
                if (h < squareEnd)
                {
                    pos = new Vector3((squareStart) * buildingFootprint - citySquareTile.transform.lossyScale.x / 2 * 0.8f, citySquareTile.transform.lossyScale.y / 2, h * buildingFootprint + lampoffset);
                    Instantiate(lamppost, pos, lamppost.transform.rotation, miscRend);

                    pos = new Vector3((squareEnd) * buildingFootprint + citySquareTile.transform.lossyScale.x / 2 * 0.8f, citySquareTile.transform.lossyScale.y / 2, h * buildingFootprint + lampoffset);
                    Instantiate(lamppost, pos, lamppost.transform.rotation, miscRend);
                }
            }
            for (int w = squareStart; w <= squareEnd; w++)
            {
                Vector3 pos = new Vector3((w) * buildingFootprint, citySquareTile.transform.lossyScale.y / 2, squareStart * buildingFootprint - citySquareTile.transform.lossyScale.z / 2 * 0.8f);
                Instantiate(bench, pos, Quaternion.Euler(Vector3.up * 90f), miscRend);
                Instantiate(trashbin, new Vector3(pos.x - benchoffset, pos.y, pos.z), Quaternion.Euler(Vector3.up * 180f), miscRend);

                pos = new Vector3(w * buildingFootprint, citySquareTile.transform.lossyScale.y / 2, squareEnd * buildingFootprint + citySquareTile.transform.lossyScale.z / 2 * 0.8f);
                Instantiate(bench, pos, Quaternion.Euler(Vector3.up * 90f), miscRend);
                Instantiate(trashbin, new Vector3(pos.x - benchoffset, pos.y, pos.z), Quaternion.identity, miscRend);
                if (w < squareEnd)
                {
                    pos = new Vector3(w * buildingFootprint + lampoffset, citySquareTile.transform.lossyScale.y / 2, (squareStart) * buildingFootprint - citySquareTile.transform.lossyScale.z / 2 * 0.8f);
                    Instantiate(lamppost, pos, lamppost.transform.rotation, miscRend);

                    pos = new Vector3(w * buildingFootprint + lampoffset, citySquareTile.transform.lossyScale.y / 2, (squareEnd) * buildingFootprint + citySquareTile.transform.lossyScale.z / 2 * 0.8f);
                    Instantiate(lamppost, pos, lamppost.transform.rotation, miscRend);
                }
            }
            //Instantiate cameras
            float camheight = 1.5f;
            foreach (Vec2Int vec in brzegowe)
            {
                GameObject cam = Instantiate(cameraPrafab);//new GameObject("Camera" + vec.w + vec.h);
                //cam.AddComponent<Camera>();
                cam.name = "Camera" + vec.w + vec.h;
                cam.AddComponent<SphereCollider>();
                cam.GetComponent<SphereCollider>().radius = 0.3f;
                cam.GetComponent<Camera>().fieldOfView = 30;
                cam.transform.position = new Vector3((float)vec.w * buildingFootprint, camheight, (float)vec.h * buildingFootprint);
                cam.transform.LookAt(renderer.position + lookAtHeightSquare * Vector3.up);
                cam.transform.parent = camRend;
                GameObject.Find("LookAt").GetComponent<CamerasController>()._cameras.Add(cam);
            }


            foreach (Transform child in renderer)
            {
                if (child.tag != "Ground")
                {
                    child.localScale = Vector3.one * 3;
                }
            }

            renderer.position = Vector3.up * 0.15f;

            GameObject navMeshPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            navMeshPlane.transform.localScale = new Vector3(buildingFootprint / 10 * mapSize * 3, 0, buildingFootprint / 10 * mapSize * 3);
            navMeshPlane.GetComponent<MeshRenderer>().materials = new Material[0];
            navMeshPlane.name = "NavMeshPlane";

            navMeshPlane.transform.parent = renderer;
            if (mapSize % 2 == 1)
            {
                navMeshPlane.transform.localPosition = 3 * new Vector3((buildingFootprint / 2), 0, buildingFootprint / 2);

            }
            else
            {
                navMeshPlane.transform.localPosition = (Vector3.zero);
            }
            // navMeshPlane.transform.position = new Vector3(navMeshPlane.transform.position.x, 0f, navMeshPlane.transform.position.z);

            ground.transform.parent = renderer;
            navMeshPlane.isStatic = true;
            scenePrefab = renderer.gameObject;

            foreach (Transform building in buildingsRend)
            {
                building.GetChild(0).GetComponent<UnityEngine.AI.NavMeshObstacle>().enabled = true;
            }

            foreach (Transform sceneProp in scenePrefab.transform)
            {
                if (sceneProp.tag != "Ground")
                {
                    sceneProp.gameObject.isStatic = true;
                }
            }
            GameObject instance = (GameObject)Instantiate(reflectionProbe, renderer);
            instance.GetComponent<ReflectionProbe>().size = mapSize * 10 * Vector3.one * 1.2f;

            GameObject overseer = Instantiate(simulationOverseerPrefab);
            overseer.transform.parent = scenePrefab.transform;

            SaveScenePrefab();
            Material[] leftoverMaterials = FindObjectsOfType<Material>();


            for (int i = 0; i < leftoverMaterials.Length; i++)
            {
                DestroyImmediate(leftoverMaterials[i]);
            }


        }
        /// <summary>
        /// Funkcja wykorzystująca klasę *CreateBuilding* do generacji prefabów
        /// </summary>
        private void CreateBuildings()
        {
            var createBuilding = new CreateBuilding();
            //Creating random buildings            
            cityTiles.Add(createBuilding.GenerateRandomBuilding(4, 3, 5));
            cityTiles.Add(createBuilding.GenerateRandomBuilding(4, 3, 3));
            cityTiles.Add(grassTile);
            cityTiles.Add(createBuilding.GenerateRandomBuilding(4, 3, 10));
            cityTiles.Add(createBuilding.GenerateRandomBuilding(4, 3, 15));
        }
        private void SaveScenePrefab()
        {
            int sceneId = GetGighestScenePrefabIndex() + 1;
            PrefabUtility.CreatePrefab(string.Format("Assets/Scenes/ScenePrefab#{0}.prefab", sceneId), scenePrefab);
        }

        private int GetGighestScenePrefabIndex()
        {
            string path = string.Format("{0}//{1}", Application.dataPath, "Scenes");
            string[] scenePrefabs = Directory.GetFiles(path, "*.prefab");
            int highestIndex = 0;
            foreach (var scene in scenePrefabs)
            {
                string fileName = Path.GetFileNameWithoutExtension(scene);
                int index = 0;
                int.TryParse(fileName.Split('#')[1], out index);

                if (index > highestIndex)
                {
                    highestIndex = index;
                }
            }

            return highestIndex;
        }
        public static void Quit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
        private static Quaternion randomQuat()
        {
            List<Vector3> rots = new List<Vector3>();
            rots.Add(Vector3.up * 90);
            rots.Add(Vector3.up * -90);
            rots.Add(Vector3.up * 180);
            rots.Add(Vector3.zero);

            int indx = Random.Range(0, rots.Count);

            return Quaternion.Euler(rots[indx]);
        }


    }
}
