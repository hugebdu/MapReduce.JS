using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor;
using JobProcessor.Exceptions;
using JobProcessor.Interfaces;
using JobProcessor.Model;

namespace JobProcessor.Implementation
{
    class CacheJobChunkRegistrator : IJobChunkRegistrator
    {
        private const string JobSumKeyPrefix = "Job_";
        private const string JobChunkKeyPrefix = "Chunk_";

        public event Action<object, string> JobMapComplete;
        public event Action<object, string> JobReduceComplete;

        public void RegisterNewMapChunk(JobChunkUid chunkUid)
        {
            UpdateJobSummeryInCache(chunkUid);
            AddChunkToCache(chunkUid, ProcessingMode.Map);
        }
        
        public void RegisterNewReduceChunk(JobChunkUid chunkUid)
        {
            UpdateJobSummeryInCache(chunkUid);
            AddChunkToCache(chunkUid, ProcessingMode.Reduce);
        }

        public void UpdateChunkMapSent(JobChunkUid chunkUid)
        {
            UpdateChunkStatusInCache(chunkUid, ChunkStatus.MapSent, ProcessingMode.Map);
        }

        public void UpdateChunkMapComplete(JobChunkUid chunkUid)
        {
            UpdateChunkStatusInCache(chunkUid, ChunkStatus.MapCompleted, ProcessingMode.Map);
            if (AllJobChunksCompleted(chunkUid.JobId, ProcessingMode.Map))
            {
                var jobMapComplete = JobMapComplete;
                if (jobMapComplete != null)
                {
                    // TODO: Cleanup cache for map-completed job
                    jobMapComplete(this, chunkUid.JobId);
                }
            }
        }

        public void UpdateChunkReduceSent(JobChunkUid chunkUid)
        {
            UpdateChunkStatusInCache(chunkUid, ChunkStatus.ReduceSent, ProcessingMode.Reduce);
        }

        public void UpdateChunkReduceComplete(JobChunkUid chunkUid)
        {
            UpdateChunkStatusInCache(chunkUid, ChunkStatus.ReduceCompleted, ProcessingMode.Reduce);
            if (AllJobChunksCompleted(chunkUid.JobId, ProcessingMode.Reduce))
            {
                var jobReduceComplete = JobReduceComplete;
                if (jobReduceComplete != null)
                {
                    // TODO: Cleanup cache for reduce-completed job
                    jobReduceComplete(this, chunkUid.JobId);
                }
            }
        }

        private bool AllJobChunksCompleted(string jobId, ProcessingMode mode)
        {
            var sumKey = GetJobSummeryKey(jobId);
            var jobSplitDetails = AzureClient.Instance.CacheClient[sumKey] as JobSplitDetails;
            var completedStatus = mode == ProcessingMode.Map ? ChunkStatus.MapCompleted : ChunkStatus.ReduceCompleted;
            foreach (var chunkId in jobSplitDetails.JobChunkIds)
            {
                var chunkKey = GetJobChunkKey(new JobChunkUid() { JobId = jobId, SplitId = chunkId }, mode);
                var chunkStatus = AzureClient.Instance.CacheClient[chunkKey] as JobChunkStatus;
                if (chunkStatus.Status != completedStatus)
                    return false;
            }

            return true;
        }

        private void UpdateJobSummeryInCache(JobChunkUid chunkUid)
        {
            var sumKey = GetJobSummeryKey(chunkUid.JobId);
            var jobSummery = AzureClient.Instance.CacheClient.Get(sumKey) as JobSplitDetails;
            if (jobSummery == null)
            {
                jobSummery = new JobSplitDetails();
                jobSummery.JobId = chunkUid.JobId;
            }

            if (!jobSummery.JobChunkIds.Contains(chunkUid.SplitId))
            {
                jobSummery.JobChunkIds.Add(chunkUid.SplitId);
                AzureClient.Instance.CacheClient.Add(sumKey, jobSummery);
            }
        }

        private void AddChunkToCache(JobChunkUid chunkUid, ProcessingMode mode)
        {
            var chunkKey = GetJobChunkKey(chunkUid, mode);
            var chunkStatus = new JobChunkStatus()
            {
                LastUpdate = DateTime.Now,
                Status = mode == ProcessingMode.Map ? ChunkStatus.NewMap : ChunkStatus.NewReduce
            };
            
            chunkStatus.ChunkUid.JobId = chunkUid.JobId;
            chunkStatus.ChunkUid.SplitId = chunkUid.SplitId;

            AzureClient.Instance.CacheClient.Add(chunkKey, chunkStatus);
        }

        private void UpdateChunkStatusInCache(JobChunkUid chunkUid, ChunkStatus status, ProcessingMode mode)
        {
            var chunkKey = GetJobChunkKey(chunkUid, mode);
            var chunkStatus = AzureClient.Instance.CacheClient.Get(chunkKey) as JobChunkStatus;
            if (chunkStatus == null)
                throw new UnknownChunkException("Unrestered chunk") { ChunkUid = chunkUid };
            
            chunkStatus.Status = status;
            chunkStatus.LastUpdate = DateTime.Now;
            AzureClient.Instance.CacheClient.Add(chunkKey, chunkStatus);
        }

        private string GetJobSummeryKey(string jobId)
        {
            return string.Concat(JobSumKeyPrefix, jobId);
        }

        private string GetJobChunkKey(JobChunkUid chunkUid, ProcessingMode mode)
        {
            return string.Format("{3}_{0}{1}_{2}", JobChunkKeyPrefix, chunkUid.JobId, chunkUid.SplitId, mode);
        }
    }
}
