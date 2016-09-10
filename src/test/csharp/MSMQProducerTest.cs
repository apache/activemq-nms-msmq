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
using Apache.NMS.Test;
using NUnit.Framework;

namespace Apache.NMS.MSMQ.Test
{
	[TestFixture]
	public class MSMQProducerTest : ProducerTest
	{
		protected const string DEFAULT_TEST_QUEUE = "defaultTestQueue";
		protected const string DEFAULT_TEST_TOPIC = "defaultTestTopic";
		protected const string DEFAULT_TEST_TEMP_QUEUE = "defaultTestTempQueue";
		protected const string DEFAULT_TEST_TEMP_TOPIC = "defaultTestTempTopic";

		protected const string DEFAULT_TEST_QUEUE2 = "defaultTestQueue2";
		protected const string DEFAULT_TEST_TOPIC2 = "defaultTestTopic2";
		protected const string DEFAULT_TEST_TEMP_QUEUE2 = "defaultTestTempQueue2";
		protected const string DEFAULT_TEST_TEMP_TOPIC2 = "defaultTestTempTopic2";

		public MSMQProducerTest()
			: base(new NMSTestSupport())
		{
		}

        [Test]
        public override void TestProducerSendToNullDestinationWithoutDefault()
        {
            base.TestProducerSendToNullDestinationWithoutDefault();
        }

        [Test]
        public override void TestProducerSendToNullDestinationWithDefault(
            [Values(DEFAULT_TEST_QUEUE /*, DEFAULT_TEST_TOPIC, DEFAULT_TEST_TEMP_QUEUE, DEFAULT_TEST_TEMP_TOPIC*/)]
            string testDestRef)
        {
            base.TestProducerSendToNullDestinationWithDefault(testDestRef);
        }

		[Test]
		public override void TestProducerSendToNonDefaultDestination(
            [Values(DEFAULT_TEST_QUEUE /*, DEFAULT_TEST_TOPIC, DEFAULT_TEST_TEMP_QUEUE, DEFAULT_TEST_TEMP_TOPIC*/)]
            string unusedTestDestRef,
            [Values(DEFAULT_TEST_QUEUE2 /*, DEFAULT_TEST_TOPIC2, DEFAULT_TEST_TEMP_QUEUE2, DEFAULT_TEST_TEMP_TOPIC2*/)]
            string usedTestDestRef)
		{
            base.TestProducerSendToNonDefaultDestination(unusedTestDestRef, usedTestDestRef);
        }
	}
}
