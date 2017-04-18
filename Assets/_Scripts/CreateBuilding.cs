using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProceduralSceneGenerator
{
    public class CreateBuilding
    {
        //Dimentions of the building
        public int width;
        public int length;
        public int height;

        //public static Color wallColor = ColorE.white.WithA(0);
        //Material of tje facade of the building
        public Material facadeMaterial;
        private static Material concrete;
        private Texture roofTex;

        //Prefabs from which the building will be created
        private GameObject buildingPrefab;
        private GameObject doorPrefab;
        private GameObject windowPrefab;
        private GameObject roofType;

        //GameObject which should be returned and GameObject which this class operates on
        private GameObject gameObject;
        private GameObject returnGo;
        //Some help stuff...
        private GameObject parentCube;
        private Transform parentCubeTransform;
        // Walls of the building ...
        private List<GameObject> walls;

        #region Lists
        private static List<GameObject> windowPrefabs;
        private static List<GameObject> doorPrefabs;
        private static List<Texture> roofTextures;
        private static List<Material> mats;
        private static List<GameObject> roofTypes;        
        private static Color randomColor;
        static BuildingTemplateMaterials btm = GameObject.Find("BuildScene").GetComponent<BuildingTemplateMaterials>();
        string matName;
        #endregion

        public GameObject GenerateBuilding(int w, int l, int h, int facadeMaterialIndex, int doorPrefabIndex, int windowPrefabIndex, int roofTypeIndex, int roofTexIndex)
        {
            EnsureCorrectDir();
            LoadResources();
            LoadAssets();

            width = w;
            length = l;
            height = h;
            facadeMaterial = mats[facadeMaterialIndex];
            doorPrefab = doorPrefabs[doorPrefabIndex];
            windowPrefab = windowPrefabs[windowPrefabIndex];
            roofType = roofTypes[roofTypeIndex];
            roofTex = roofTextures[roofTexIndex];
            GenBuild();
            DestroyGo();

            return gameObject;

        }
        public GameObject GenerateRandomBuilding(int w, int l, int h)
        {
            EnsureCorrectDir();
            LoadResources();
            LoadAssets();
            width = w;
            length = l;
            height = h;

            facadeMaterial = mats[Random.Range(0, mats.Count)];
            doorPrefab = doorPrefabs[Random.Range(0, doorPrefabs.Count)];
            windowPrefab = windowPrefabs[Random.Range(0, windowPrefabs.Count)];
            int randomRoof = Random.Range(0, roofTypes.Count);

            if (height > 5)
            {
                roofType = roofTypes[1];
            }
            else
            {
                roofType = roofTypes[randomRoof];
            }

            roofTex = roofTextures[Random.Range(0, roofTextures.Count)];
            randomColor = new Color(Random.value, Random.value, Random.value, 1.0f);

            

            GenBuild();
            CreateRoofMats();

            DestroyGo();
         
            return gameObject;
        }

        private void EnsureCorrectDir()
        {
            string dirPath = Application.dataPath + "/Resources/GeneratedMaterials/";
            if (!System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }
        }

        private void DestroyGo()
        {

            //returnGo = gameObject;
            //Object.DestroyImmediate(gameObject);
        }

        //Sends parameters to the BuildingUI class which is attached to the parent
        private void LoadResources()
        {
            // Loading a building prefab from resources (it's basically a cube)
            buildingPrefab = btm.buildingPrefab;
        }
        private static void LoadAssets()
        {
            concrete = btm.concreteMaterial;
            mats = btm.facadeMaterials;
            doorPrefabs = btm.doorPrefabs;
            windowPrefabs = btm.windowPrefabs;
            roofTextures = btm.roofTextures;
            roofTypes = btm.roofTypes;
        }
        //Function for generating a building
        public void GenBuild()
        {
            gameObject = new GameObject("Building");
            walls = new List<GameObject>(); //walls of the Building Prefab

            parentCube = (GameObject)Object.Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);
            parentCubeTransform = parentCube.transform;

            parentCube.transform.parent = gameObject.transform;
            parentCubeTransform.localScale = new Vector3(width, height, length);

            //Adding walls to the table
            for (int i = 0; i < parentCubeTransform.childCount; i++)
            {

                walls.Add(parentCubeTransform.GetChild(i).gameObject);

            }

            Vector3 tempPos = parentCubeTransform.position;
            Quaternion tempRot = parentCubeTransform.rotation;

            parentCubeTransform.position = Vector3.zero;
            parentCubeTransform.rotation = Quaternion.identity;

            //foreach (GameObject gameobject in walls)
            for (int i = 0; i < walls.Count; i++)
            {
                walls[i].transform.parent = null;
                walls[i].GetComponent<Renderer>().material = facadeMaterial;
                tilingPlane(walls[i].transform, 1);
                if (walls[i].transform.childCount != 0)
                {
                    walls[i].transform.Clear();
                }
                if(walls[i].tag == "withWindows")
                {
                    instantWindows(walls[i], true);
                }
                else
                {
                    instantWindows(walls[i], false);
                }

                //walls[i].GetComponent<Renderer>().material = Resources.Load(CreateAsset(walls[i].GetComponent<Renderer>().material, "Facade")) as Material;
                walls[i].GetComponent<Renderer>().material = Resources.Load(CreateAsset(walls[i].GetComponent<Renderer>().sharedMaterial, "Facade")) as Material;
                walls[i].transform.parent = parentCubeTransform.transform;
            }
            parentCubeTransform.position = tempPos;
            parentCubeTransform.rotation = tempRot;


            //fundamenty
            if (parentCubeTransform.Find("Fundament") != null)
            {
                Object.DestroyImmediate(parentCubeTransform.Find("Fundament"));
            }
            GameObject fundament = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fundament.transform.localScale = new Vector3(width + 0.1f, 0.03f, length + 0.1f);
            var fundamentPos = new Vector3(parentCubeTransform.position.x, -parentCubeTransform.lossyScale.y / 2, parentCubeTransform.position.z);
            fundament.transform.position = fundamentPos;
            fundament.name = "fundament";
            fundament.tag = "conc";

            //roof
            if (parentCubeTransform.FindChild("Top") != null)
            {
                Object.DestroyImmediate(parentCubeTransform.FindChild("Top").gameObject);
            }
            var posit = new Vector3(parentCubeTransform.position.x, (parentCubeTransform.localScale.y + fundament.transform.localScale.y), parentCubeTransform.position.z);
            GameObject roofInstance = (GameObject)Object.Instantiate(roofType, posit, parentCubeTransform.rotation);



            float roofHeight = Mathf.Pow(2, 4 / height);

            if (roofInstance.name == "Gable(Clone)")
            {
                GameObject[] gameObjs = new GameObject[2];
                gameObjs[0] = roofInstance.transform.FindChild("sidewall_1").gameObject;
                gameObjs[1] = roofInstance.transform.FindChild("sidewall_2").gameObject;

                //Ustawianie materiałów sidewalli                     
                FindChildAndAssignMaterial(gameObjs, roofHeight);

                //zmiana tekstury dachu
                GameObject roof = roofInstance.transform.FindChild("gableroof").gameObject;
                roof.tag = "gableroof";
                //roof.GetComponent<Renderer>().materials[1].SetTexture("_MainTex", roofTex);
                roof.GetComponent<Renderer>().sharedMaterials[1].SetTexture("_MainTex", roofTex);

            }

            else if (roofInstance.name == "Flat(Clone)")
            {
                //adds roof which looks nice
                string taggg = "conc";
                var tempposi = parentCubeTransform.position;
                var temproti = parentCubeTransform.rotation;
                parentCubeTransform.position = Vector3.zero;
                parentCubeTransform.rotation = Quaternion.identity;
                GameObject elD01;
                GameObject elD02;
                GameObject elD03;
                GameObject elD04;

                GameObject cubik = GameObject.CreatePrimitive(PrimitiveType.Cube);
                

                float heightPlus = 0.5f * (height + fundament.transform.lossyScale.y);
                Vector3 elPos = new Vector3(0, heightPlus, (float)length / 2);
                elD01 = (GameObject)Object.Instantiate(cubik, elPos, parentCubeTransform.rotation);
                elD01.transform.localScale = new Vector3(width + 0.1f, 0.2f, 0.1f);
                elD01.name = "elD01";
                elD01.tag = taggg;

                elPos = new Vector3(0, heightPlus, -(float)length / 2);
                elD02 = (GameObject)Object.Instantiate(cubik, elPos, parentCubeTransform.rotation);
                elD02.transform.localScale = new Vector3(width + 0.1f, 0.2f, 0.1f);
                elD02.name = "elD02";
                elD02.tag = taggg;

                elPos = new Vector3((float)width / 2, heightPlus, 0);
                elD03 = (GameObject)Object.Instantiate(cubik, elPos, parentCubeTransform.rotation);
                elD03.transform.localScale = new Vector3(0.1f, 0.2f, length - 0.1f);
                elD03.name = "elD03";
                elD03.tag = taggg;

                elPos = new Vector3(-(float)width / 2, heightPlus, 0);
                elD04 = (GameObject)Object.Instantiate(cubik, elPos, parentCubeTransform.rotation);
                elD04.transform.localScale = new Vector3(0.1f, 0.2f, length - 0.1f);
                elD04.name = "elD04";
                elD04.tag = taggg;

                Object.DestroyImmediate(cubik);

                elD01.transform.parent = parentCubeTransform;
                elD02.transform.parent = parentCubeTransform;
                elD03.transform.parent = parentCubeTransform;
                elD04.transform.parent = parentCubeTransform;

                parentCubeTransform.position = tempposi;
                parentCubeTransform.rotation = temproti;

                roofInstance.transform.GetChild(0).gameObject.tag = taggg;
            }
            roofInstance.tag = ("roof");
            roofInstance.name = ("Top");

            roofInstance.transform.localScale = new Vector3(width, roofHeight, length);
            fundament.transform.parent = parentCubeTransform;
            parentCubeTransform.position = new Vector3(parentCubeTransform.position.x, fundament.transform.lossyScale.y + parentCubeTransform.localScale.y / 2, parentCubeTransform.position.z);
            roofInstance.transform.parent = parentCubeTransform;

            //Use concrete material on roof
            GameObject[] concreteObjects = GameObject.FindGameObjectsWithTag("conc");
            foreach (GameObject gameObj in concreteObjects)
            {
                gameObj.GetComponent<Renderer>().material = concrete;              
            }
        }

        private void FindChildAndAssignMaterial(GameObject[] gameobjs, float roofH)
        {
            foreach (GameObject gameObj in gameobjs)
            {
                Renderer rendi = gameObj.GetComponent<Renderer>();
                rendi.material = facadeMaterial;
                rendi.material.color = randomColor;
                if (rendi.material.name == "Brick 1 (Instance)")
                {
                    tilingRoofTriangle(gameObj.transform, new Vector2(0.25f, 0), roofH / 2);
                }
                if (rendi.material.name == "Brick 2 (Instance)")
                {
                    tilingRoofTriangle(gameObj.transform, new Vector2(0.19f, 0), roofH / 2);
                }
                else
                {
                    tilingRoofTriangle(gameObj.transform, new Vector2(0.5f, 0), roofH / 2);
                }
                //gameObj.GetComponent<Renderer>().material = Resources.Load(CreateAsset(gameObj.GetComponent<Renderer>().material, "sidewall")) as Material;
                gameObj.GetComponent<Renderer>().material = Resources.Load(CreateAsset(gameObj.GetComponent<Renderer>().sharedMaterial, "sidewall")) as Material;
            }
        }
        private float[] getTable(float scale)
        {
            int skalaInt = (int)scale + 1;
            float[] returnTable = new float[(int)scale];
            float windowsDist = scale / skalaInt;
            for (int i = 1; i <= (int)scale; i++)
            {
                returnTable[i - 1] = windowsDist * i;
            }
            return returnTable;

        }

        private List<Vector3> addWindowsToList(float[] xx, float[] yy)
        {
            var lista = new List<Vector3>();
            for (int j = 0; j < xx.Length; j++)
                for (int i = 0; i < yy.Length; i++)
                {
                    lista.Add(new Vector3(xx[j], yy[i]));
                }
            return lista;
        }
        private void instantWindows(GameObject gameOb, bool hasDoor)
        {
            Vector3 tempPosGo = gameOb.transform.position;
            Quaternion tempRotGo = gameOb.transform.rotation;

            gameOb.transform.position = Vector3.zero;
            gameOb.transform.rotation = Quaternion.identity;

            float x = gameOb.transform.lossyScale.x;
            float y = gameOb.transform.lossyScale.y;


            var ys = getTable(y);

            var windowCoords = addWindowsToList(getTable(x), ys);
            if (hasDoor)
            {
                var posDoor = doorInTheMiddle(windowCoords, ys);
                var doorPosition = new Vector3(posDoor.x - x / 2, -y / 2);

                GameObject tempDoor = Object.Instantiate(doorPrefab);
                tempDoor.transform.position = doorPosition;
                tempDoor.transform.parent = gameOb.transform;

            }
            for (int i = 0; i < windowCoords.Count; i++)
            {
                Vector3 posWindow = new Vector3(windowCoords[i].x - x / 2, windowCoords[i].y - y / 2);

                GameObject tempWindow = (GameObject)Object.Instantiate(windowPrefab, posWindow, Quaternion.identity);
                tempWindow.transform.localScale = tempWindow.transform.lossyScale * (Mathf.Pow(Mathf.Log(height), 0.25f));
                tempWindow.transform.parent = gameOb.transform;
            }

            gameOb.transform.position = tempPosGo;
            gameOb.transform.rotation = tempRotGo;

        }
        //Dodawanie drzwi w środku budynku
        private Vector3 doorInTheMiddle(List<Vector3> wCoords, float[] ys)
        {
            Vector3 returnVec = Vector3.zero;

            //1 - find min y            
            float miny = Mathf.Min(ys);
            var possibleDoors = new List<Vector3>();

            //2 - possible doors will be a list of min Y locations
            for (int i = 0; i < wCoords.Count; i++)
            {
                if (Mathf.Abs(wCoords[i].y - miny) <= Mathf.Epsilon)
                {
                    possibleDoors.Add(wCoords[i]);
                }
            }
            //3 nieparzysta ilość okien
            if (possibleDoors.Count % 2 == 1)
            {
                var doorLoc = possibleDoors[(int)possibleDoors.Count / 2];
                wCoords.Remove(doorLoc);

                returnVec = doorLoc;
            }
            else //parzysta ilość okien
            {
                var doorLoc1 = possibleDoors[((int)possibleDoors.Count / 2) - 1];
                var doorLoc2 = possibleDoors[((int)possibleDoors.Count / 2)];

                wCoords.Remove(doorLoc1);
                wCoords.Remove(doorLoc2);

                returnVec = new Vector3((doorLoc1.x + doorLoc2.x) / 2, doorLoc2.y, doorLoc2.z);
            }

            return returnVec;

        }
        private string CreateAsset(Material mat, string materialType)
        {
            Material facMat = new Material(mat);
            facMat.color = randomColor;
            int instanceID = facMat.GetInstanceID();
            matName = materialType + instanceID;
            string pathh = "Assets/Resources/GeneratedMaterials/" + matName + ".mat";
            AssetDatabase.CreateAsset(facMat, pathh);
            //GameObject.DestroyImmediate(mat);
            //GameObject.DestroyImmediate(facMat);
            return "GeneratedMaterials/" + matName;
        }
        //Funkcja dopasowująca skalę tekstury
        private void tilingPlane(Transform transform, float tiling)
        {
            //Material mat = transform.GetComponent<Renderer>().material;
            Material mat = transform.GetComponent<Renderer>().sharedMaterial;

            float scaleX = transform.lossyScale.x;
            float scaleY = transform.lossyScale.y;

            Vector2 scale = new Vector2(scaleX * tiling, scaleY * tiling);
            mat.SetTextureScale("_MainTex", scale);

        }
        //Funkcja dostosowująca skalę tekstury trójkątnej części dachu
        private void tilingRoofTriangle(Transform transform, Vector2 offset, float scaleY)
        {
            //Material mat = transform.GetComponent<Renderer>().material;
            Material mat = transform.GetComponent<Renderer>().sharedMaterial;
            float scaleX = length;

            mat.SetTextureScale("_MainTex", new Vector2(scaleX, scaleY));
            mat.SetTextureOffset("_MainTex", offset);
        }
        private void CreateRoofMats()
        {
            GameObject[] gables = GameObject.FindGameObjectsWithTag("gableroof");
            foreach (GameObject gable in gables)
            {
                //string asset1 = CreateAsset(gable.GetComponent<Renderer>().materials[0], "Roof");
                //string asset2 = CreateAsset(gable.GetComponent<Renderer>().materials[1], "Roof");

                string asset1 = CreateAsset(gable.GetComponent<Renderer>().sharedMaterials[0], "Roof");
                string asset2 = CreateAsset(gable.GetComponent<Renderer>().sharedMaterials[1], "Roof");
                Material[] matsy = new Material[2];
                matsy[0] = Resources.Load(asset1) as Material;
                matsy[1] = Resources.Load(asset2) as Material;

                gable.GetComponent<Renderer>().materials = matsy;

            }
        }      
    }

}
