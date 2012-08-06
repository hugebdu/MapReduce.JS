using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using JobProcessor;
using JobProcessor.Interfaces;

namespace JobProcessor.Implementation
{
    class DefaultJobHistoryUpdater : IJobHistoryUpdater
    {
        public void AddChunckInfo(Model.JobChunk chunk)
        {
            Logger.Log.Instance.Info(string.Format("DefaultJobHistoryUpdater. Add new chunk info. JobId: {0}, ChunkId: {1}, Mode: {2}",
                chunk.ChunkUid.JobId,
                chunk.ChunkUid.ChunkId,
                chunk.Mode.ToString()));

            using (var connection = new SqlConnection(RoleSettings.DbConnectionString))
            {
                connection.Open();
                var commandText = "INSERT INTO JobChunks (JobId,ChunkId,Status,Mode,UpdateDate) VALUES (@JobId,@ChunkId,@Status,@Mode,GETDATE())";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.Add("@JobId", SqlDbType.NVarChar);
                    command.Parameters["@JobId"].Value = chunk.ChunkUid.JobId;

                    command.Parameters.Add("@ChunkId", SqlDbType.UniqueIdentifier);
                    command.Parameters["@ChunkId"].Value = new Guid(chunk.ChunkUid.ChunkId);

                    command.Parameters.Add("@Status", SqlDbType.NVarChar);
                    command.Parameters["@Status"].Value = "New";

                    command.Parameters.Add("@Mode", SqlDbType.NVarChar);
                    command.Parameters["@Mode"].Value = chunk.Mode.ToString();

                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        public void UpdateJobStatus(Model.JobInfo jobInfo, Model.JobProcessStatus status)
        {
            Logger.Log.Instance.Info(string.Format("DefaultJobHistoryUpdater. Update Job status. JobId: {0}, Status: {1}",
                jobInfo.JobId,
                status.ToString()));

            using (var connection = new SqlConnection(RoleSettings.DbConnectionString))
            {
                connection.Open();
                var commandText = "INSERT INTO JobHistory (JobId,Status,UpdateDate,ProcessingRole) VALUES (@JobId,@Status, GETDATE(),@RoleId)";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.Add("@JobId", SqlDbType.NVarChar);
                    command.Parameters["@JobId"].Value = jobInfo.JobId;

                    command.Parameters.Add("@Status", SqlDbType.NVarChar);
                    command.Parameters["@Status"].Value = status.ToString();

                    command.Parameters.Add("@RoleId", SqlDbType.NVarChar);
                    command.Parameters["@RoleId"].Value = RoleSettings.RoleId;

                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
    }
}
