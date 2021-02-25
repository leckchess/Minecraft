using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Realtime.Messaging.Internal;

public class World : MonoBehaviour
{
    public GameObject player;
    public Material textureAtlas;
    public Material fluidTextureAtlas;

    public static int columnHeight = 16;
    public static int chunkSize = 16;
    public static int worldSize = 2;
    public static int radious = 4;
    public static ConcurrentDictionary<string, Chunk> chunks;
    public static bool firstBuild = true;
    public static List<string> chunksToRemove = new List<string>();

    public static CoroutineQueue coroutineQueue;
    public static uint maxCouroutines = 1000;


    public Vector3 lastBuildPosition;

    private void Start()
    {
        Vector3 playerpos = player.transform.position;
        player.transform.position = new Vector3(playerpos.x, Utils.GenerateHeight(playerpos.x, playerpos.z) + 1, playerpos.z);

        lastBuildPosition = player.transform.position;

        player.SetActive(false);

        firstBuild = true;
        chunks = new ConcurrentDictionary<string, Chunk>();
        coroutineQueue = new CoroutineQueue(maxCouroutines, StartCoroutine);

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        BuildChunkAt((int)(player.transform.position.x / chunkSize), (int)(player.transform.position.y / chunkSize), (int)(player.transform.position.z / chunkSize));
        coroutineQueue.Run(DrawChunk());

        coroutineQueue.Run(BuildRecursiveWorld((int)(player.transform.position.x / chunkSize), (int)(player.transform.position.y / chunkSize), (int)(player.transform.position.z / chunkSize), radious));
    }

    private void Update()
    {
        Vector3 movement = lastBuildPosition - player.transform.position;
        if (movement.magnitude > chunkSize)
        {
            lastBuildPosition = player.transform.position;
            BuildNearplayer();
        }

        if (!player.activeSelf)
        {
            player.SetActive(true);
            firstBuild = false;
        }

        coroutineQueue.Run(DrawChunk());
        coroutineQueue.Run(RemoveOldChunks());
    }

    void BuildNearplayer()
    {
        StopCoroutine("BuildRecursiveWorld");
        coroutineQueue.Run(BuildRecursiveWorld((int)(player.transform.position.x / chunkSize), (int)(player.transform.position.y / chunkSize), (int)(player.transform.position.z / chunkSize), radious));
    }

    public static string BuildChunkName(Vector3 position)
    {
        return position.x + "_" + position.y + "_" + position.z;
    }

    public static Block GetWorldBlock(Vector3 pos)
    {
        int chunkx, chunky, chunkz;
        if (pos.x < 0)
            chunkx = (int)(Mathf.Round(pos.x - chunkSize) / (float)chunkSize) * chunkSize;
        else
            chunkx = (int)(Mathf.Round(pos.x) / (float)chunkSize) * chunkSize;

        if (pos.y < 0)
            chunky = (int)(Mathf.Round(pos.y - chunkSize) / (float)chunkSize) * chunkSize;
        else
            chunky = (int)(Mathf.Round(pos.y) / (float)chunkSize) * chunkSize;

        if (pos.z < 0)
            chunkz = (int)(Mathf.Round(pos.z - chunkSize) / (float)chunkSize) * chunkSize;
        else
            chunkz = (int)(Mathf.Round(pos.z) / (float)chunkSize) * chunkSize;

        int blockx = (int)Mathf.Abs((float)Mathf.Round(pos.x) - chunkx);
        int blocky = (int)Mathf.Abs((float)Mathf.Round(pos.y) - chunky);
        int blockz = (int)Mathf.Abs((float)Mathf.Round(pos.z) - chunkz);

        string chunkname = BuildChunkName(new Vector3(chunkx, chunky, chunkz));
        Chunk chunk;
        if (chunks.TryGetValue(chunkname, out chunk))
        {
            return chunk.chunkBlocks[blockx, blocky, blockz];
        }
        else
            return null;
    }

    public void BuildChunkAt(int x, int y, int z)
    {
        Vector3 position = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize);

        Chunk chunk;

        string chunkName = World.BuildChunkName(position);

