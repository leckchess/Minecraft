using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkMB : MonoBehaviour
{
    Chunk _owner;
    public ChunkMB() { }
    public void SetOwner(Chunk owner)
    {
        _owner = owner;
        InvokeRepeating("SaveProgress", 10, 1000);
    }

    public IEnumerator HealBlock(Vector3 blockpos)
    {
        yield return new WaitForSeconds(3);
        int x = (int)blockpos.x;
        int y = (int)blockpos.y;
        int z = (int)blockpos.z;

        if (_owner.chunkBlocks[x, y, z].blockType != Block.BlockType.air)
            _owner.chunkBlocks[x, y, z].Reset();
    }

    void SaveProgress()
    {
        if (_owner.changed)
        {
            _owner.Save();
            _owner.changed = false;
        }
    }

    public IEnumerator Flow(Block block, Block.BlockType blockType, int strength, int maxsize)
    {
        if (maxsize <= 0) yield break;
        if (block == null) yield break;
        if (strength <= 0) yield break;
        if (block.blockType != Block.BlockType.air) yield break;

        block.SetType(blockType);
        block.currHealth = strength;
        block.owner.ReDraw();

        yield return new WaitForSeconds(1);

        int x = (int)block.position.x;
        int y = (int)block.position.y;
        int z = (int)block.position.z;

        Block below = block.GetBlock(x, y - 1, z);
        if (below != null && below.blockType == Block.BlockType.air)
        {
            StartCoroutine(Flow(below, blockType, strength, --maxsize));
            yield break;
        }
        else
        {
            --strength;
            --maxsize;

            World.coroutineQueue.Run(Flow(block.GetBlock(x - 1, y, z), blockType, strength, maxsize));
            yield return new WaitForSeconds(1);

            World.coroutineQueue.Run(Flow(block.GetBlock(x + 1, y, z), blockType, strength, maxsize));
            yield return new WaitForSeconds(1);

            World.coroutineQueue.Run(Flow(block.GetBlock(x, y, z - 1), blockType, strength, maxsize));
            yield return new WaitForSeconds(1);

            World.coroutineQueue.Run(Flow(block.GetBlock(x, y, z + 1), blockType, strength, maxsize));
            yield return new WaitForSeconds(1);
        }
    }

    public IEnumerator Drop(Block block, Block.BlockType blockType, int maxdrop)
    {
        Block thisblock = block;
        Block prevblock = null;

        for (int i = 0; i < maxdrop; i++)
        {
            Block.BlockType prevblocktype = thisblock.blockType;

            if (prevblocktype != blockType)
                thisblock.SetType(blockType);
            
            if (prevblock != null)
                prevblock.SetType(prevblocktype);

            prevblock = thisblock;
            block.owner.ReDraw();

            yield return new WaitForSeconds(0.2f);
            Vector3 pos = thisblock.position;

            thisblock = thisblock.GetBlock((int)pos.x, (int)pos.y - 1, (int)pos.z);
            if (thisblock.isSolid)
                yield break;
        }
    }
}
