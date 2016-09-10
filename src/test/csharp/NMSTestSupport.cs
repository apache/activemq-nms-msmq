/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Apache.NMS.Util;
using NUnit.Framework;

using Apache.NMS.MSMQ;

namespace Apache.NMS.Test
{
	/// <summary>
	/// Useful class for test cases support.
	/// </summary>
	public class NMSTestSupport
	{
		protected TimeSpan receiveTimeout = TimeSpan.FromMilliseconds(15000);
		protected int testRun;
		protected int idCounter;

        protected Type testClassType;
		public Type TestClassType
		{
			get { return this.testClassType; }
			set { this.testClassType = value; }
		}

		#region Constructors

		public NMSTestSupport()
		{
		}

		#endregion

		#region Set up and tear down

		public virtual void SetUp()
		{
			this.testRun++;
		}

		public virtual void TearDown()
		{
		}

		#endregion

		#region Configuration file

		private XmlDocument configurationDocument = null;
		/// <summary>
		/// The configuration document.
		/// </summary>
		public XmlDocument ConfigurationDocument
		{
			get
			{
				if(this.configurationDocument == null)
				{
					this.configurationDocument = LoadConfigFile();
					Assert.IsTrue(this.configurationDocument != null,
						"Error loading configuration.");
				}

				return this.configurationDocument;
			}
		}

		/// <summary>
		/// Loads the configuration file.
		/// </summary>
		/// <returns>XmlDocument of the configuration file</returns>
		public virtual XmlDocument LoadConfigFile()
		{
			return LoadConfigFile(GetConfigFilePath());
		}

		/// <summary>
		/// Loads the configuration file.
		/// </summary>
		/// <param name="configFilePath">Configuration file path</param>
		/// <returns>XmlDocument of the configuration file</returns>
		public virtual XmlDocument LoadConfigFile(string configFilePath)
		{
			XmlDocument configDoc = new XmlDocument();

			configDoc.Load(configFilePath);

			return configDoc;
		}

		/// <summary>
		/// Gets the path of the configuration filename.
		/// </summary>
		/// <returns>Path of the configuration filename</returns>
		public virtual string GetConfigFilePath()
		{
			// The full path may be specified by an environment variable
			string configFilePath = GetEnvVar(GetConfigEnvVarName(), "");
			bool configFound = (!string.IsNullOrEmpty(configFilePath)
				&& File.Exists(configFilePath));

			// Else it may be found in well known locations
			if(!configFound)
			{
				string[] paths = GetConfigSearchPaths();
				string configFileName = GetDefaultConfigFileName();

				foreach(string path in paths)
				{
					string fullpath = Path.Combine(path, configFileName);
					Tracer.Debug("\tScanning folder: " + path);

					if(File.Exists(fullpath))
					{
						Tracer.Debug("\tAssembly found!");
						configFilePath = fullpath;
						configFound = true;
						break;
					}
				}
			}

			Tracer.Debug("\tConfig file: " + configFilePath);
			Assert.IsTrue(configFound, "Connection configuration file does not exist.");
			return configFilePath;
		}

		/// <summary>
		/// Gets the environment variable name for the configuration file path.
		/// </summary>
		/// <returns>Environment variable name</returns>
		public virtual string GetConfigEnvVarName()
		{
			return "NMSTESTCONFIGPATH";
		}

		/// <summary>
		/// Gets the default name for the configuration filename.
		/// </summary>
		/// <returns>Default name of the configuration filename</returns>
		public virtual string GetDefaultConfigFileName()
		{
			return "nmsprovider-test.config";
		}

		/// <summary>
		/// Gets an array of paths where the configuration file sould be found.
		/// </summary>
		/// <returns>Array of paths</returns>
		private static string[] GetConfigSearchPaths()
		{
			ArrayList pathList = new ArrayList();

			// Check the current folder first.
			pathList.Add("");
#if !NETCF
			AppDomain currentDomain = AppDomain.CurrentDomain;

			// Check the folder the assembly is located in.
			pathList.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			if(null != currentDomain.BaseDirectory)
			{
				pathList.Add(currentDomain.BaseDirectory);
			}

			if(null != currentDomain.RelativeSearchPath)
			{
				pathList.Add(currentDomain.RelativeSearchPath);
			}
#endif

			return (string[]) pathList.ToArray(typeof(string));
		}

