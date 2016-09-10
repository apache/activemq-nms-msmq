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
using NUnit.Framework;

namespace Apache.NMS.Test
{
	//[TestFixture]
	public class ConnectionTest : NMSTest
	{
		IConnection startedConnection = null;
		IConnection stoppedConnection = null;

		protected ConnectionTest(NMSTestSupport testSupport)
			: base(testSupport)
		{
		}

		//[SetUp]
		public override void SetUp()
		{
			base.SetUp();

			startedConnection = CreateConnection(null);
			startedConnection.Start();
			stoppedConnection = CreateConnection(null);
		}

		//[TearDown]
		public override void TearDown()
		{
			startedConnection.Close();
			stoppedConnection.Close();

			base.TearDown();
		}

		/// <summary>
		/// Verify that it is possible to create multiple connections to the broker.
		/// There was a bug in the connection factory which set the clientId member which made
		/// it impossible to create an additional connection.
		/// </summary>
		//[Test]
		public virtual void TestTwoConnections()
		{
			using(IConnection connection1 = CreateConnection(null))
			{
				connection1.Start();
				using(IConnection connection2 = CreateConnection(null))
				{
					// with the bug present we'll get an exception in connection2.start()
					connection2.Start();
				}
			}
		}

		//[Test]
		public virtual void TestCreateAndDisposeWithConsumer(
			//[Values(true, false)]
			bool disposeConsumer, string testDestRef)
		{
			using(IConnection connection = CreateConnection("DisposalTestConnection"))
			{
				connection.Start();

				using(ISession session = connection.CreateSession())
				{
					IDestination destination = GetClearDestinationByNodeReference(session, testDestRef);
					IMessageConsumer consumer = session.CreateConsumer(destination);

					connection.Stop();
					if(disposeConsumer)
					{
						consumer.Dispose();
					}
				}
			}
		}

		//[Test]
		public virtual void TestCreateAndDisposeWithProducer(
			//[Values(true, false)]
			bool disposeProducer, string testDestRef)
		{
			using(IConnection connection = CreateConnection("DisposalTestConnection"))
			{
				connection.Start();

				using(ISession session = connection.CreateSession())
				{
					IDestination destination = GetClearDestinationByNodeReference(session, testDestRef);
					IMessageProducer producer = session.CreateProducer(destination);

					connection.Stop();
					if(disposeProducer)
					{
						producer.Dispose();
					}
				}
			}
		}

		//[Test]
		public virtual void TestStartAfterSend(
			//[Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode,
			string testDestRef)
		{
			using(IConnection connection = CreateConnection(GetTestClientId()))
			{
				ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
				IDestination destination = GetClearDestinationByNodeReference(session, testDestRef);
				IMessageConsumer consumer = session.CreateConsumer(destination);

				// Send the messages
				SendMessages(session, destination, deliveryMode, 1);

				// Start the conncection after the message was sent.
				connection.Start();

				// Make sure only 1 message was delivered.
				Assert.IsNotNull(consumer.Receive(TimeSpan.FromMilliseconds(1000)));
				Assert.IsNull(consumer.ReceiveNoWait());
			}
		}

		/// <summary>
		/// Tests if the consumer receives the messages that were sent before the
		/// connection was started.
		/// </summary>
		//[Test]
		public virtual void TestStoppedConsumerHoldsMessagesTillStarted(string testDestRef)
		{
			ISession startedSession = startedConnection.CreateSession(AcknowledgementMode.AutoAcknowledge);
			ISession stoppedSession = stoppedConnection.CreateSession(AcknowledgementMode.AutoAcknowledge);

			// Setup the consumers.
			IDestination destination = GetClearDestinationByNodeReference(startedSession, testDestRef);
			IMessageConsumer startedConsumer = startedSession.CreateConsumer(destination);
			IMessageConsumer stoppedConsumer = stoppedSession.CreateConsumer(destination);

			// Send the message.
			IMessageProducer producer = startedSession.CreateProducer(destination);
			ITextMessage message = startedSession.CreateTextMessage("Hello");
			producer.Send(message);

			// Test the assertions.
			IMessage m;
            if(destination.IsTopic)
            {
                m = startedConsumer.Receive(TimeSpan.FromMilliseconds(1000));
			    Assert.IsNotNull(m);
            }

			m = stoppedConsumer.Receive(TimeSpan.FromMilliseconds(1000));
			Assert.IsNull(m);

			stoppedConnection.Start();
			m = stoppedConsumer.Receive(TimeSpan.FromMilliseconds(5000));
			Assert.IsNotNull(m);

			startedSession.Close();
			stoppedSession.Close();
		}

		/// <summary>
		/// Tests if the consumer is able to receive messages even when the
		/// connecction restarts multiple times.
		/// </summary>
		//[Test]
		public virtual void TestMultipleConnectionStops(string testDestRef)
		{
			TestStoppedConsumerHoldsMessagesTillStarted(testDestRef);
			stoppedConnection.Stop();
			TestStoppedConsumerHoldsMessagesTillStarted(testDestRef);
			stoppedConnection.Stop();
			TestStoppedConsumerHoldsMessagesTillStarted(testDestRef);
		}
	}
}
