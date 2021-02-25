using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockInteraction : MonoBehaviour
{
    public GameObject cam;
    private Block.BlockType blockType = Block.BlockType.stone;

    void Update()
    {
        if (Input.GetKeyDown("1"))
            blockType = Block.BlockType.dirt;
        else if (Input.GetKeyDown("2"))
            blockType = Block.BlockType.stone;
        else if (Input.GetKeyDown("3"))
            blockType = Block.BlockType.grass;
        else if (Input.GetKeyDown("4"))
            blockType = Block.BlockType.sand;
        else if (Input.GetKeyDown("5"))
            blockType = Block.BlockType.water;


        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 10))
            //if (Physics.Raycast(ray, out hit, 10))
            {
                Chunk hitchunk;
                if (!World.chunks.TryGetValue(hit.collider.gameObject.name, out hitchunk)) return;

                Vector3 hitBlock;
                if (Input.GetMouseButtonDown(0))
                    hitBlock = hit.point - hit.normal / 2.0f;
                else
                    hitBlock = hit.point + hit.normal / 2.0f;

                Block b = World.GetWorldBlock(hitBlock);
                hitchunk = b.owner;

                bool update = false;
                if (Input.GetMouseButtonDown(0))
                    update = b.HitBlock();
                else
                    update = b.BuildBlock(blockType);


                if (update)
                {
                    hitchunk.changed = true;

                    List<string> updates = new List<string>();

                    if (hit.collider)
                    {
                        float thischunkx = hit.collider.transform.position.x;
                        float thischunky = hit.collider.transform.position.y;
                        float thischunkz = hit.collider.transform.position.z;

                        if (b.position.x == 0)
                            updates.Add(World.BuildChunkName(new Vector3(thischunkx - World.chunkSize, thischunky, thischunkz)));
                        if (b.position.x == World.chunkSize - 1)
                            updates.Add(World.BuildChunkName(new Vector3(thischunkx + World.chunkSize, thischunky, thischunkz)));
                        if (b.position.y == 0)
                            updates.Add(World.BuildChunkName(new Vector3(thischunkx, thischunky - World.chunkSize, thischunkz)));
                        if (b.position.y == World.chunkSize - 1)
                            updates.Add(World.BuildChunkName(new Vector3(thischunkx, thischunky + World.chunkSize, thischunkz)));
                        if (b.position.z == 0)
                            updates.Add(World.BuildChunkName(new Vector3(thischunkx, thischunky, thischunkz - World.chunkSize)));
                        if (b.position.z == World.chunkSize - 1)
                            updates.Add(World.BuildChunkName(new Vector3(thischunkx, thischunky, thischunkz + World.chunkSize)));

                        foreach (string chunkname in updates)
                        {
                            Chunk chunk;
                            if (World.chunks.TryGetValue(chunkname, out chunk))
                            {
                                chunk.ReDraw();
                            }
                        }
                    }
                }
            }

        }
    }
}
