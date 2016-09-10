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
using System.Threading;
using Apache.NMS.Test;
using NUnit.Framework;

namespace Apache.NMS.MSMQ.Test
{
	[TestFixture]
	public class MSMQConsumerTest : ConsumerTest
	{
		protected const string DEFAULT_TEST_QUEUE = "transactionTestQueue";
		protected const string DEFAULT_TEST_TOPIC = "defaultTestTopic";

		public MSMQConsumerTest()
			: base(new MSMQTestSupport())
		{
		}

// The .NET CF does not have the ability to interrupt threads, so this test is impossible.
#if !NETCF
		[Test]
		public override void TestNoTimeoutConsumer(
            [Values(DEFAULT_TEST_QUEUE /*, DEFAULT_TEST_TOPIC*/)]
            string testDestRef,
			[Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge,
				AcknowledgementMode.DupsOkAcknowledge, AcknowledgementMode.Transactional)]
			AcknowledgementMode ackMode)
		{
			base.TestNoTimeoutConsumer(testDestRef, ackMode);
		}

		[Test]
		public override void TestSyncReceiveConsumerClose(
            [Values(DEFAULT_TEST_QUEUE /*, DEFAULT_TEST_TOPIC*/)]
            string testDestRef,
			[Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge,
				AcknowledgementMode.DupsOkAcknowledge, AcknowledgementMode.Transactional)]
			AcknowledgementMode ackMode)
		{
			base.TestSyncReceiveConsumerClose(testDestRef, ackMode);
		}

		[Test]
		public override void TestDoChangeSentMessage(
            [Values(DEFAULT_TEST_QUEUE /*, DEFAULT_TEST_TOPIC*/)]
            string testDestRef,
			[Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge,
				AcknowledgementMode.DupsOkAcknowledge, AcknowledgementMode.Transactional)]
			AcknowledgementMode ackMode,
			[Values(true, false)] bool doClear)
		{
			base.TestDoChangeSentMessage(testDestRef, ackMode, doClear);
		}

		[Test]
		public override void TestConsumerReceiveBeforeMessageDispatched(
            [Values(DEFAULT_TEST_QUEUE /*, DEFAULT_TEST_TOPIC*/)]
            string testDestRef,
			[Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge,
				AcknowledgementMode.DupsOkAcknowledge, AcknowledgementMode.Transactional)]
			AcknowledgementMode ackMode)
		{
			base.TestConsumerReceiveBeforeMessageDispatched(testDestRef, ackMode);
		}

		[Test]
		public override void TestDontStart(
            [Values(DEFAULT_TEST_QUEUE /*, DEFAULT_TEST_TOPIC*/)]
            string testDestRef,
			[Values(MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode)
		{
			base.TestDontStart(testDestRef, deliveryMode);
		}

		[Test]
		public override void TestSendReceiveTransacted(
            [Values(DEFAULT_TEST_QUEUE /*, DEFAULT_TEST_TOPIC, DEFAULT_TEST_TEMP_QUEUE, DEFAULT_TEST_TEMP_TOPIC*/)]
            string testDestRef,
			[Values(MsgDeliveryMode.NonPersistent, MsgDeliveryMode.Persistent)]
			MsgDeliveryMode deliveryMode)
		{
			base.TestSendReceiveTransacted(testDestRef, deliveryMode);
		}

		[Test]
		public void TestAckedMessageAreConsumed()
		{
			base.TestAckedMessageAreConsumed(DEFAULT_TEST_QUEUE);
		}

		[Test]
		public void TestLastMessageAcked()
		{
			base.TestLastMessageAcked(DEFAULT_TEST_QUEUE);
		}

		[Test]
		public void TestUnAckedMessageAreNotConsumedOnSessionClose()
		{
			base.TestUnAckedMessageAreNotConsumedOnSessionClose(DEFAULT_TEST_QUEUE);
		}

		[Test]
		public void TestAsyncAckedMessageAreConsumed()
		{
			base.TestAsyncAckedMessageAreConsumed(DEFAULT_TEST_QUEUE);
		}

		[Test]
		public void TestAsyncUnAckedMessageAreNotConsumedOnSessionClose()
		{
			base.TestAsyncUnAckedMessageAreNotConsumedOnSessionClose(DEFAULT_TEST_QUEUE);
		}

		[Test]
		public /*override*/ void TestAddRemoveAsnycMessageListener()
		{
			base.TestAddRemoveAsnycMessageListener(DestinationType.Queue, DEFAULT_TEST_QUEUE);
		}

		[Test]
		public override void TestReceiveNoWait(
            [Values(DEFAULT_TEST_QUEUE /*, DEFAULT_TEST_TOPIC*/)]
            string testDestRef,
			[Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge,
				AcknowledgementMode.DupsOkAcknowledge, AcknowledgementMode.Transactional)]
			AcknowledgementMode ackMode,
			[Values(MsgDeliveryMode.NonPersistent, MsgDeliveryMode.Persistent)]
			MsgDeliveryMode deliveryMode)
		{
			base.TestReceiveNoWait(testDestRef, ackMode, deliveryMode);
		}

#endif

    }
}
