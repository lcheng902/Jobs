using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

public class PatrolControl : MonoBehaviour
{
    private TransformAccessArray array;
    public Transform[] players;
    public NativeArray<int> _index;
    public Transform target;
    private void Start()
    {
        array = new TransformAccessArray(players);
        PosArray = new NativeArray<Vector3>(target.childCount, Allocator.Persistent);
        Debug.Log(target.childCount);
        for (int i = 0; i < target.childCount; i++)
        {
            PosArray[i] = target.GetChild(i).position;
        }

        _index = new NativeArray<int>(1, Allocator.Persistent);
        for (int i = 0; i < _index.Length; i++)
        {
            _index[i] = 0;
        }

        results = new NativeArray<bool>(1, Allocator.Persistent);
        for (int i = 0; i < _index.Length; i++)
        {
            results[i] = false;
        }
    }
    public NativeArray<bool> results;
    public NativeArray<Vector3> PosArray;

    private void Update()
    {

        var j5 = new J5
        {
            dt = Time.deltaTime,
            speed = 2,
            positions = PosArray,
            _index = _index,
            results = results
        };
        var j5Handle = j5.Schedule(array);
        j5Handle.Complete();

        if (j5.results[0])
        {
            j5.results[0] = false;
            var i = _index[0];

            Debug.Log(_index.Length);
            i = Random.Range(0,5) % 4;
            _index[0] = i;
        }
    }

    private void OnDestroy()
    {
        results.Dispose();
        PosArray.Dispose();
        _index.Dispose();
        array.Dispose();
    }

    /// <summary>
    ///  巡逻点
    /// </summary>
    [BurstCompile] struct J5 : IJobParallelForTransform
    {
        [ReadOnly] 
        public NativeArray<Vector3> positions;
        public float dt, speed;
        public NativeArray<bool> results;
        public NativeArray<int> _index;

        public void Execute(int index, TransformAccess transform)
        {
            var dis = Vector3.Distance(transform.position, positions[_index[0]]);
            if (dis > 0.01f)
            {
                var dir = positions[_index[0]] - transform.position;
                transform.rotation = quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.position += dir.normalized * speed * dt;
                results[0] = false;
            }
            else
            {
                results[0] = true;
            }
        }
    }
}