		/// <summary>
		/// Gets the value of the "value" attribute of the specified node.
		/// </summary>
		/// <param name="parentNode">Parent node</param>
		/// <param name="nodeName">Node name</param>
		/// <param name="defaultVaue">Default value</param>
		/// <returns></returns>
		public string GetNodeValueAttribute(XmlElement parentNode,
			string nodeName, string defaultVaue)
		{
			XmlElement node = (XmlElement)parentNode.SelectSingleNode(nodeName);

			return (node == null ? defaultVaue : node.GetAttribute("value"));
		}

		#endregion

		#region URI node

		/// <summary>
		/// Gets the URI node for the default configuration.
		/// </summary>
		/// <returns>URI node for the default configuration name</returns>
		public virtual XmlElement GetURINode()
		{
			return GetURINode(GetNameTestURI());
		}

		/// <summary>
		/// Gets the URI node for the default configuration.
		/// </summary>
		/// <param name="nameTestURI">Name of the default configuration node
		/// </param>
		/// <returns>URI node for the default configuration name</returns>
		public virtual XmlElement GetURINode(string nameTestURI)
		{
			return (XmlElement)ConfigurationDocument.SelectSingleNode(
				String.Format("/configuration/{0}", nameTestURI));
		}

		/// <summary>
		/// Gets the name of the default connection configuration to be loaded.
		/// </summary>
		/// <returns>Default configuration name</returns>
		public virtual string GetNameTestURI()
		{
			return "testURI";
		}

		#endregion

		#region Factory

		private NMSConnectionFactory nmsFactory;
		/// <summary>
		/// The connection factory interface property.
		/// </summary>
		public IConnectionFactory Factory
		{
			get
			{
				if(this.nmsFactory == null)
				{
					this.nmsFactory = CreateNMSFactory();

					Assert.IsNotNull(this.nmsFactory, "Error creating factory.");
				}

				return this.nmsFactory.ConnectionFactory;
			}
		}

		/// <summary>
		/// Create the NMS Factory that can create NMS Connections.
		/// </summary>
		/// <returns>Connection factory</returns>
		public NMSConnectionFactory CreateNMSFactory()
		{
			return CreateNMSFactory(GetNameTestURI());
		}

		/// <summary>
		/// Create the NMS Factory that can create NMS Connections. This
		/// function loads the connection settings from the configuration file.
		/// </summary>
		/// <param name="nameTestURI">The named connection configuration.
		/// </param>
		/// <returns>Connection factory</returns>
		public NMSConnectionFactory CreateNMSFactory(string nameTestURI)
		{
			XmlElement uriNode = GetURINode(nameTestURI);

			Uri brokerUri = null;
			object[] factoryParams = null;
			if(uriNode != null)
			{
				// Replace any environment variables embedded inside the string.
				brokerUri = new Uri(uriNode.GetAttribute("value"));
				factoryParams = GetFactoryParams(uriNode);
				cnxClientId = GetNodeValueAttribute(uriNode, "cnxClientId", "NMSTestClientId");
				cnxUserName = GetNodeValueAttribute(uriNode, "cnxUserName", null);
				cnxPassWord = GetNodeValueAttribute(uriNode, "cnxPassWord", null);
			}

			if(factoryParams == null)
			{
				this.nmsFactory = new Apache.NMS.NMSConnectionFactory(brokerUri);
			}
			else
			{
				this.nmsFactory = new Apache.NMS.NMSConnectionFactory(brokerUri, factoryParams);
			}

			return this.nmsFactory;
		}

