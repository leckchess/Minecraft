using System;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    enum CubeSide { bottom, top, left, right, front, back };
    public enum BlockType { grass, snow, dirt, stone, woodbase, pinewoodbase, wood, pinewood, leaves, pineleaves, water, diamond, bedrock, redstone, sand, nocrack, crack1, crack2, crack3, crack4, air };
    public bool isSolid;

    public BlockType blockType;
    public BlockType health;

    public Chunk owner;
    private GameObject _parent;
    public Vector3 position;
    public int currHealth;
    private int[] _blockHealthMax = { 3, 3, 3, 4, 4, 4, 4, 4, 2, 2, 8, 0, -1, 4, 2, 4, 0, 0, 0, 0, 0 };


    private Vector2[,] _blocksUVs = {
        {new Vector2(0.125f,0.375f), new Vector2(0.1875f,0.375f),new Vector2(0.125f,0.4375f),new Vector2(0.1875f,0.4375f)}, // grass top
        {new Vector2(0.125f,0.6875f),new Vector2(0.1875f,0.6875f),new Vector2(0.125f,0.75f),new Vector2(0.1875f,0.75f)}, // snow
        {new Vector2(0.1875f,0.9375f),new Vector2(0.25f,0.9375f),new Vector2(0.1875f,1),new Vector2(0.25f,1) }, // grass side
        {new Vector2(0.25f,0.6875f),new Vector2(0.3125f,0.6875f),new Vector2(0.25f,0.75f),new Vector2(0.3125f,0.75f) }, // snow side
        {new Vector2(0.125f,0.9375f),new Vector2(0.1875f,0.9375f),new Vector2(0.125f,1),new Vector2(0.1875f,1) }, // dirt
        {new Vector2(0,0.875f),new Vector2(0.0625f,0.875f),new Vector2(0,0.9375f),new Vector2(0.0625f,0.9375f) }, // stone
        {new Vector2(0.5f,0.5625f),new Vector2(0.5625f,0.5625f),new Vector2(0.5f,0.625f),new Vector2(0.5625f,0.625f) }, // wood base 1
        {new Vector2(0.4375f,0.625f),  new Vector2(0.5f,0.625f),new Vector2(0.4375f,0.6875f), new Vector2(0.5f,0.6875f)}, // wood base 2
        {new Vector2(0.375f,0.625f),new Vector2(0.4375f,0.625f),new Vector2(0.375f,0.6875f),new Vector2(0.4375f,0.6875f) }, // wood
        {new Vector2(0.4375f,0.625f),  new Vector2(0.5f,0.625f),new Vector2(0.4375f,0.6875f), new Vector2(0.5f,0.6875f)}, // wood 2
        {new Vector2(0.0625f,0.375f),new Vector2(0.125f,0.375f),new Vector2(0.0625f,0.4375f),new Vector2(0.125f,0.4375f) }, // leaves
        {new Vector2(0.125f,0.375f),  new Vector2(0.1875f,0.375f),new Vector2(0.125f,0.4375f), new Vector2(0.1875f,0.4375f) }, // leaves 2
        {new Vector2(0.875f,0.125f),  new Vector2(0.9375f,0.125f),new Vector2(0.875f,0.1875f), new Vector2(0.9375f,0.1875f)}, // water
        {new Vector2(0.125f,0.75f),new Vector2(0.1875f,0.75f),new Vector2(0.125f,0.8125f),new Vector2(0.1875f,0.8125f) }, // diamond
        {new Vector2(0.3125f, 0.8125f), new Vector2(0.375f,0.8125f), new Vector2(0.3125f, 0.875f),new Vector2(0.375f, 0.875f) }, // red rock
        {new Vector2(0.1875f,0.75f),new Vector2(0.25f,0.75f),new Vector2(0.1875f,0.8125f),new Vector2(0.25f,0.8125f) }, // red stone
        {new Vector2(0,0.25f),new Vector2(0.0625f,0.25f),new Vector2(0,0.3125f),new Vector2(0.0625f,0.3125f) }, // sand
        {new Vector2(0.6875f,0f),  new Vector2(0.75f,0f),new Vector2(0.6875f,0.0625f), new Vector2(0.75f,0.0625f) }, // no crack
        {new Vector2(0f,0f),  new Vector2(0.0625f,0f),new Vector2(0f,0.0625f), new Vector2(0.0625f,0.0625f) }, // crack 1
        {new Vector2(0.0625f,0f),  new Vector2(0.125f,0f),new Vector2(0.0625f,0.0625f), new Vector2(0.125f,0.0625f) }, // crack 2
        {new Vector2(0.125f,0f),  new Vector2(0.1875f,0f),new Vector2(0.125f,0.0625f), new Vector2(0.1875f,0.0625f) }, // crack 3
        {new Vector2(0.1875f,0f),  new Vector2(0.25f,0f),new Vector2(0.1875f,0.0625f), new Vector2(0.25f,0.0625f) }, // crack 4
    };




    public Block(BlockType blockType, Vector3 position, GameObject parent, Chunk owner)
    {
        this.blockType = blockType;
        this.position = position;
        _parent = parent;
        this.owner = owner;
        SetType(blockType);
    }

    private void CreateQuad(CubeSide side)
    {
        Mesh mesh = new Mesh();
        mesh.name = "ScriptableMesh";

        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        List<Vector2> cracksUVs = new List<Vector2>();
        int[] triangles = new int[6];

        //uvs
        Vector2 uv00;
        Vector2 uv10;
        Vector2 uv01;
        Vector2 uv11;

        if (blockType == BlockType.grass && side == CubeSide.top)
        {
            uv00 = _blocksUVs[0, 0];
            uv10 = _blocksUVs[0, 1];
            uv01 = _blocksUVs[0, 2];
            uv11 = _blocksUVs[0, 3];
        }
        else if (blockType == BlockType.snow && side == CubeSide.top)
        {
            uv00 = _blocksUVs[1, 0];
            uv10 = _blocksUVs[1, 1];
            uv01 = _blocksUVs[1, 2];
            uv11 = _blocksUVs[1, 3];
        }
        else if ((blockType == BlockType.grass || blockType == BlockType.snow) && side == CubeSide.bottom)
        {
            uv00 = _blocksUVs[(int)(BlockType.dirt + 2), 0];
            uv10 = _blocksUVs[(int)(BlockType.dirt + 2), 1];
            uv01 = _blocksUVs[(int)(BlockType.dirt + 2), 2];
            uv11 = _blocksUVs[(int)(BlockType.dirt + 2), 3];
        }
        else
        {
            uv00 = _blocksUVs[(int)(blockType + 2), 0];
            uv10 = _blocksUVs[(int)(blockType + 2), 1];
            uv01 = _blocksUVs[(int)(blockType + 2), 2];
            uv11 = _blocksUVs[(int)(blockType + 2), 3];
        }

        // cracks
        cracksUVs.Add(_blocksUVs[(int)health + 2, 3]);
        cracksUVs.Add(_blocksUVs[(int)health + 2, 2]);
        cracksUVs.Add(_blocksUVs[(int)health + 2, 0]);
        cracksUVs.Add(_blocksUVs[(int)health + 2, 1]);


        //vertices
        Vector3 v0 = new Vector3(-0.5f, -0.5f, 0.5f);
        Vector3 v1 = new Vector3(0.5f, -0.5f, 0.5f);
        Vector3 v2 = new Vector3(0.5f, -0.5f, -0.5f);
        Vector3 v3 = new Vector3(-0.5f, -0.5f, -0.5f);
        Vector3 v4 = new Vector3(-0.5f, 0.5f, 0.5f);
        Vector3 v5 = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 v6 = new Vector3(0.5f, 0.5f, -0.5f);
        Vector3 v7 = new Vector3(-0.5f, 0.5f, -0.5f);


        switch (side)
        {
            case CubeSide.bottom:
                vertices = new Vector3[] { v0, v1, v2, v3 };
                normals = new Vector3[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
                uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
            case CubeSide.top:
                vertices = new Vector3[] { v7, v6, v5, v4 };
                normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
            case CubeSide.left:
                vertices = new Vector3[] { v7, v4, v0, v3 };
                normals = new Vector3[] { Vector3.left, Vector3.left, Vector3.left, Vector3.left };
                uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
            case CubeSide.right:
                vertices = new Vector3[] { v5, v6, v2, v1 };
                normals = new Vector3[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
                uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
            case CubeSide.front:
                vertices = new Vector3[] { v4, v5, v1, v0 };
                normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
                uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
            case CubeSide.back:
                vertices = new Vector3[] { v6, v7, v3, v2 };
                normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
                uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.SetUVs(1, cracksUVs);
        mesh.triangles = triangles;

        mesh.RecalculateBounds();

        GameObject quad = new GameObject("quad");
        quad.transform.position = position;
        quad.transform.SetParent(_parent.transform);
        MeshFilter meshFilter = quad.AddComponent(typeof(MeshFilter)) as MeshFilter;
        meshFilter.mesh = mesh;
        //MeshRenderer renderer = quad.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        //renderer.material = _blockMaterial;
    }

    private bool HasSolidNeighbour(int x, int y, int z)
    {
        try
        {
            Block b = GetBlock(x, y, z);
            if (b != null)
                return b.isSolid || b.blockType == blockType;
        }
        catch (System.IndexOutOfRangeException) { return false; }

        return false;
    }

    public Block GetBlock(int x, int y, int z)
    {
        Block[,,] chunks;

        if (x < 0 || x >= World.chunkSize ||
           y < 0 || y >= World.chunkSize ||
           z < 0 || z >= World.chunkSize)
        {

            int newX = x, newY = y, newZ = z;

            if (x < 0 || x >= World.chunkSize)
                newX = (x - (int)position.x) * World.chunkSize;

            if (y < 0 || y >= World.chunkSize)
                newY = (y - (int)position.y) * World.chunkSize;

            if (z < 0 || z >= World.chunkSize)
                newZ = (z - (int)position.z) * World.chunkSize;

            Vector3 neighbourChunkPosition = _parent.transform.position + new Vector3(newX, newY, newZ);

            string neighbourChunkName = World.BuildChunkName(neighbourChunkPosition);
            x = ConvertBlockIndexToLocal(x);
            y = ConvertBlockIndexToLocal(y);
            z = ConvertBlockIndexToLocal(z);

            Chunk neighbourChunk;
            if (World.chunks.TryGetValue(neighbourChunkName, out neighbourChunk))
                chunks = neighbourChunk.chunkBlocks;
            else return null;
        }
        else
            chunks = owner.chunkBlocks;

        return chunks[x, y, z];
    }

    private int ConvertBlockIndexToLocal(int i)
    {
        if (i <= -1)
            i = World.chunkSize + i;

        else if (i >= World.chunkSize)
            i = i = World.chunkSize;

        return i;
    }

    public void Draw()
    {
        if (blockType == BlockType.air)
            return;

        if (!HasSolidNeighbour((int)position.x, (int)position.y - 1, (int)position.z))
            CreateQuad(CubeSide.bottom);
        if (!HasSolidNeighbour((int)position.x, (int)position.y + 1, (int)position.z))
            CreateQuad(CubeSide.top);
        if (!HasSolidNeighbour((int)position.x - 1, (int)position.y, (int)position.z))
            CreateQuad(CubeSide.left);
        if (!HasSolidNeighbour((int)position.x + 1, (int)position.y, (int)position.z))
            CreateQuad(CubeSide.right);
        if (!HasSolidNeighbour((int)position.x, (int)position.y, (int)position.z + 1))
            CreateQuad(CubeSide.front);
        if (!HasSolidNeighbour((int)position.x, (int)position.y, (int)position.z - 1))
            CreateQuad(CubeSide.back);

    }

    public bool BuildBlock(BlockType type)
    {
        if (type == BlockType.water)
            owner.chunkMB.StartCoroutine(owner.chunkMB.Flow(this, BlockType.water, _blockHealthMax[(int)BlockType.water], 10));

        else if (type == BlockType.sand)
            owner.chunkMB.StartCoroutine(owner.chunkMB.Drop(this, BlockType.sand, 20));

        else
        {
            SetType(type);
            owner.ReDraw();
        }
        return true;
    }

    public bool HitBlock()
    {
        if (currHealth == -1) return false;
        currHealth--;
        health++;

        if (currHealth == _blockHealthMax[(int)blockType] - 1)
            owner.chunkMB.StartCoroutine(owner.chunkMB.HealBlock(position));

        if (currHealth <= 0)
        {
            blockType = BlockType.air;
            isSolid = false;
            health = BlockType.nocrack;
            owner.ReDraw();
            owner.UpdateChunk();
            return true;
        }

        owner.ReDraw();
        return false;
    }

    public void SetType(BlockType type)
    {
        blockType = type;
        if (type == BlockType.air || type == BlockType.water)
            isSolid = false;
        else
            isSolid = true;

        health = BlockType.nocrack;
        currHealth = _blockHealthMax[(int)blockType];
    }

    public void Reset()
    {
        health = BlockType.nocrack;
        currHealth = _blockHealthMax[(int)blockType];
        owner.ReDraw();
    }

}
