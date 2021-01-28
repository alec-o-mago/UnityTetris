using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TetrisBlock : MonoBehaviour
{
    public Vector3 rotationPoint;
    private float previousTime;
    public float fallTime = 0.8f;
    public static int height = 20;
    public static int width = 10;
    private static Transform[,] grid = new Transform[width, height];
    //Swipe variables
    private float maxSwipeTime = 1000;
    private float minSwipeDistance = 150;
    private float swipeStartTime;
    private float swipeEndTime;
    private float swipePassedTime;
    private Vector2 startSwipePosition;
    private Vector2 endSwipePosition;
    private float SwipeLength;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SwipeTest();//Touch screen input
        //Keyboard Input
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveRight();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Rotate();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveDown();
        }

        if (Time.time - previousTime > fallTime)
        {
            transform.position += new Vector3(0, -1, 0);
            if (!ValidMove()) //Finish move
            {
                transform.position -= new Vector3(0, -1, 0);
                AddToGrid();
                this.enabled = false;
                CheckLines();
                CheckGameOver();
                FindObjectOfType<SpawnTetromino>().NewTetromino();
            }
            previousTime = Time.time;
        }
    }

    bool ValidMove()
    {
        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            if (roundedX < 0 || roundedX >= width || roundedY < 0 || roundedY >= height)
            {
                return false;
            }

            if (grid[roundedX, roundedY] != null)
            {
                return false;
            }
        }

        return true;
    }

    void AddToGrid()
    {
        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);
            grid[roundedX, roundedY] = children;
        }
    }

    void CheckLines()
    {
        for (int i = height-1; i>=0; i--)
        {
            bool fullLine = true;
            for (int j = 0; j < width; j++)
            {
                if (grid[j, i] == null)
                    fullLine = false;
            }
            if (fullLine)
            {
                FindObjectOfType<GameManager>().AddScore(10);
                DeleteLine(i);
                RowDown(i);
            }
        }
    }

    void DeleteLine(int i)
    {
        for (int j = 0; j < width; j++)
        {
            Destroy(grid[j, i].gameObject);
            grid[j, i] = null;
        }
    }

    void RowDown(int i)
    {
        for (int y = i; y<height;y++)
        {
            for (int j = 0; j<width; j++)
            {
                if(grid[j,y] != null)
                {
                    grid[j, y - 1] = grid[j, y];
                    grid[j, y] = null;
                    grid[j, y - 1].transform.position -= new Vector3(0, 1, 0);
                }
            }
        }
    }

    void SwipeTest()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                swipeStartTime = Time.time;
                startSwipePosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                swipeEndTime = Time.time;
                endSwipePosition = touch.position;
                swipePassedTime = swipeEndTime - swipeStartTime;
                SwipeLength = (endSwipePosition - startSwipePosition).magnitude;
            }
            if (swipePassedTime < maxSwipeTime && SwipeLength > minSwipeDistance)
            {
                SwipeControl();
            }
        }
    }

    void SwipeControl()
    {
        Vector2 Distance = endSwipePosition - startSwipePosition;
        float xDistance = Mathf.Abs(Distance.x);
        float yDistance = Mathf.Abs(Distance.y);
        if (xDistance > yDistance)
        {
            if (Distance.x > 0)
            {
                MoveRight();
            }
            else
            {
                MoveLeft();
            }
        }
        else
        {
            if (Distance.y > 0)
            {
                Rotate();
            }
            else
            {
                MoveDown();
            }
        }
    }

    void MoveLeft()
    {
        transform.position += new Vector3(-1, 0, 0);
        if (!ValidMove())
            transform.position += new Vector3(1, 0, 0);
    }

    void MoveRight()
    {
        transform.position += new Vector3(1, 0, 0);
        if (!ValidMove())
            transform.position += new Vector3(-1, 0, 0);
    }

    void Rotate()
    {
        transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
        if (!ValidMove())//If move was invalid, rollback
            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
    }

    void MoveDown()
    {
        while (ValidMove())
        {
            transform.position += new Vector3(0, -1, 0);
        }
        transform.position += new Vector3(0, 1, 0);
    }
    
    void CheckGameOver()
    {
        for (int i = 0; i < width; i++)
        {
            if (grid[i, height-2] != null)//if there is a piece at the top
            {
                SceneManager.LoadScene("Scenes/GameOver");
            }
        }
    }
}
