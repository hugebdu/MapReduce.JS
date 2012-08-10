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
        private IFactory _factory;
        private Dictionary<string, JobProcessData> _jobProcessDataCollection;
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
            _jobChunkResultWatcher.ChunkResultArrive += new Action<object, ChunkResultMessage>(_chunkResultWatcher_ChunkResultArrive);
            _jobChunkResultWatcher.StartWatch();
            
            _jobProcessDataCollection = new Dictionary<string, JobProcessData>();
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

                if(_jobProcessDataCollection.ContainsKey(jobInfo.JobId))
                    _jobProcessDataCollection.Remove(jobInfo.JobId);

                _jobProcessDataCollection.Add(jobInfo.JobId, new JobProcessData(_factory, jobInfo));
                _jobProcessDataCollection[jobInfo.JobId].JobCompleteCallback = callback;

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

                var splittedMappedData = _jobProcessDataCollection[jobId].MapResultsCollector.SplittedMappedData();
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

        private void _chunkResultWatcher_ChunkResultArrive(object sender, ChunkResultMessage mapResultMessage)
        {
            try
            {
                Logger.Log.Instance.Info(string.Format("JobMonitor. Chunk of {2} stage result arrived for JobId '{0}', ChunkId '{1}'",
                    mapResultMessage.ChunkUid.JobId,
                    mapResultMessage.ChunkUid.ChunkId,
                    mapResultMessage.Mode));

                if (mapResultMessage.Mode == ProcessingMode.Map)
                {
                    _jobProcessDataCollection[mapResultMessage.ChunkUid.JobId].MapResultsCollector.AddResult(mapResultMessage);
                    _jobJobChunkRegistrator.UpdateChunkMapComplete(mapResultMessage.ChunkUid);
                }
                else if (mapResultMessage.Mode == ProcessingMode.Reduce)
                {
                    _jobProcessDataCollection[mapResultMessage.ChunkUid.JobId].ReduceResultsCollector.AddResult(mapResultMessage);
                    _jobJobChunkRegistrator.UpdateChunkReduceComplete(mapResultMessage.ChunkUid);
                }
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
                _jobProcessDataCollection[jobId].ReduceResultsCollector.SubmitResult();
                _jobProcessDataCollection[jobId].JobCompleteCallback(_jobProcessDataCollection[jobId].JobInfo, JobProcessStatus.Completed);
            }
            catch (Exception ex)
            {
                Logger.Log.Instance.Exception(ex, "JobProcessManager. JobReduceComplete handler failed.");
            }
        }
        #endregion Private event handlers
    }
}
