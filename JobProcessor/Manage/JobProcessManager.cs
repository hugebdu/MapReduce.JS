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
        #region Data Members
        private IJobChunkDispatcher _chunkDispatcher;
        private IJobChunksProvider _jobChunksProvider;
        private IJobChunkRegistrator _jobJobChunkRegistrator;
        private IJobChunkResultWatcher _jobChunkResultWatcher;
        private IJobHistoryUpdater _jobHistoryUpdater;
        private Dictionary<string, IMapResultsCollector> _mapResultsCollectors;
        private IFactory _factory;
        #endregion Data Members

        #region Ctor
        public JobProcessManager(IFactory factory)
        {
            Logger.Log.Instance.Info("JobProcessManager. Constructor. Initialize job chunk provider and dispatcher");
            _factory = factory;
            _jobChunksProvider = factory.CreateChunksProvider();
            
            _jobJobChunkRegistrator = factory.CreateJobChunkRegistrator();
            _jobJobChunkRegistrator.JobMapComplete += new Action<object, string>(_jobJobChunkRegistrator_JobMapComplete);
            _jobJobChunkRegistrator.JobReduceComplete += new Action<object, string>(_jobJobChunkRegistrator_JobReduceComplete);

            _chunkDispatcher = factory.CreateChunkDispatcher(_jobJobChunkRegistrator);

            _jobHistoryUpdater = factory.CreateJobHistoryUpdater();

            _jobChunkResultWatcher = factory.CreateChunkResultWatcher();
            _jobChunkResultWatcher.ChunkMapResultArrive += new Action<object, MapResultMessage>(_chunkResultWatcher_ChunkMapResultArrive);
            _jobChunkResultWatcher.StartWatch();
            
            _mapResultsCollectors = new Dictionary<string, IMapResultsCollector>();
        }
        #endregion Ctor

        #region Public methods
        public bool ProcessJob(JobInfo jobInfo, Action<JobInfo, JobProcessStatus> callback)
        {
            try
            {
                Logger.Log.Instance.Info(string.Format("JobProcessManager. Process job. Message Id: {0}, PopReceipt: {1}",
                    jobInfo.JobMessageId,
                    jobInfo.PopReceipt));

                _jobHistoryUpdater.UpdateJobStatus(jobInfo, JobProcessStatus.New);

                // Job returned (not completed during the invisibility period of previous dequeue)
                if (_mapResultsCollectors.ContainsKey(jobInfo.JobId))
                    _mapResultsCollectors.Remove(jobInfo.JobId);

                _mapResultsCollectors.Add(jobInfo.JobId, _factory.CreateMapResultsCollector(jobInfo));
                foreach (var chunk in _jobChunksProvider.SplitJob(jobInfo))
                {
                    _jobJobChunkRegistrator.RegisterNewMapChunk(chunk.ChunkUid);
                    _jobHistoryUpdater.AddChunckInfo(chunk);
                    _chunkDispatcher.Dispatch(chunk);
                }
                
                _jobHistoryUpdater.UpdateJobStatus(jobInfo, JobProcessStatus.MapRunning);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log.Instance.Exception(ex, "JobProcessManager. Job processing failed.");
                return false;
            }
        }
        #endregion Public methods

        #region Private event handlers
        private void _jobJobChunkRegistrator_JobMapComplete(object sender, string jobId)
        {
            try
            {
                Logger.Log.Instance.Info(string.Format("JobMonitor. Job map complete. JobId '{0}'", jobId));
                _jobHistoryUpdater.UpdateJobStatus(new JobInfo() {JobId = jobId }, JobProcessStatus.MapComplete);

                var splittedMappedData = _mapResultsCollectors[jobId].SplittedMappedData();
                foreach (var chunk in splittedMappedData)
                {
                    _jobJobChunkRegistrator.RegisterNewReduceChunk(chunk.ChunkUid);
                    _jobHistoryUpdater.AddChunckInfo(chunk);
                    _chunkDispatcher.Dispatch(chunk);
                }
                
                _jobHistoryUpdater.UpdateJobStatus(new JobInfo() { JobId = jobId }, JobProcessStatus.ReduceRunning);
            }
            catch (Exception ex)
            {
                Logger.Log.Instance.Exception(ex, "JobProcessManager. JobMapComplete handler failed.");
            }
        }

        private void _chunkResultWatcher_ChunkMapResultArrive(object sender, MapResultMessage mapResultMessage)
        {
            try
            {
                Logger.Log.Instance.Info(string.Format("JobMonitor. Map stage result arrived for JobId '{0}', ChunkId '{1}'",
                    mapResultMessage.ChunkUid.JobId,
                    mapResultMessage.ChunkUid.ChunkId));

                _mapResultsCollectors[mapResultMessage.ChunkUid.JobId].AddResult(mapResultMessage);
                _jobJobChunkRegistrator.UpdateChunkMapComplete(mapResultMessage.ChunkUid);
            }
            catch (Exception ex)
            {
                Logger.Log.Instance.Exception(ex, "JobProcessManager. ChunkMapResultArrive handler failed.");
            }
        }

        private void _jobJobChunkRegistrator_JobReduceComplete(object sender, string jobId)
        {
            try
            {
                _jobHistoryUpdater.UpdateJobStatus(new JobInfo() { JobId = jobId }, JobProcessStatus.ReduceComplete);
                
                // TODO: callback on job complete
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Logger.Log.Instance.Exception(ex, "JobProcessManager. JobReduceComplete handler failed.");
            }
        }
        #endregion Private event handlers
    }
}
