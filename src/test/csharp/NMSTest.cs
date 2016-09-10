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
using System.Xml;
using Apache.NMS.Util;
using NUnit.Framework;

namespace Apache.NMS.Test
{
	/// <summary>
	/// Base class for test cases
	/// </summary>
	public abstract class NMSTest
	{
		protected TimeSpan receiveTimeout = TimeSpan.FromMilliseconds(15000);

		public static string ToHex(long value)
		{
			return String.Format("{0:x}", value);
		}

		#region Constructors and test support

		private NMSTestSupport testSupport;

		static NMSTest()
		{
			Apache.NMS.Tracer.Trace = new NMSTracer();
		}

		protected NMSTest(NMSTestSupport testSupport)
		{
			this.testSupport = testSupport;
			this.testSupport.TestClassType = this.GetType();
		}

		#endregion

		#region Set up and tear down

		[SetUp]
		public virtual void SetUp()
		{
			this.testSupport.SetUp();
		}

		[TearDown]
		public virtual void TearDown()
		{
			this.testSupport.TearDown();
		}

		#endregion

		#region Configuration file

		/// <summary>
		/// The configuration document.
		/// </summary>
		public XmlDocument ConfigurationDocument
		{
			get { return this.testSupport.ConfigurationDocument; }
		}

		/// <summary>
		/// Loads the configuration file.
		/// </summary>
		/// <returns>XmlDocument of the configuration file</returns>
		protected virtual XmlDocument LoadConfigFile()
		{
			return this.testSupport.LoadConfigFile();
		}

		/// <summary>
		/// Loads the configuration file.
		/// </summary>
		/// <param name="configFilePath">Configuration file path</param>
		/// <returns>XmlDocument of the configuration file</returns>
		protected virtual XmlDocument LoadConfigFile(string configFilePath)
		{
			return this.testSupport.LoadConfigFile(configFilePath);
		}

		/// <summary>
		/// Gets the path of the configuration filename.
		/// </summary>
		/// <returns>Path of the configuration filename</returns>
		protected virtual string GetConfigFilePath()
		{
			return this.testSupport.GetConfigFilePath();
		}

		/// <summary>
		/// Gets the environment variable name for the configuration file path.
		/// </summary>
		/// <returns>Environment variable name</returns>
		protected virtual string GetConfigEnvVarName()
		{
			return this.testSupport.GetConfigEnvVarName();
		}

		/// <summary>
		/// Gets the default name for the configuration filename.
		/// </summary>
		/// <returns>Default name of the configuration filename</returns>
		protected virtual string GetDefaultConfigFileName()
		{
			return this.testSupport.GetDefaultConfigFileName();
		}

		/// <summary>
		/// Gets the value of the "value" attribute of the specified node.
		/// </summary>
		/// <param name="parentNode">Parent node</param>
		/// <param name="nodeName">Node name</param>
		/// <param name="defaultVaue">Default value</param>
		/// <returns></returns>
		protected virtual string GetNodeValueAttribute(XmlElement parentNode,
			string nodeName, string defaultVaue)
		{
			return this.testSupport.GetNodeValueAttribute(parentNode,
				nodeName, defaultVaue);
		}

		#endregion

		#region URI node

		/// <summary>
		/// Gets the URI node for the default configuration.
		/// </summary>
		/// <returns>URI node for the default configuration name</returns>
		public virtual XmlElement GetURINode()
		{
			return this.testSupport.GetURINode();
		}

		/// <summary>
		/// Gets the URI node for the default configuration.
		/// </summary>
		/// <param name="nameTestURI">Name of the default configuration node
		/// </param>
		/// <returns>URI node for the default configuration name</returns>
		public virtual XmlElement GetURINode(string nameTestURI)
		{
			return this.testSupport.GetURINode(nameTestURI);
		}

		/// <summary>
		/// Gets the name of the default connection configuration to be loaded.
		/// </summary>
		/// <returns>Default configuration name</returns>
		protected virtual string GetNameTestURI()
		{
			return this.testSupport.GetNameTestURI();
		}

		#endregion

		#region Factory

		private NMSConnectionFactory nmsFactory;
		/// <summary>
		/// The connection factory interface property.
		/// </summary>
		public IConnectionFactory Factory
		{
			get { return this.testSupport.Factory; }
		}

		/// <summary>
		/// Create the NMS Factory that can create NMS Connections.
		/// </summary>
		/// <returns>Connection factory</returns>
		protected NMSConnectionFactory CreateNMSFactory()
		{
			return this.testSupport.CreateNMSFactory();
		}

