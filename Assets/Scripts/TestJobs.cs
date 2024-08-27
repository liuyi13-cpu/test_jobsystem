using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// 1.Jobs适合并行处理CPU密集型任务
/// </summary>
public class TestJobs : MonoBehaviour
{
    private void Start()
    {
        // Editor下 10倍
        // Jobs + BurstCompile
        NoJobs(); // 262 ms
        Jobs();   // 25  ms
    }

    private void NoJobs()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        
        NativeArray<float> result = new NativeArray<float>(10000000, Allocator.Temp);
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = i * Mathf.PI;
        }
        
        sw.Stop();
        Debug.LogWarning($"NoJobs: {sw.ElapsedMilliseconds}");
    }

    private void Jobs()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        
        NativeArray<float> result = new NativeArray<float>(10000000, Allocator.TempJob);
        var job = new MyJob();
        job.result = result;
        var handle = job.ScheduleByRef(); // 1.执行
        handle.Complete();                // 2.等待完成，阻塞主线程
        result.Dispose();
        
        sw.Stop();
        Debug.LogWarning($"Jobs: {sw.ElapsedMilliseconds}");
    }
}

[BurstCompile]
public struct MyJob : IJob
{
    public NativeArray<float> result;
    
    public void Execute()
    {
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = i * Mathf.PI;
        }
    }
}