        if (!chunks.TryGetValue(chunkName, out chunk))
        {
            chunk = new Chunk(position, textureAtlas, fluidTextureAtlas);
            chunk.chunk.transform.SetParent(transform);
            chunks.TryAdd(BuildChunkName(position), chunk);
        }
    }

    IEnumerator BuildRecursiveWorld(int x, int y, int z, int rad)
    {
        rad--;
        if (rad <= 0) yield break;

        BuildChunkAt(x, y, z - 1);
        coroutineQueue.Run(BuildRecursiveWorld(x, y, z - 1, rad));
        BuildChunkAt(x, y, z + 1);
        coroutineQueue.Run(BuildRecursiveWorld(x, y, z - 1, rad));

        BuildChunkAt(x, y - 1, z);
        coroutineQueue.Run(BuildRecursiveWorld(x, y, z - 1, rad));
        BuildChunkAt(x, y + 1, z);
        coroutineQueue.Run(BuildRecursiveWorld(x, y, z - 1, rad));

        BuildChunkAt(x - 1, y, z);
        coroutineQueue.Run(BuildRecursiveWorld(x, y, z - 1, rad));
        BuildChunkAt(x + 1, y, z);
        coroutineQueue.Run(BuildRecursiveWorld(x, y, z - 1, rad));

        yield return null;
    }

    IEnumerator DrawChunk()
    {
        foreach (KeyValuePair<string, Chunk> chunk in chunks)
        {
            if (chunk.Value.status == Chunk.ChunkStatus.draw)
            {
                chunk.Value.DrawChunk();
            }

            if (chunk.Value.chunk && Vector3.Distance(player.transform.position, chunk.Value.chunk.transform.position) > radious * chunkSize)
                chunksToRemove.Add(chunk.Key);

            yield return null;
        }
    }

    IEnumerator RemoveOldChunks()
    {
        foreach (string chunkName in chunksToRemove)
        {
            Chunk chunk;
            if (chunks.TryGetValue(chunkName, out chunk))
            {
                Destroy(chunk.chunk);
                chunk.Save();
                chunks.TryRemove(chunkName, out chunk);
                yield return null;
            }
        }
    }

    #region old code
    //public GameObject player;
    //public Material textureAtlas;
    //public Slider loadingSlider;
    //public GameObject uiCanvas;

    //public static int columnHeight = 16;
    //public static int chunkSize = 16;
    //public static int worldSize = 2;
    //public static int radious = 1;
    //public static Dictionary<string, Chunk> chunks;

    //private bool _firstBuild = true;
    //private bool _building = false;


    //private void Start()
    //{
    //    player.SetActive(false);
    //    chunks = new Dictionary<string, Chunk>();
    //    transform.position = Vector3.zero;
    //    transform.rotation = Quaternion.identity;
    //}

    //public void StartBuildTheWorld()
    //{
    //    StartCoroutine(BuildWorld());
    //}

    //private void Update()
    //{
    //    if (!_building && !_firstBuild)
    //        StartCoroutine(BuildWorld());
    //}

    //public static string BuildChunkName(Vector3 position)
    //{
    //    return position.x + "_" + position.y + "_" + position.z;
    //}

    //private IEnumerator BuildWorld()
    //{
    //    _building = true;

    //    int posX = (int)Mathf.Floor(player.transform.position.x / chunkSize);
    //    int posZ = (int)Mathf.Floor(player.transform.position.z / chunkSize);

    //    float totalChunks = (Mathf.Pow(radious * 2 + 1, 2) * columnHeight) * 2;
    //    int builtChunks = 0;

    //    for (int z = -radious; z <= radious; z++)
    //        for (int x = -radious; x <= radious; x++)
    //            for (int y = 0; y < columnHeight; y++)
    //            {
    //                Vector3 position = new Vector3((x + posX) * chunkSize, y * chunkSize, (z + posZ) * chunkSize);

    //                Chunk chunk;

    //                string chunkName = World.BuildChunkName(position);

    //                if (chunks.TryGetValue(chunkName, out chunk))
    //                {
    //                    chunk.status = Chunk.ChunkStatus.keep;
    //                    break;
    //                }
    //                else
    //                {
    //                    chunk = new Chunk(position, textureAtlas);
    //                    chunk.chunk.transform.SetParent(transform);
    //                    chunks.Add(BuildChunkName(position), chunk);
    //                }

    //                if (_firstBuild)
    //                {
    //                    builtChunks++;
    //                    loadingSlider.value = builtChunks / totalChunks * 100;
    //                }

    //                yield return null;

    //            }

    //    foreach (KeyValuePair<string, Chunk> chunk in chunks)
    //    {
    //        if (chunk.Value.status == Chunk.ChunkStatus.draw)
    //        {
    //            chunk.Value.DrawChunk();
    //            chunk.Value.status = Chunk.ChunkStatus.keep;
    //        }

    //        // delete
    //        chunk.Value.status = Chunk.ChunkStatus.done;

    //        if (_firstBuild)
    //        {
    //            builtChunks++;
    //            loadingSlider.value = builtChunks / totalChunks * 100;
    //        }

    //        yield return null;
    //    }

    //    if (_firstBuild)
    //    {
    //        player.SetActive(true);
    //        uiCanvas.SetActive(false);
    //        _firstBuild = false;
    //    }

    //    _building = false;
    //}

    //private IEnumerator BuildChunkColumn()
    //{
    //    for (int i = 0; i < columnHeight; i++)
    //    {
    //        Vector3 position = new Vector3(transform.position.x, i * chunkSize, transform.position.z);
    //        Chunk chunk = new Chunk(position, textureAtlas);
    //        chunk.chunk.transform.SetParent(transform);
    //        chunks.Add(BuildChunkName(position), chunk);
    //    }

    //    foreach (KeyValuePair<string, Chunk> c in chunks)
    //    {
    //        c.Value.DrawChunk();
    //    }

    //    yield return null;
    //}

    #endregion
}