		/// <summary>
		/// Create the NMS Factory that can create NMS Connections. This
		/// function loads the connection settings from the configuration file.
		/// </summary>
		/// <param name="nameTestURI">The named connection configuration.
		/// </param>
		/// <returns>Connection factory</returns>
		protected NMSConnectionFactory CreateNMSFactory(string nameTestURI)
		{
			return this.testSupport.CreateNMSFactory(nameTestURI);
		}

		/// <summary>
		/// Get the parameters for the ConnectionFactory from the configuration
		/// file.
		/// </summary>
		/// <param name="uriNode">Parent node of the factoryParams node.</param>
		/// <returns>Object array of parameter objects to be passsed to provider
		/// factory object.  Null if no parameters are specified in
		/// configuration file.</returns>
		protected object[] GetFactoryParams(XmlElement uriNode)
		{
			return this.testSupport.GetFactoryParams(uriNode);
		}

		#endregion

		#region Client id and connection

		/// <summary>
		/// Client id.
		/// </summary>
		public string ClientId
		{
			get { return this.testSupport.ClientId; }
		}

		/// <summary>
		/// Gets a new client id.
		/// </summary>
		/// <returns>Client id</returns>
		public virtual string GetTestClientId()
		{
			return this.testSupport.GetTestClientId();
		}

		/// <summary>
		/// Create a new connection to the broker.
		/// </summary>
		/// <returns>New connection</returns>
		public virtual IConnection CreateConnection()
		{
			return this.testSupport.CreateConnection();
		}

		/// <summary>
		/// Create a new connection to the broker.
		/// </summary>
		/// <param name="newClientId">Client ID of the new connection.</param>
		/// <returns>New connection</returns>
		public virtual IConnection CreateConnection(string newClientId)
		{
			return this.testSupport.CreateConnection(newClientId);
		}

		/// <summary>
		/// Create a new connection to the broker, and start it.
		/// </summary>
		/// <returns>Started connection</returns>
		public virtual IConnection CreateConnectionAndStart()
		{
			return this.testSupport.CreateConnectionAndStart();
		}

		/// <summary>
		/// Create a new connection to the broker, and start it.
		/// </summary>
		/// <param name="newClientId">Client ID of the new connection.</param>
		/// <returns>Started connection</returns>
		public virtual IConnection CreateConnectionAndStart(string newClientId)
		{
			return this.testSupport.CreateConnectionAndStart(newClientId);
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
			return this.testSupport.GetClearDestinationByNodeReference(session, destinationNodeReference);
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
			return this.testSupport.GetClearDestination(session, destinationURI);
		}

		/// <summary>
		/// Gets an existing destination. Don't clear its contents.
		/// </summary>
		/// <param name="session">Session</param>
		/// <param name="destinationNodeReference">Configuration node name for
        /// the destination URI</param>
		/// <returns>Destination</returns>
		public virtual IDestination GetDestinationByNodeReference(ISession session,
			string destinationNodeReference)
		{
			return this.testSupport.GetDestinationByNodeReference(session, destinationNodeReference);
		}

		/// <summary>
		/// Gets a destination URI.
		/// </summary>
		/// <param name="destinationNodeReference">Configuration node name for the
		/// destination URI</param>
		/// <returns>Destination URI</returns>
		public virtual string GetDestinationURI(string destinationNodeReference)
		{
			return this.testSupport.GetDestinationURI(destinationNodeReference);
		}

		#endregion

		#region Durable consumer

		/// <summary>
		/// Register a durable consumer
		/// </summary>
		/// <param name="connectionID">Connection ID of the consumer.</param>
		/// <param name="destination">Destination name to register.  Supports
		/// embedded prefix names.</param>
		/// <param name="consumerID">Name of the durable consumer.</param>
		/// <param name="selector">Selector parameters for consumer.</param>
		/// <param name="noLocal"></param>
		protected void RegisterDurableConsumer(string connectionID,
			string destination, string consumerID, string selector, bool noLocal)
		{
			using(IConnection connection = CreateConnection(connectionID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(
					AcknowledgementMode.DupsOkAcknowledge))
				{
					ITopic destinationTopic = (ITopic)SessionUtil.GetDestination(session, destination);
					Assert.IsNotNull(destinationTopic, "Could not get destination topic.");

					using(IMessageConsumer consumer = session.CreateDurableConsumer(destinationTopic, consumerID, selector, noLocal))
					{
						Assert.IsNotNull(consumer, "Could not create durable consumer.");
					}
				}
			}
		}

