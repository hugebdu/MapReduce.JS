using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor.Model;
using JobProcessor.Interfaces;

namespace JobProcessor.Manage
{
    public class JobProcessManager
    {
        private IJobChunkDispatcher _chunkDispatcher;
        private IJobChunksProvider _jobChunksProvider;
        private IJobChunkRegistrator _jobJobChunkRegistrator;
        private IJobChunkResultWatcher _jobChunkResultWatcher;
        private Dictionary<string, IMapResultsCollector> _mapResultsCollectors;
        private IFactory _factory;

        public JobProcessManager(IFactory factory)
        {
            Logger.Log.Instance.Info("JobProcessManager. Constructor. Initialize job chunk provider and dispatcher");
            _factory = factory;
            _jobChunksProvider = factory.CreateChunksProvider();
            
            _jobJobChunkRegistrator = factory.CreateJobChunkRegistrator();
            _jobJobChunkRegistrator.JobMapComplete += new Action<object, string>(_jobJobChunkRegistrator_JobMapComplete);
            _jobJobChunkRegistrator.JobReduceComplete += new Action<object, string>(_jobJobChunkRegistrator_JobReduceComplete);

            _chunkDispatcher = factory.CreateChunkDispatcher(_jobJobChunkRegistrator);
            
            _jobChunkResultWatcher = factory.CreateChunkResultWatcher();
            _jobChunkResultWatcher.ChunkMapResultArrive += new Action<object, MapResultMessage>(_chunkResultWatcher_ChunkMapResultArrive);
            _jobChunkResultWatcher.StartWatch();
            
            _mapResultsCollectors = new Dictionary<string, IMapResultsCollector>();
        }

        void _jobJobChunkRegistrator_JobReduceComplete(object arg1, string arg2)
        {
            // TODO: callback on job complete
            throw new NotImplementedException();
        }

        public void ProcessJob(JobInfo jobInfo, Action<JobInfo, JobProcessStatus> callback)
        {
            Logger.Log.Instance.Info(string.Format("JobProcessManager. Process job. Message Id: {0}, PopReceipt: {1}",
                jobInfo.JobMessageId,
                jobInfo.PopReceipt));

            _mapResultsCollectors.Add(jobInfo.JobId, _factory.CreateMapResultsCollector(jobInfo));
            foreach (var chunk in _jobChunksProvider.SplitJob(jobInfo))
            {
                _jobJobChunkRegistrator.RegisterNewMapChunk(chunk.ChunkUid);
                _chunkDispatcher.Dispatch(chunk);
            }
        }

        private void _jobJobChunkRegistrator_JobMapComplete(object sender, string jobId)
        {
            Logger.Log.Instance.Info(string.Format("JobMonitor. Job map complete. JobId '{0}'", jobId));
            
            var splittedMappedData = _mapResultsCollectors[jobId].SplittedMappedData();
            foreach (var chunk in splittedMappedData)
            {
                _jobJobChunkRegistrator.RegisterNewReduceChunk(chunk.ChunkUid);
                _chunkDispatcher.Dispatch(chunk);                
            }
        }

        private void _chunkResultWatcher_ChunkMapResultArrive(object sender, MapResultMessage mapResultMessage)
        {
            Logger.Log.Instance.Info(string.Format("JobMonitor. Map stage result arrived for JobId '{0}', SplitId '{1}'",
                mapResultMessage.ChunkUid.JobId,
                mapResultMessage.ChunkUid.SplitId));

            _mapResultsCollectors[mapResultMessage.ChunkUid.JobId].AddResult(mapResultMessage);
            _jobJobChunkRegistrator.UpdateChunkMapComplete(mapResultMessage.ChunkUid);
        }
    }
}
