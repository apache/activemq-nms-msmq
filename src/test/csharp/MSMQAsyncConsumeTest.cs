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
using System.Threading;
using Apache.NMS.Test;
using NUnit.Framework;

namespace Apache.NMS.MSMQ.Test
{
	[TestFixture]
	public class MSMQAsyncConsumeTest : AsyncConsumeTest
	{
		protected static string DEFAULT_TEST_QUEUE = "defaultTestQueue";

		public MSMQAsyncConsumeTest() :
			base(new MSMQTestSupport())
		{
		}

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
		}

		[Test]
		public void TestAsynchronousConsume(
			[Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode)
		{
			base.TestAsynchronousConsume(deliveryMode, DEFAULT_TEST_QUEUE);
		}

		[Test]
		public void TestCreateConsumerAfterSend(
			[Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode)
		{
			base.TestCreateConsumerAfterSend(deliveryMode, DEFAULT_TEST_QUEUE);
		}

		[Test]
		public void TestCreateConsumerBeforeSendAddListenerAfterSend(
			[Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode)
		{
			base.TestCreateConsumerBeforeSendAddListenerAfterSend(deliveryMode, DEFAULT_TEST_QUEUE);
		}

		[Test]
		public void TestAsynchronousTextMessageConsume(
			[Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode)
		{
			base.TestAsynchronousTextMessageConsume(deliveryMode, DEFAULT_TEST_QUEUE);
		}

		[Test]
        [Ignore("Temporary queues are not supported")]
		public void TestTemporaryQueueAsynchronousConsume(
			[Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode)
		{
			using(IConnection connection = CreateConnectionAndStart(GetTestClientId()))
			using(ISession syncSession = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
			using(ISession asyncSession = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
			using(IDestination destination = GetClearDestinationByNodeReference(syncSession, DEFAULT_TEST_QUEUE))
			using(ITemporaryQueue tempReplyDestination = syncSession.CreateTemporaryQueue())
			using(IMessageConsumer consumer = asyncSession.CreateConsumer(destination))
			using(IMessageConsumer tempConsumer = asyncSession.CreateConsumer(tempReplyDestination))
			using(IMessageProducer producer = syncSession.CreateProducer(destination))
			{
				producer.DeliveryMode = deliveryMode;
				tempConsumer.Listener += new MessageListener(OnMessage);
				consumer.Listener += new MessageListener(OnQueueMessage);

				IMessage request = syncSession.CreateMessage();
				request.NMSCorrelationID = "TemqQueueAsyncConsume";
				request.NMSType = "Test";
				request.NMSReplyTo = tempReplyDestination;
				producer.Send(request);

				WaitForMessageToArrive();
				Assert.AreEqual("TempQueueAsyncResponse", receivedMsg.NMSCorrelationID, "Invalid correlation ID.");
			}
		}
	}
}
