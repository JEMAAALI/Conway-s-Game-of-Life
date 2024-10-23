using UnityEngine;

public class GameLifeControllerCPU : MonoBehaviour
{
    public int gridSize = 1024; // Size of the grid
    private bool[,] currentState, nextState;
    private Renderer renderer;

    private Texture2D gridTexture;

    private void Start()
    {
        // Initialize the grid
        currentState = new bool[gridSize, gridSize];
        nextState = new bool[gridSize, gridSize];
        renderer = GetComponent<Renderer>();

        // Create a texture to display the grid
        gridTexture = new Texture2D(gridSize, gridSize);
        renderer.material.mainTexture = gridTexture;

        // Randomly initialize the grid
        InitializeRandomState();

        // Draw the initial state
        DrawGrid();
    }

    private void DrawGrid()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                // Set pixel color based on cell state
                gridTexture.SetPixel(x, y, currentState[x, y] ? Color.white : Color.black);
            }
        }
        gridTexture.Apply(); // Apply changes to the texture
    }

    void Update()
    {
        // Run the Game of Life update
        RunGameOfLife();

        // Handle user input to add cells
        if (Input.GetMouseButton(0)) // Check if the left mouse button is held down
        {
            AddCellsAtMousePosition();
        }

        // Draw the current state
        DrawGrid();
    }

    private void RunGameOfLife()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                int liveNeighbors = CountLiveNeighbors(x, y);
                bool isAlive = currentState[x, y];

                // Apply the rules of the Game of Life
                if (isAlive && (liveNeighbors == 2 || liveNeighbors == 3))
                {
                    nextState[x, y] = true; // Stay alive
                }
                else if (!isAlive && liveNeighbors == 3)
                {
                    nextState[x, y] = true; // Come to life
                }
                else
                {
                    nextState[x, y] = false; // Stay dead
                }
            }
        }

        // Swap the current and next states
        var temp = currentState;
        currentState = nextState;
        nextState = temp;
    }

    private int CountLiveNeighbors(int x, int y)
    {
        int count = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue; // Skip the cell itself
                int neighborX = (x + i + gridSize) % gridSize; // Wrap around
                int neighborY = (y + j + gridSize) % gridSize; // Wrap around
                count += currentState[neighborX, neighborY] ? 1 : 0;
            }
        }
        return count;
    }

    private void InitializeRandomState()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                currentState[x, y] = Random.value > 0.5f; // Randomly set cells to alive or dead
            }
        }
    }

    private void AddCellsAtMousePosition()
    {
        Vector2 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector2 uv = hit.textureCoord;

            int x = (int)(uv.x * gridSize);
            int y = (int)(uv.y * gridSize);

            // Set the clicked cell and its surrounding cells to alive
            int brushSize = 10; // Size of the brush for adding live cells
            for (int i = -brushSize; i <= brushSize; i++)
            {
                for (int j = -brushSize; j <= brushSize; j++)
                {
                    int newX = Mathf.Clamp(x + i, 0, gridSize - 1);
                    int newY = Mathf.Clamp(y + j, 0, gridSize - 1);
                    currentState[newX, newY] = true; // Set the cell to alive
                }
            }
        }
    }

     
}
