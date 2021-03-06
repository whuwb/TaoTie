﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace Task
{
    /// <summary>
    /// Encapsulate Testing and Cloud Environment Config
    /// </summary>
    class Environment
    {
        public static CloudTable GetTable(string name)
        {
            Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount = GetStorageAccount();

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(name);

            table.CreateIfNotExists();

            return table;
        }

        public static CloudQueue GetQueue(string name)
        {
            Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount = GetStorageAccount();

            CloudQueueClient client = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = client.GetQueueReference(name);

            queue.CreateIfNotExists();

            return queue;
        }

        /// <summary>
        /// $TODO: consider caching later
        /// </summary>
        /// <returns></returns>
        private static Microsoft.WindowsAzure.Storage.CloudStorageAccount GetStorageAccount()
        {
            Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount = null;

            if (RoleEnvironment.IsEmulated)
            {
                storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.DevelopmentStorageAccount;
            }
            else
            {
                // Retrieve the storage account from the connection string.
                storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"));
            }
            return storageAccount;
        }

        #region Prepare Test Data

        private static int ActorStep = 0;
        private static int Example = 1;

        private static List<ActorAssignment> wordCountAssignments = new List<ActorAssignment>()
        {
            new ActorAssignment(Guid.Empty)
                    {
                        Topology = "TestTopology",
                        Name = "WordReadSpout",
                        IsSpout = true,
                        InQueue = string.Empty,
                        OutQueues = "spoutoutput1,spoutoutput2",
                        SchemaGroupingMode = "ShuffleGrouping",
                        HeartBeat = DateTime.UtcNow,
                    },

            new ActorAssignment(Guid.Empty)
                    {
                        Topology = "TestTopology",
                        Name = "WordNormalizeBolt",
                        IsSpout = false,
                        InQueue = "spoutoutput1",
                        OutQueues = "wnboltoutput1,wnboltoutput2",
                        SchemaGroupingMode = "FieldGrouping",
                        GroupingField = "word",
                        HeartBeat = DateTime.UtcNow,
                    },

            new ActorAssignment(Guid.Empty)
                    {
                        Topology = "TestTopology",
                        Name = "WordNormalizeBolt",
                        IsSpout = false,
                        InQueue = "spoutoutput2",
                        OutQueues = "wnboltoutput1,wnboltoutput2",
                        SchemaGroupingMode = "FieldGrouping",
                        GroupingField = "word",
                        HeartBeat = DateTime.UtcNow,
                    },

            new ActorAssignment(Guid.Empty)
                    {
                        Topology = "TestTopology",
                        Name = "WordCountBolt",
                        IsSpout = false,
                        InQueue = "wnboltoutput1",
                        OutQueues = null,
                        HeartBeat = DateTime.UtcNow,
                    },

            new ActorAssignment(Guid.Empty)
                    {
                        Topology = "TestTopology",
                        Name = "WordCountBolt",
                        IsSpout = false,
                        InQueue = "wnboltoutput2",
                        OutQueues = null,
                        HeartBeat = DateTime.UtcNow,
                    },
        };

        private static List<ActorAssignment> cfrAssignments = new List<ActorAssignment>()
        {
            new ActorAssignment(Guid.Empty)
                    {
                        Topology = "CFRTopology",
                        Name = "TagIdSpout",
                        IsSpout = true,
                        InQueue = string.Empty,
                        OutQueues = "cfroutput1,cfroutput2",
                        SchemaGroupingMode = "FieldGrouping",
                        GroupingField = "tagId,dateTime",
                        HeartBeat = DateTime.UtcNow,
                    },

            new ActorAssignment(Guid.Empty)
                    {
                        Topology = "CFRTopology",
                        Name = "TagIdGroupBolt",
                        IsSpout = false,
                        InQueue = "cfroutput1",
                        OutQueues = "cfroutput3,cfroutput4",
                        SchemaGroupingMode = "FieldGrouping",
                        GroupingField = "page,dateTime",
                        HeartBeat = DateTime.UtcNow,
                    },

            new ActorAssignment(Guid.Empty)
                    {
                        Topology = "CFRTopology",
                        Name = "TagIdGroupBolt",
                        IsSpout = false,
                        InQueue = "cfroutput2",
                        OutQueues = "cfroutput3,cfroutput4",
                        SchemaGroupingMode = "FieldGrouping",
                        GroupingField = "page,dateTime",
                        HeartBeat = DateTime.UtcNow,
                    },

            new ActorAssignment(Guid.Empty)
                    {
                        Topology = "CFRTopology",
                        Name = "PageGroupBolt",
                        IsSpout = false,
                        InQueue = "cfroutput3",
                        HeartBeat = DateTime.UtcNow,
                    },

            new ActorAssignment(Guid.Empty)
                    {
                        Topology = "CFRTopology",
                        Name = "PageGroupBolt",
                        IsSpout = false,
                        InQueue = "cfroutput4",
                        HeartBeat = DateTime.UtcNow,
                    },
        };

        public static void PrepareTestData(Actor actor)
        {
            // $TEST: This code for testing environment only
            if (RoleEnvironment.IsEmulated && ActorStep < 5)
            {
                CloudTable table = Environment.GetTable("topology");
                ActorAssignment entity = Example == 0 ? wordCountAssignments[ActorStep++] : cfrAssignments[ActorStep++];

                entity.RowKey = actor.Id.ToString();

                TableOperation insertOperation = TableOperation.InsertOrReplace(entity);
                table.Execute(insertOperation);
            }
        }
        #endregion
    }
}
