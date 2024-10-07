using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CourseGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Transform blockPrefab;
    [SerializeField] private Transform jumpPadPrefab;
    [SerializeField] private Transform jumpBallPrefab;
    [Header("Parents")]
    [SerializeField] private Transform blockParent;
    [SerializeField] private Transform jumpPadParent;
    [Header("Settings")]
    [SerializeField, Tooltip("Seed will not be set if this is 0")] private int seed;
    [SerializeField] private Vector3 courseMin;
    [SerializeField] private Vector3 courseMax;
    [SerializeField] private int blockAmount;
    [SerializeField] private int jumpPadAmount;
    [SerializeField] private float blockMaxAngle;
    [SerializeField] private float padMaxAngle;
    [SerializeField] private Vector2 jumpPadImpulseRange;
    [SerializeField] private Vector2 jumpBallImpulseRange;
    [SerializeField] private float minSpacing;
    [Space(10)]
    [SerializeField] private List<Transform> blocks = new();
    [SerializeField] private List<Transform> jumpPads = new();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = (courseMax - courseMin) / 2f + courseMin;
        Gizmos.DrawWireCube(center, courseMax - courseMin);
    }

    public void Generate()
    {
        if (seed != 0)
            Random.InitState(seed);

        GenerateBlocks();
        GenerateJumpPads();
    }

    public void GenerateBlocks()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            return;

        foreach (Transform t in blocks)
            DestroyImmediate(t.gameObject);

        blocks.Clear();
        
        for (int i = 0; i < blockAmount; i++)
            GenerateBlock(false);

        EditorUtility.SetDirty(this);
#endif
    }

    public void GenerateBlock(bool isSingle)
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            return;

        Transform instantiated = (Transform)PrefabUtility.InstantiatePrefab(blockPrefab, blockParent);
        instantiated.SetPositionAndRotation(GetRandPos(), GetRandRot(0f, blockMaxAngle));
        blocks.Add(instantiated);

        Physics.SyncTransforms(); // Ensure colliders are synced to the new transform position so that future valid position checks can query the collider properly

        if (isSingle)
        {
            blockAmount++;
            EditorUtility.SetDirty(this);
        }
#endif
    }

    public void GenerateJumpPads()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            return;

        foreach (Transform t in jumpPads)
            DestroyImmediate(t.gameObject);

        jumpPads.Clear();

        for (int i = 0; i < jumpPadAmount; i++)
            GenerateJumpPad(false);

        EditorUtility.SetDirty(this);
#endif
    }

    public void GenerateJumpPad(bool isSingle)
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            return;

        if (Random.value > 0.5f)
        {
            Transform instantiated = (Transform)PrefabUtility.InstantiatePrefab(jumpPadPrefab, jumpPadParent);
            instantiated.SetPositionAndRotation(GetRandPos(), GetRandRot(-90f, padMaxAngle));
            //instantiated.GetComponent<ImpulseProcessor>().ImpulseStrength = Random.Range(jumpPadImpulseRange.x, jumpPadImpulseRange.y);
            jumpPads.Add(instantiated);
        }
        else
        {
            Transform instantiated = (Transform)PrefabUtility.InstantiatePrefab(jumpBallPrefab, jumpPadParent);
            instantiated.SetPositionAndRotation(GetRandPos(), Quaternion.identity);
            //instantiated.GetComponent<ImpulseProcessor>().ImpulseStrength = Random.Range(jumpBallImpulseRange.x, jumpBallImpulseRange.y);
            jumpPads.Add(instantiated);
        }

        Physics.SyncTransforms(); // Ensure colliders are synced to the new transform position so that future valid position checks can query the collider properly

        if (isSingle)
        {
            jumpPadAmount++;
            EditorUtility.SetDirty(this);
        }
#endif
    }

    private Vector3 GetRandPos()
    {
        for (int i = 0; i < 100; i++)
        {
            Vector3 pos = new(Random.Range(courseMin.x, courseMax.x), Random.Range(courseMin.y, courseMax.y), Random.Range(courseMin.z, courseMax.z));
            if (!Physics.CheckBox(pos, new Vector3(minSpacing, minSpacing, minSpacing) / 2f))
                return pos;
        }

        Debug.LogWarning($"Failed to generate a valid position after {100} attempts, returning {Vector3.zero}!");
        return Vector3.zero;
    }

    private Quaternion GetRandRot(float xRotForUp, float maxAngleFromUp)
    {
        return Quaternion.Euler(xRotForUp + Random.Range(0f, maxAngleFromUp), Random.Range(0f, 360f), 0f);
    }
}
