using UnityEngine;

public class GameOfLifeControllerGPU_2k : MonoBehaviour
{
    public ComputeShader gameOfLifeCompute;
    public int textureSize = 2560;

    private RenderTexture currentState, nextState;
    private int kernelHandle;

 

    void Start()
    {
        // Initialize the textures
        currentState = CreateRenderTexture();
        nextState = CreateRenderTexture();
        GetComponent<Renderer>().material.mainTexture = currentState;

        // Randomly initialize the grid
        InitializeRandomState(currentState);

        // Get the compute shader kernel
        kernelHandle = gameOfLifeCompute.FindKernel("CSMain");

 
    }

    void Update()
    {
        // Run the compute shader
        RunComputeShader();

        // Handle user input to add cells
        if (Input.GetMouseButton(0)) // Check if the left mouse button is held down
        {
            AddCellsAtMousePosition();
        }

        // Swap textures
        Swap(ref currentState, ref nextState);
    }

    private void AddCellsAtMousePosition()
    {
        Vector2 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector2 uv = hit.textureCoord;

            int x = (int)(uv.x * textureSize);
            int y = (int)(uv.y * textureSize);

            // Pass the position to the compute shader
            gameOfLifeCompute.SetInt("mouseX", x);
            gameOfLifeCompute.SetInt("mouseY", y);

            // Define the brush size for adding live cells
            int brushSize = 10;
            gameOfLifeCompute.SetInt("brushSize", brushSize);

            // Use a separate kernel to handle mouse input and directly update the texture
            int addCellKernel = gameOfLifeCompute.FindKernel("AddCells");
            gameOfLifeCompute.SetTexture(addCellKernel, "NextState", nextState);
            gameOfLifeCompute.Dispatch(addCellKernel, brushSize / 8, brushSize / 8, 1);
        }
    }




    private void RunComputeShader()
    {
        gameOfLifeCompute.SetTexture(kernelHandle, "CurrentState", currentState);
        gameOfLifeCompute.SetTexture(kernelHandle, "Result", nextState);
        // Ensure that the number of groups dispatched matches the texture size and thread group size (16x16 in this case)
        gameOfLifeCompute.Dispatch(kernelHandle, textureSize / 16, textureSize / 16, 1);
    }

    private RenderTexture CreateRenderTexture()
    {
        RenderTexture rt = new RenderTexture(textureSize, textureSize, 0); 
        rt.enableRandomWrite = true; // Must be true for compute shaders
        rt.filterMode = FilterMode.Point; // Avoids smoothing, keeps pixelated look
        rt.wrapMode = TextureWrapMode.Repeat; // Wrap mode for texture
        rt.Create(); // Important to create the RenderTexture
        return rt;
    }

    private void InitializeRandomState(RenderTexture texture)
    {
        Texture2D randomTexture = new Texture2D(textureSize, textureSize);
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                randomTexture.SetPixel(x, y, Random.value > 0.5f ? Color.white : Color.black);
            }
        }
        randomTexture.Apply();
        Graphics.Blit(randomTexture, texture);
        Destroy(randomTexture);
    }

    private void Swap(ref RenderTexture a, ref RenderTexture b)
    {
        RenderTexture temp = a;
        a = b;
        b = temp;
    }
}
