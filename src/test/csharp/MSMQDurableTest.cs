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
using Apache.NMS.Util;
using Apache.NMS.Test;
using NUnit.Framework;

namespace Apache.NMS.MSMQ.Test
{
	[TestFixture]
    [Ignore("Topics are not supported")]
	public class MSMQDurableTest : DurableTest
	{
		protected const string DEFAULT_TEST_QUEUE = "defaultTestQueue";
		protected const string DEFAULT_TEST_TOPIC = "defaultTestTopic";
		protected const string DURABLE_TEST_TOPIC = "durableConsumerTestTopic";

		public MSMQDurableTest()
			: base(new MSMQTestSupport())
		{
		}
		
		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
		}

		[Test]
		public void TestSendWhileClosed(
			[Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge,
				AcknowledgementMode.DupsOkAcknowledge, AcknowledgementMode.Transactional)]
			AcknowledgementMode ackMode)
		{
			base.TestSendWhileClosed(ackMode, DEFAULT_TEST_TOPIC);
	    }		
		
		[Test]
		public void TestDurableConsumerSelectorChange(
			[Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge,
				AcknowledgementMode.DupsOkAcknowledge, AcknowledgementMode.Transactional)]
			AcknowledgementMode ackMode)
		{
			base.TestDurableConsumerSelectorChange(ackMode, DEFAULT_TEST_TOPIC);
		}

		[Test]
		public void TestDurableConsumer(
			[Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge,
				AcknowledgementMode.DupsOkAcknowledge, AcknowledgementMode.Transactional)]
			AcknowledgementMode ackMode)
		{
			string testDurableTopicURI = GetDestinationURI(DURABLE_TEST_TOPIC);

			base.TestDurableConsumer(ackMode, testDurableTopicURI); 
		}
	}
}
