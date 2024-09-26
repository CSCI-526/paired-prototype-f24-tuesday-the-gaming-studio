using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
    public TetrominoData[] tetrominoes;
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);

    public RectInt Bounds
    {
        get
        {
            
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < this.tetrominoes.Length; i++)
        {
            this.tetrominoes[i].Initialize();
        }
    }

    public void Start()
    {
        SpawnPiece();
    }

    public void SpawnPiece()
    {

        
        if (IsGameOver())
        {
            Debug.Log("Game ended"); 
            return; 
        }


     

        int random = Random.Range(0, this.tetrominoes.Length);
        TetrominoData data = this.tetrominoes[random];

        this.activePiece.Initialize(this, this.spawnPosition, data);
        Set(this.activePiece);

    }

    
    private bool IsGameOver()
    {
        int totalHeight = boardSize.y; 
        int filledRows = 0; 

        
        for (int y = -boardSize.y / 2; y < boardSize.y / 2; y++)
        {
            bool isRowFilled = true; 

            for (int x = -boardSize.x / 2; x < boardSize.x / 2; x++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0); 

                if (!tilemap.HasTile(tilePosition)) 
                {
                    isRowFilled = false; 
                    break; 
                }
            }

            
            if (isRowFilled)
            {
                filledRows++;
            }
            else
            {
                
                break;
            }
        }

        
        int remainingHeight = totalHeight - filledRows;
        Debug.Log($"Total height is: {totalHeight}");
        
        int thresholdHeight = Mathf.CeilToInt(0.7f * totalHeight);
        Debug.Log($"Threshold height is: {thresholdHeight}");
        
        Debug.Log($"Remaining height from the top: {remainingHeight}");

        
        return remainingHeight <= thresholdHeight;
    }




  

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }
    
    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null); 
        }
    }
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }
            
            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
            
        }
        return true;
    }

    
    public void CheckAndClearTiles(Piece piece)
    {
        

        List<Vector3Int> clearedTiles = new List<Vector3Int>();
        
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;

            
            Vector3Int[] directions = new Vector3Int[]
            {
                new Vector3Int(1, 0, 0), 
                new Vector3Int(-1, 0, 0), 
                new Vector3Int(0, 1, 0), 
                new Vector3Int(0, -1, 0) 
            };

            foreach (Vector3Int direction in directions)
            {
                Vector3Int neighborPosition = tilePosition + direction;

                if (tilemap.HasTile(neighborPosition))
                {
                    
                    Tetromino neighborTetromino = GetTetrominoAtPosition(neighborPosition);
                    Tetromino currentTetromino = piece.data.tetromino;

                    if (ShouldClearTile(currentTetromino, neighborTetromino))
                    {
                       
                        tilemap.SetTile(tilePosition, null);
                        tilemap.SetTile(neighborPosition, null);

                       

                        clearedTiles.Add(tilePosition);
                        clearedTiles.Add(neighborPosition);
                       
                    }
                }
            }
        }

        

        MoveTilesDown(clearedTiles);
      
    }

   
    private void MoveTilesDown(List<Vector3Int> clearedTiles)
    {
      
        clearedTiles.Sort((a, b) => b.y.CompareTo(a.y));

        
        HashSet<Vector3Int> processedTiles = new HashSet<Vector3Int>();

        foreach (Vector3Int tile in clearedTiles)
        {
            
            Vector3Int aboveTilePosition = tile + new Vector3Int(0, 1, 0);

            
            if (tilemap.HasTile(aboveTilePosition) && !processedTiles.Contains(aboveTilePosition))
            {
                
                TileBase tileToMove = tilemap.GetTile(aboveTilePosition);
                if (tileToMove != null)
                {
                    Vector3Int newPosition = tile + new Vector3Int(0, -1, 0);

                   
                    if (newPosition.y >= Bounds.min.y) 
                    {
                        tilemap.SetTile(newPosition, tileToMove); 
                        tilemap.SetTile(aboveTilePosition, null); 
                        processedTiles.Add(aboveTilePosition); 
                    }
                    else
                    {
                       
                        tilemap.SetTile(tile, null); 
                    }
                }
            }
        }

      
        bool hasMoved;
        do
        {
            hasMoved = false;

            foreach (Vector3Int tile in clearedTiles)
            {
                Vector3Int aboveTilePosition = tile + new Vector3Int(0, 1, 0);

                
                if (tilemap.HasTile(aboveTilePosition) && !processedTiles.Contains(aboveTilePosition))
                {
                    // Move the tile down
                    TileBase tileToMove = tilemap.GetTile(aboveTilePosition);
                    if (tileToMove != null)
                    {
                        Vector3Int newPosition = tile + new Vector3Int(0, -1, 0);

                       
                        if (newPosition.y >= Bounds.min.y) 
                        {
                            tilemap.SetTile(newPosition, tileToMove); 
                            tilemap.SetTile(aboveTilePosition, null); 
                            processedTiles.Add(aboveTilePosition); 
                            hasMoved = true; 
                        }
                        else
                        {
                           
                            tilemap.SetTile(tile, null); 
                        }
                    }
                }
            }
        } while (hasMoved); 
    }




   
    private bool ShouldClearTile(Tetromino current, Tetromino neighbor)
    {
        
        if ((current == Tetromino.I || current == Tetromino.J || current == Tetromino.S) && 
            (neighbor == Tetromino.T || neighbor == Tetromino.Z)) 
        {
            return true;
        }
        else if ((current == Tetromino.O || current == Tetromino.L) && 
                 (neighbor == Tetromino.I || neighbor == Tetromino.J || neighbor == Tetromino.S)) 
        {
            return true;
        }
        else if ((current == Tetromino.T || current == Tetromino.Z) && 
                 (neighbor == Tetromino.O || neighbor == Tetromino.L)) 
        {
            return true;
        }

        return false;
    }

   
    private Tetromino GetTetrominoAtPosition(Vector3Int position)
    {
        TileBase tile = tilemap.GetTile(position);

        foreach (TetrominoData data in tetrominoes)
        {
            if (data.tile == tile)
            {
                return data.tetromino;
            }
        }

        return Tetromino.I; 
    }

}