		/// <summary>
		/// Get the parameters for the ConnectionFactory from the configuration
		/// file.
		/// </summary>
		/// <param name="uriNode">Parent node of the factoryParams node.</param>
		/// <returns>Object array of parameter objects to be passsed to provider
		/// factory object.  Null if no parameters are specified in
		/// configuration file.</returns>
		public object[] GetFactoryParams(XmlElement uriNode)
		{
			ArrayList factoryParams = new ArrayList();
			XmlElement factoryParamsNode = (XmlElement)uriNode.SelectSingleNode("factoryParams");

			if(factoryParamsNode != null)
			{
				XmlNodeList nodeList = factoryParamsNode.SelectNodes("param");

				if(nodeList != null)
				{
					foreach(XmlElement paramNode in nodeList)
					{
						string paramType = paramNode.GetAttribute("type");
						string paramValue = paramNode.GetAttribute("value");

						switch(paramType)
						{
							case "string":
								factoryParams.Add(paramValue);
								break;

							case "int":
								factoryParams.Add(int.Parse(paramValue));
								break;

							// TODO: Add more parameter types
						}
					}
				}
			}

			if(factoryParams.Count > 0)
			{
				return factoryParams.ToArray();
			}

			return null;
		}

		#endregion

		#region Environment variables

		/// <summary>
		/// Get environment variable value.
		/// </summary>
		/// <param name="varName"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static string GetEnvVar(string varName, string defaultValue)
		{
#if (PocketPC||NETCF||NETCF_2_0)
            string varValue = null;
#else
			string varValue = Environment.GetEnvironmentVariable(varName);
#endif
			if(null == varValue)
			{
				varValue = defaultValue;
			}

			return varValue;
		}

		#endregion

		#region Client id and connection

		protected string cnxClientId;
		/// <summary>
		/// Client id.
		/// </summary>
		public string ClientId
		{
			get { return this.cnxClientId; }
		}

		/// <summary>
		/// Gets a new client id.
		/// </summary>
		/// <returns>Client id</returns>
		public virtual string GetTestClientId()
		{
			System.Text.StringBuilder id = new System.Text.StringBuilder();

			id.Append("ID:");
			id.Append(this.GetType().Name);
			id.Append(":");
			id.Append(this.testRun);
			id.Append(":");
			id.Append(++idCounter);

			return id.ToString();
		}

		protected string cnxUserName;
		/// <summary>
		/// User name.
		/// </summary>
		public string UserName
		{
			get { return this.cnxUserName; }
		}

		protected string cnxPassWord;
		/// <summary>
		/// User pass word.
		/// </summary>
		public string PassWord
		{
			get { return this.cnxPassWord; }
		}

		/// <summary>
		/// Create a new connection to the broker.
		/// </summary>
		/// <returns>New connection</returns>
		public virtual IConnection CreateConnection()
		{
			return CreateConnection(null);
		}

		/// <summary>
		/// Create a new connection to the broker.
		/// </summary>
		/// <param name="newClientId">Client ID of the new connection.</param>
		/// <returns>New connection</returns>
		public virtual IConnection CreateConnection(string newClientId)
		{
			IConnection newConnection;

			if(this.cnxUserName == null)
			{
				newConnection = Factory.CreateConnection();
			}
			else
			{
				newConnection = Factory.CreateConnection(cnxUserName, cnxPassWord);
			}

			Assert.IsNotNull(newConnection, "Connection not created");

			if(newClientId != null)
			{
				newConnection.ClientId = newClientId;
			}

			return newConnection;
		}

		/// <summary>
		/// Create a new connection to the broker, and start it.
		/// </summary>
		/// <returns>Started connection</returns>
		public virtual IConnection CreateConnectionAndStart()
		{
			return CreateConnectionAndStart(null);
		}

		/// <summary>
		/// Create a new connection to the broker, and start it.
		/// </summary>
		/// <param name="newClientId">Client ID of the new connection.</param>
		/// <returns>Started connection</returns>
		public virtual IConnection CreateConnectionAndStart(string newClientId)
		{
			IConnection newConnection = CreateConnection(newClientId);
			newConnection.Start();
			return newConnection;
		}

		#endregion

		#region Destination

