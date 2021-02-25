using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[Serializable]
class BlockData
{
    public Block.BlockType[,,] matrix;
    public BlockData() { }
    public BlockData(Block[,,] b)
    {
        matrix = new Block.BlockType[World.chunkSize, World.chunkSize, World.chunkSize];
        for (int z = 0; z < World.chunkSize; z++)
        {
            for (int y = 0; y < World.chunkSize; y++)
            {
                for (int x = 0; x < World.chunkSize; x++)
                {
                    matrix[x, y, z] = b[x, y, z].blockType;
                }
            }
        }
    }
}

public class Chunk
{
    public enum ChunkStatus { draw, done, keep };

    public Material chunkMaterial;
    public Material fluidMaterial;

    public Block[,,] chunkBlocks;
    public GameObject chunk;
    public GameObject fluid;
    public ChunkMB chunkMB;
    public bool changed = false;

    public ChunkStatus status;

    private BlockData _blockData;
    private bool _treesCreated = false;

    public Chunk(Vector3 position, Material chunkMat, Material fluidMat)
    {
        chunk = new GameObject(World.BuildChunkName(position));
        chunk.transform.position = position;

        fluid = new GameObject(World.BuildChunkName(position) + "_F");
        fluid.transform.position = position;

        chunkMB = chunk.AddComponent<ChunkMB>();
        chunkMB.SetOwner(this);
        chunkMaterial = chunkMat;
        fluidMaterial = fluidMat;

        BuildChunk();
    }

    private string BuildChunkFileName(Vector3 v)
    {
        return Application.dataPath + "/SavedData/Chunk_" + (int)v.x + "_" + (int)v.y + "_" + (int)v.z + "_" + World.chunkSize + "_" + World.radious + ".dat";
    }

    private bool Load()
    {
        string chunkFileName = BuildChunkFileName(chunk.transform.position);

        if (File.Exists(chunkFileName))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(chunkFileName, FileMode.Open);
            _blockData = new BlockData();
            _blockData = (BlockData)bf.Deserialize(fs);
            fs.Close();
            return true;
        }