		/// <summary>
		/// Unregister a durable consumer for the given connection ID.
		/// </summary>
		/// <param name="connectionID">Connection ID of the consumer.</param>
		/// <param name="consumerID">Name of the durable consumer.</param>
		protected void UnregisterDurableConsumer(string connectionID, string consumerID)
		{
			using(IConnection connection = CreateConnection(connectionID))
			{
				connection.Start();
				using(ISession session = connection.CreateSession(AcknowledgementMode.DupsOkAcknowledge))
				{
					session.DeleteDurableConsumer(consumerID);
				}
			}
		}

		#endregion

		#region Send messages

		/// <summary>
		/// Sends a specified number of text messages to the designated
		/// destination.
		/// </summary>
		/// <param name="destination">Destination.</param>
		/// <param name="deliveryMode">Delivery mode.</param>
		/// <param name="count">Number of messages to be sent.</param>
		public void SendMessages(IDestination destination,
			MsgDeliveryMode deliveryMode, int count)
		{
			IConnection connection = CreateConnection();
			connection.Start();
			SendMessages(connection, destination, deliveryMode, count);
			connection.Close();
		}

		/// <summary>
		/// Sends a specified number of text messages to the designated
		/// destination.
		/// </summary>
		/// <param name="connection">Connection.</param>
		/// <param name="destination">Destination.</param>
		/// <param name="deliveryMode">Delivery mode.</param>
		/// <param name="count">Number of messages to be sent.</param>
		public void SendMessages(IConnection connection,
			IDestination destination, MsgDeliveryMode deliveryMode, int count)
		{
			ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
			SendMessages(session, destination, deliveryMode, count);
			session.Close();
		}

		/// <summary>
		/// Sends a specified number of text messages to the designated
		/// destination.
		/// </summary>
		/// <param name="session">Session.</param>
		/// <param name="destination">Destination.</param>
		/// <param name="deliveryMode">Delivery mode.</param>
		/// <param name="count">Number of messages to be sent.</param>
		public void SendMessages(ISession session, IDestination destination,
			MsgDeliveryMode deliveryMode, int count)
		{
			IMessageProducer producer = session.CreateProducer(destination);
			producer.DeliveryMode = deliveryMode;
			for(int i = 0; i < count; i++)
			{
				producer.Send(session.CreateTextMessage("" + i));
			}
			producer.Close();
		}

		#endregion

		#region Check messages

		protected void AssertTextMessagesEqual(IMessage[] firstSet, IMessage[] secondSet)
		{
			AssertTextMessagesEqual(firstSet, secondSet, "");
		}

		protected void AssertTextMessagesEqual(IMessage[] firstSet, IMessage[] secondSet, string messsage)
		{
			Assert.AreEqual(firstSet.Length, secondSet.Length, "Message count does not match: " + messsage);

			for(int i = 0; i < secondSet.Length; i++)
			{
				ITextMessage m1 = firstSet[i] as ITextMessage;
				ITextMessage m2 = secondSet[i] as ITextMessage;

				AssertTextMessageEqual(m1, m2, "Message " + (i + 1) + " did not match : ");
			}
		}

		protected void AssertEquals(ITextMessage m1, ITextMessage m2)
		{
			AssertEquals(m1, m2, "");
		}

		protected void AssertTextMessageEqual(ITextMessage m1, ITextMessage m2, string message)
		{
			Assert.IsFalse(m1 == null ^ m2 == null, message + ": expected {" + m1 + "}, but was {" + m2 + "}");

			if(m1 == null)
			{
				return;
			}

			Assert.AreEqual(m1.Text, m2.Text, message);
		}

		protected void AssertEquals(IMessage m1, IMessage m2)
		{
			AssertEquals(m1, m2, "");
		}

		protected void AssertEquals(IMessage m1, IMessage m2, string message)
		{
			Assert.IsFalse(m1 == null ^ m2 == null, message + ": expected {" + m1 + "}, but was {" + m2 + "}");

			if(m1 == null)
			{
				return;
			}

			Assert.IsTrue(m1.GetType() == m2.GetType(), message + ": expected {" + m1 + "}, but was {" + m2 + "}");

			if(m1 is ITextMessage)
			{
				AssertTextMessageEqual((ITextMessage) m1, (ITextMessage) m2, message);
			}
			else
			{
				Assert.AreEqual(m1, m2, message);
			}
		}

		#endregion
	}
}