		/// <summary>
		/// Gets a clear destination by its configuration node reference.
		/// </summary>
		/// <param name="session">Session</param>
		/// <param name="destinationNodeReference">Configuration node name for
        /// the destination URI</param>
		/// <returns>Destination</returns>
		public virtual IDestination GetClearDestinationByNodeReference(
            ISession session, string destinationNodeReference)
		{
			string uri = GetDestinationURI(destinationNodeReference);
			return GetClearDestination(session, uri);
		}

		/// <summary>
		/// Gets a clear destination. This will try to delete an existing
		/// destination and re-create it.
		/// </summary>
		/// <param name="session">Session</param>
		/// <param name="destinationURI">Destination URI</param>
		/// <returns>Clear destination</returns>
		public virtual IDestination GetClearDestination(ISession session,
			string destinationURI)
		{
			IDestination destination;

			try
			{
				DeleteDestination(session, destinationURI);
				destination = CreateDestination(session, destinationURI);
			}
			catch(Exception)
			{
				// Can't delete it, so lets try and purge it.
				destination = SessionUtil.GetDestination(session, destinationURI);

				using(IMessageConsumer consumer = session.CreateConsumer(destination))
				{
					while(consumer.Receive(TimeSpan.FromMilliseconds(750)) != null)
					{
					}
				}
			}

			return destination;
		}

		/// <summary>
		/// Deletes a destination.
		/// </summary>
		/// <param name="session">Session</param>
		/// <param name="destinationURI">Destination URI</param>
		protected virtual void DeleteDestination(ISession session,
			string destinationURI)
		{
			// Only delete the destination if it can be recreated
			// SessionUtil.DeleteDestination(session, destinationURI, DestinationType.Queue)
			throw new NotSupportedException();
		}

		/// <summary>
		/// Creates a destination.
		/// </summary>
		/// <param name="session">Session</param>
		/// <param name="destinationURI">Destination URI</param>
		protected virtual IDestination CreateDestination(ISession session,
			string destinationURI)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Gets an existing destination. Don't clear its contents.
		/// </summary>
		/// <param name="session">Session</param>
		/// <param name="destinationNodeReference">Configuration node name for
		/// the destination URI</param>
		/// <returns>Destination</returns>
		public virtual IDestination GetDestinationByNodeReference(
            ISession session, string destinationNodeReference)
		{
			string uri = GetDestinationURI(destinationNodeReference);

			IDestination destination = SessionUtil.GetDestination(session, uri);

			return destination;
		}

		/// <summary>
		/// Gets a destination URI.
		/// </summary>
		/// <param name="destinationNodeReference">Configuration node name for
        /// the destination URI</param>
		/// <returns>Destination URI</returns>
		public virtual string GetDestinationURI(
			string destinationNodeReference)
		{
			string uri = null;

			if(!string.IsNullOrEmpty(destinationNodeReference))
			{
				XmlElement uriNode = GetURINode();

				if(uriNode != null)
				{
					uri = GetNodeValueAttribute(uriNode, destinationNodeReference, null);
				}
			}

			if(string.IsNullOrEmpty(uri))
			{
				uri = NewDestinationURI(destinationNodeReference);
			}

			return uri;
		}

		/// <summary>
		/// Gets a new destination URI for the specified URI scheme (valid
        /// values are "queue://", "topic://", "temp-queue://" and
        /// "temp-topic://").
		/// </summary>
		/// <param name="destinationTypeScheme">Destination type</param>
		/// <returns>Destination URI</returns>
		public virtual string NewDestinationURI(string destinationTypeScheme)
		{
            if(destinationTypeScheme != "queue://" &&
               destinationTypeScheme != "topic://" &&
               destinationTypeScheme != "temp-queue://" &&
               destinationTypeScheme != "temp-topic://")
            {
                throw new ArgumentException(
                    string.Format("Invalid destination type scheme \"{0}\".",
                    destinationTypeScheme));
            }

			return destinationTypeScheme + "TEST." + this.TestClassType.Name
						+ "." + Guid.NewGuid().ToString();
		}

		#endregion
	}
}