        return false;
    }

    public void Save()
    {
        string chunkFileName = BuildChunkFileName(chunk.transform.position);
        if (!File.Exists(chunkFileName))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(chunkFileName));
        }
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = File.Open(chunkFileName, FileMode.OpenOrCreate);
        _blockData = new BlockData(chunkBlocks);
        bf.Serialize(fs, _blockData);
        fs.Close();
    }

    public void UpdateChunk()
    {
        for (int z = 0; z < World.chunkSize; z++)
        {
            for (int y = 0; y < World.chunkSize; y++)
            {
                for (int x = 0; x < World.chunkSize; x++)
                {
                    if (chunkBlocks[x, y, z].blockType == Block.BlockType.sand)
                    {
                        chunkMB.StartCoroutine(chunkMB.Drop(chunkBlocks[x, y, z], Block.BlockType.sand, 20));
                    }
                }
            }
        }
    }

    private void BuildChunk()
    {
        bool dataFromFile = Load();

        chunkBlocks = new Block[World.chunkSize, World.chunkSize, World.chunkSize];

        for (int z = 0; z < World.chunkSize; z++)
        {
            for (int y = 0; y < World.chunkSize; y++)
            {
                for (int x = 0; x < World.chunkSize; x++)
                {
                    Vector3 pos = new Vector3(x, y, z);

                    if (dataFromFile)
                    {
                        chunkBlocks[x, y, z] = new Block(_blockData.matrix[x, y, z], pos, chunk, this);
                        continue;
                    }

                    int worldX = (int)(x + chunk.transform.position.x);
                    int worldY = (int)(y + chunk.transform.position.y);
                    int worldZ = (int)(z + chunk.transform.position.z);

                    int surfaceHeight = Utils.GenerateHeight(worldX, worldZ);


                    if (worldY == 0)
                        chunkBlocks[x, y, z] = new Block(Block.BlockType.bedrock, pos, chunk, this);

                    else if (worldY <= Utils.GenerateStoneHeight(worldX, worldZ))
                    {
                        if (Utils.FBM3D(worldX, worldY, worldZ, 0.01f, 2) < 0.4f && worldY < 40)
                            chunkBlocks[x, y, z] = new Block(Block.BlockType.diamond, pos, chunk, this);
                        else if (Utils.FBM3D(worldX, worldY, worldZ, 0.03f, 3) < 0.41f && worldY < 20)
                            chunkBlocks[x, y, z] = new Block(Block.BlockType.redstone, pos, chunk, this);
                        else
                            chunkBlocks[x, y, z] = new Block(Block.BlockType.stone, pos, chunk, this);
                    }

                    else if (worldY == surfaceHeight)
                    {
                        if (Utils.FBM3D(worldX, worldY, worldZ, 0.4f, 2) < 0.4f)
                        {
                            if (worldY > 70)
                                chunkBlocks[x, y, z] = new Block(Block.BlockType.pinewoodbase, pos, chunk, this);
                            else
                                chunkBlocks[x, y, z] = new Block(Block.BlockType.woodbase, pos, chunk, this);
                        }
                        else if (Utils.FBM3D(worldX, worldY, worldZ, 0.1f, 2) < 0.42f)
                            chunkBlocks[x, y, z] = new Block(Block.BlockType.sand, pos, chunk, this);
                        else if (Utils.FBM3D(worldX, worldY, worldZ, 0.1f, 2) < 0.45f)
                            chunkBlocks[x, y, z] = new Block(Block.BlockType.snow, pos, chunk, this);
                        else
                            chunkBlocks[x, y, z] = new Block(Block.BlockType.grass, pos, chunk, this);
                    }

                    else if (worldY < surfaceHeight)
                    {
                        if (Utils.FBM3D(worldX, worldY, worldZ, 0.1f, 2) < 0.45f)
                            chunkBlocks[x, y, z] = new Block(Block.BlockType.sand, pos, chunk, this);
                        else
                            chunkBlocks[x, y, z] = new Block(Block.BlockType.dirt, pos, chunk, this);
                    }

                    else if (worldY < 65)
                        chunkBlocks[x, y, z] = new Block(Block.BlockType.water, pos, fluid, this);

                    else
                        chunkBlocks[x, y, z] = new Block(Block.BlockType.air, pos, chunk, this);

                    if (chunkBlocks[x, y, z].blockType != Block.BlockType.water && Utils.FBM3D(worldX, worldY, worldZ, 0.1f, 3) < 0.42f)
                        chunkBlocks[x, y, z] = new Block(Block.BlockType.air, pos, chunk, this);


                    status = ChunkStatus.draw;
                }
            }
        }
    }

    public void ReDraw()
    {
        GameObject.DestroyImmediate(chunk.GetComponent<MeshFilter>());
        GameObject.DestroyImmediate(chunk.GetComponent<MeshRenderer>());
        GameObject.DestroyImmediate(chunk.GetComponent<MeshCollider>());
        GameObject.DestroyImmediate(fluid.GetComponent<MeshFilter>());
        GameObject.DestroyImmediate(fluid.GetComponent<MeshRenderer>());
        GameObject.DestroyImmediate(fluid.GetComponent<UVScroller>());

        DrawChunk();
    }

    public void DrawChunk()
    {
        if (!_treesCreated)
        {
            for (int z = 0; z < World.chunkSize; z++)
            {
                for (int y = 0; y < World.chunkSize; y++)
                {
                    for (int x = 0; x < World.chunkSize; x++)
                    {
                        BuildTrees(chunkBlocks[x, y, z], x, y, z);
                    }
                }
            }
            _treesCreated = true;
        }

        for (int z = 0; z < World.chunkSize; z++)
        {
            for (int y = 0; y < World.chunkSize; y++)
            {
                for (int x = 0; x < World.chunkSize; x++)
                {
                    chunkBlocks[x, y, z].Draw();
                }
            }
        }

        CombineQuads(chunk, chunkMaterial);
        MeshCollider meshCollider = chunk.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = chunk.transform.GetComponent<MeshFilter>().mesh;

        CombineQuads(fluid, fluidMaterial);
        fluid.AddComponent<UVScroller>();

        status = ChunkStatus.done;
    }

    private void BuildTrees(Block trunk, int x, int y, int z)
    {
        if (trunk.blockType == Block.BlockType.woodbase)
            BuildTree(trunk, x, y, z);
        else if (trunk.blockType == Block.BlockType.pinewoodbase)
            BuildPineTree(trunk, x, y, z);
    }


    private void BuildTree(Block trunk, int x, int y, int z)
    {
        if (trunk.blockType != Block.BlockType.woodbase) return;

        Block b1 = trunk.GetBlock(x, y + 1, z);
        if (b1 != null)
        {
            b1.SetType(Block.BlockType.wood);

            Block b2 = b1.GetBlock(x, y + 2, z);
            if (b2 != null)
            {
                b2.SetType(Block.BlockType.wood);

                for (int i = -1; i <= 1; i++)
                    for (int j = -1; j <= 1; j++)
                        for (int k = 3; k <= 4; k++)
                        {
                            Block b3 = b2.GetBlock(x + i, y + k, z + j);
                            if (b3 != null)
                                b3.SetType(Block.BlockType.leaves);
                            else return;
                        }
                Block b4 = b2.GetBlock(x, y + 5, z);
                if (b4 != null)
                    b4.SetType(Block.BlockType.leaves);

            }
        }
    }

    private void BuildPineTree(Block trunk, int x, int y, int z)
    {
        if (trunk.blockType != Block.BlockType.pinewoodbase) return;

        Block b1 = trunk.GetBlock(x, y + 1, z);
        if (b1 != null)
        {
            b1.SetType(Block.BlockType.pinewood);

            Block b2 = b1.GetBlock(x, y + 2, z);
            if (b2 != null)
            {
                b2.SetType(Block.BlockType.pinewood);

                for (int i = -1; i <= 1; i++)
                    for (int j = -1; j <= 1; j++)
                        for (int k = 3; k <= 4; k++)
                        {
                            Block b3 = b2.GetBlock(x + i, y + k, z + j);
                            if (b3 != null)
                                b3.SetType(Block.BlockType.pineleaves);
                            else return;
                        }
                Block b4 = b2.GetBlock(x, y + 5, z);
                if (b4 != null)
                    b4.SetType(Block.BlockType.pineleaves);
            }
        }
    }

    private void CombineQuads(GameObject parent, Material material)
    {
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }

        MeshFilter meshFilter = parent.AddComponent(typeof(MeshFilter)) as MeshFilter;
        meshFilter.mesh = new Mesh();

        meshFilter.mesh.CombineMeshes(combine);

        MeshRenderer renderer = parent.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material = material;

        foreach (Transform quad in parent.transform)
        {
            GameObject.Destroy(quad.gameObject);
        }
    }
}
