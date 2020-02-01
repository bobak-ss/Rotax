using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameJab2020
{
    public class GameController : MonoBehaviour
    {
        public Material fixedObjectMat;
        public Transform[] objectsT;
        public List<Transform> levelObjsT;
        public float speed;
        public float startRotationForce;
        public float roundFactor;
        public float checkForWinForgiveness;
        public Text gameText;
        public bool easyControll;
        public GameState state;

        private Vector3 mouseInitialPos;
        private Quaternion[] correctRotations;
        private Vector3 sampleParentFinalPos;
        private Vector3 sampleParentFinalScale;
        private Vector3 startAnimationRandomRotation;
        private int randomAnimationCounter = 1;
        private Vector3 diffrentToWin;
        private int level = 1;
        private int percentToRecreate;
        private bool sampleAnimating = false;
        private GameObject sampleParent;
        private bool winAnimation = true;
        private bool goToNextLevelAnim = false;
        private bool newObjectAddAnim = false;

        void Start()
        {
            InitiateLevel();
        }

        private void InitiateLevel(Transform go = null)
        {
            // int objectsNum = Math.Max(2, level);
            int objectsNum = (go == null) ? 2 : UnityEngine.Random.Range(1, 4);
            levelObjsT.Clear();
            int randomIndex = UnityEngine.Random.Range(0, objectsT.Length);
            for (int i = 0; i < objectsT.Length; i++)
            {
                objectsT[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < objectsNum; i++)
            {
                levelObjsT.Add(objectsT[(i + randomIndex > objectsT.Length - 1) ? i + randomIndex - objectsT.Length : i + randomIndex]);
                levelObjsT[i].Rotate(UnityEngine.Random.Range(0, 180), UnityEngine.Random.Range(0, 180), UnityEngine.Random.Range(0, 180));
                levelObjsT[i].gameObject.SetActive(true);
                if (go != null)
                    levelObjsT[i].localScale *= 0.1f;
            }
            if (go != null)
            {
                levelObjsT.Add(go);
                newObjectAddAnim = true;
            }
        }

        void Update()
        {
            switch (state)
            {
                case GameState.InMenu:
                    
                    if (sampleAnimating)
                    {
                        sampleParent.transform.position = Vector3.Lerp(sampleParent.transform.position, sampleParentFinalPos, Time.deltaTime * 2);
                        sampleParent.transform.localScale = Vector3.Lerp(sampleParent.transform.localScale, sampleParentFinalScale, Time.deltaTime * 2);
                        if (Vector3.Distance(sampleParent.transform.position, sampleParentFinalPos) < 0.3f)
                        {
                            sampleParent.transform.position = sampleParentFinalPos;
                            sampleAnimating = false;
                            state = GameState.Start;
                            randomAnimationCounter = 1;
                        }
                    }
                    else
                    {
                        gameText.text = "Rotate to Repair:\nTap to Start!";
                        gameText.color = Color.white;
                        gameText.fontSize = 25;
                        gameText.fontStyle = FontStyle.Normal;
                        if (Input.anyKey)
                        {
                            sampleParent = new GameObject();
                            for (int i = 0; i < levelObjsT.Count; i++)
                            {
                                GameObject.Instantiate(levelObjsT[i], sampleParent.transform);
                            }
                            sampleParentFinalPos = sampleParent.transform.position + new Vector3(-5.5f, 5f, 0f);
                            sampleParentFinalScale = sampleParent.transform.localScale / 3;
                            startAnimationRandomRotation = new Vector3(UnityEngine.Random.Range(-1, 2) * UnityEngine.Random.Range(100, 200) * Time.deltaTime * startRotationForce, 
                                                                        UnityEngine.Random.Range(-1, 2) * UnityEngine.Random.Range(100, 200) * Time.deltaTime * startRotationForce, 0);
                            sampleAnimating = true;
                        }
                    }
                    break;
                case GameState.Start:
                    if (randomAnimationCounter > 0)
                    {
                        Debug.Log("Saving rotations!");
                        correctRotations = new Quaternion[levelObjsT.Count];
                        for (int i = 0; i < levelObjsT.Count; i++)
                            correctRotations[i] = levelObjsT[i].rotation;
                        StartCoroutine(GoToInGameState());
                    }
                    RotateShapes(startAnimationRandomRotation);
                    break;
                case GameState.InGame:

                    // percentToRecreate = (100 - Mathf.RoundToInt(Vector3.Distance(EularChanger(levelObjsT[0].eulerAngles), EularChanger(correctRotations[0].eulerAngles)) / 
                    //                     Vector3.Distance(EularChanger(correctRotations[0].eulerAngles + new Vector3(360, 360, 360)), EularChanger(correctRotations[0].eulerAngles)) * 100));
                    percentToRecreate =  Mathf.RoundToInt((180f - Quaternion.Angle(levelObjsT[0].rotation, correctRotations[0])) / 180f * 100f);
                    gameText.text = "Rotate to Repair:\n" + percentToRecreate + "%";

                    if (easyControll)
                    {
                        if (Input.GetMouseButton(0))
                        {
                            for (int i = 0; i < levelObjsT.Count; i++)
                            {
                                levelObjsT[i].rotation = Quaternion.Lerp(levelObjsT[i].rotation, correctRotations[i], 
                                                                    Time.deltaTime * 1f * Vector3.Magnitude(new Vector3(Mathf.Round(Input.GetAxis("Mouse Y")), Mathf.Round(Input.GetAxis("Mouse X")), 0)));
                            }
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButton(0))
                        {
                            RotateShapes(new Vector3((Input.GetAxis("Mouse Y")), (Input.GetAxis("Mouse X")), 0) * Time.deltaTime * speed);
                        }
                    }
                    if (CheckForWin())
                    {
                        Debug.Log("WIN!!!");
                        winAnimation = true;
                        state = GameState.Win;
                    }
                    break;
                case GameState.Win:
                

                    if (winAnimation)
                    {
                        // if (Mathf.Abs(Quaternion.Dot(levelObjsT[0].rotation, correctRotations[0])) < 1 - 0.001)
                        // if (100 - percentToRecreate <= 1)
                        if (Quaternion.Angle(levelObjsT[0].rotation, correctRotations[0]) < 2)
                        {
                            for (int i = 0; i < levelObjsT.Count; i++)
                            {
                                levelObjsT[i].rotation = correctRotations[i];
                            }
                            winAnimation = false;
                            gameText.text = "100%\nWell Done!";
                            gameText.color = Color.green;
                            gameText.fontSize = 35;
                            gameText.fontStyle = FontStyle.BoldAndItalic;

                            level++;
                            Debug.Log("level: " + level);
                        }
                        else
                        {
                            // percentToRecreate = (100 - Mathf.RoundToInt(Vector3.Distance(EularChanger(levelObjsT[0].eulerAngles), correctRotations[0].eulerAngles) / 
                            //                     Vector3.Distance(EularChanger(correctRotations[0].eulerAngles + new Vector3(180, 180, 180)), correctRotations[0].eulerAngles) * 100));
                            percentToRecreate =  Mathf.RoundToInt((180f - Quaternion.Angle(levelObjsT[0].rotation, correctRotations[0])) / 180f * 100f);
                            gameText.text = "Rotate to Repair:\n" + percentToRecreate + "%";
                            for (int i = 0; i < levelObjsT.Count; i++)
                            {
                                levelObjsT[i].rotation = Quaternion.Lerp(levelObjsT[i].rotation, correctRotations[i], Time.deltaTime * 1f);
                            }
                            Debug.Log("wining: " + levelObjsT[0].eulerAngles);
                        }
                    }
                    else
                    {
                        if (goToNextLevelAnim)
                        {
                            sampleParent.transform.position = Vector3.Lerp(sampleParent.transform.position, sampleParentFinalPos, Time.deltaTime * 2);
                            sampleParent.transform.localScale = Vector3.Lerp(sampleParent.transform.localScale, sampleParentFinalScale, Time.deltaTime * 2);
                            if (Vector3.Distance(sampleParent.transform.position, sampleParentFinalPos) < 0.3f)
                            {
                                sampleParent.transform.position = sampleParentFinalPos;
                                goToNextLevelAnim = false;
                                // state = GameState.Start;
                                for (int i = 0; i < levelObjsT.Count; i++)
                                {
                                    Debug.Log(i);
                                    levelObjsT[i].gameObject.SetActive(false);
                                    // sampleParent.transform.GetChild(i).GetChild(0).GetComponent<MeshRenderer>().material = fixedObjectMat;
                                }
                                sampleAnimating = false;
                                InitiateLevel(sampleParent.transform);
                                
                                newObjectAddAnim = true;
                            }
                        }
                        else if (newObjectAddAnim)
                        {
                            for (int i = 0; i < levelObjsT.Count - 1; i++)
                            {
                                levelObjsT[i].localScale += new Vector3(Time.deltaTime * 1, Time.deltaTime * 1, Time.deltaTime * 1);
                            }
                            if (levelObjsT[0].localScale.x >= 1.1)
                            {
                                newObjectAddAnim = false;
                                state = GameState.InMenu;
                            }
                        }
                        else if (Input.GetMouseButtonDown(0))
                        {
                            goToNextLevelAnim = true;
                            sampleParentFinalPos = sampleParent.transform.position - new Vector3(-5.5f, 5f, 0f);
                            sampleParentFinalScale = sampleParent.transform.localScale * 3;
                            for (int i = 0; i < levelObjsT.Count; i++)
                                sampleParent.transform.GetChild(i).GetChild(0).GetComponent<MeshRenderer>().material = fixedObjectMat;
                        }
                    }
                    break;
                default:
                    break;
            }

            if (Input.GetKey(KeyCode.Escape))
            {
                SceneManager.LoadScene(0);
            }
        }

        public void QuickRepair()
        {
            diffrentToWin = new Vector3((levelObjsT[0].eulerAngles.x < checkForWinForgiveness) ? levelObjsT[0].eulerAngles.x : (levelObjsT[0].eulerAngles.x > 360 - checkForWinForgiveness) ? levelObjsT[0].eulerAngles.x - 360 : levelObjsT[0].eulerAngles.x, 
                                            (levelObjsT[0].eulerAngles.y < checkForWinForgiveness) ? levelObjsT[0].eulerAngles.y : (levelObjsT[0].eulerAngles.y > 360 - checkForWinForgiveness) ? levelObjsT[0].eulerAngles.y - 360 : levelObjsT[0].eulerAngles.x,
                                            (levelObjsT[0].eulerAngles.z < checkForWinForgiveness) ? levelObjsT[0].eulerAngles.z : (levelObjsT[0].eulerAngles.z > 360 - checkForWinForgiveness) ? levelObjsT[0].eulerAngles.z - 360 : levelObjsT[0].eulerAngles.x);
            Debug.Log("diff to win: " + diffrentToWin);
            winAnimation = true;
            state = GameState.Win;
        }

        private bool CheckForWin()
        {
            bool res = true;
            Debug.Log(levelObjsT[0].eulerAngles + " vs " + correctRotations[0].eulerAngles);
            // if ( (Mathf.Abs(levelObjsT[0].eulerAngles.x - correctRotations[0].x) > checkForWinForgiveness && Mathf.Abs(levelObjsT[0].eulerAngles.x - correctRotations[0].x) < 360 - checkForWinForgiveness) ||
            //         (Mathf.Abs(levelObjsT[0].eulerAngles.y - correctRotations[0].y) > checkForWinForgiveness && Mathf.Abs(levelObjsT[0].eulerAngles.y - correctRotations[0].y) < 360 - checkForWinForgiveness) ||
            //         (Mathf.Abs(levelObjsT[0].eulerAngles.z - correctRotations[0].z) > checkForWinForgiveness && Mathf.Abs(levelObjsT[0].eulerAngles.z - correctRotations[0].z) < 360 - checkForWinForgiveness) )
            // {
            //     res = false;
            // }
            if (100 - percentToRecreate > checkForWinForgiveness)
                res = false;
            else
            {
                res = true;
                diffrentToWin = new Vector3((levelObjsT[0].eulerAngles.x < checkForWinForgiveness) ? levelObjsT[0].eulerAngles.x : (levelObjsT[0].eulerAngles.x > 360 - checkForWinForgiveness) ? levelObjsT[0].eulerAngles.x - 360 : levelObjsT[0].eulerAngles.x, 
                                            (levelObjsT[0].eulerAngles.y < checkForWinForgiveness) ? levelObjsT[0].eulerAngles.y : (levelObjsT[0].eulerAngles.y > 360 - checkForWinForgiveness) ? levelObjsT[0].eulerAngles.y - 360 : levelObjsT[0].eulerAngles.x,
                                            (levelObjsT[0].eulerAngles.z < checkForWinForgiveness) ? levelObjsT[0].eulerAngles.z : (levelObjsT[0].eulerAngles.z > 360 - checkForWinForgiveness) ? levelObjsT[0].eulerAngles.z - 360 : levelObjsT[0].eulerAngles.x);
                Debug.Log("diff to win: " + diffrentToWin);
            }
            return res;
        }

        private Vector3 EularChanger(Vector3 eularDegree)
        {
            return new Vector3((eularDegree.x < 180) ? eularDegree.x : 360 - eularDegree.x,
                                (eularDegree.y < 180) ? eularDegree.y : 360 - eularDegree.y,
                                (eularDegree.z < 180) ? eularDegree.z : 360 - eularDegree.z);
        }

        IEnumerator GoToInGameState()
        {
            randomAnimationCounter--;
            yield return new WaitForSeconds(1);
            state = GameState.InGame;
        }

        private void RotateShapes(Vector3 rotationValue)
        {
            for (int i = 0; i < levelObjsT.Count; i++)
            {
                levelObjsT[i].transform.Rotate(rotationValue);
            }
        }

        public enum GameState
        {
            InMenu,
            Start,
            InGame,
            Win,

        }
    }
}
