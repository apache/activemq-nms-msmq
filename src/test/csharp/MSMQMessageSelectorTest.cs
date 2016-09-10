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
using System.Globalization;

namespace Apache.NMS.MSMQ.Test
{
	[TestFixture]
	[Category("LongRunning")]
	public class MSMQMessageSelectorTest : MessageSelectorTest
	{
		protected const string SELECTOR_TEST_QUEUE = "messageSelectorTestQueue";
		protected const string SELECTOR_TEST_TOPIC = "messageSelectorTestTopic";

		public MSMQMessageSelectorTest()
			: base(new MSMQTestSupport())
		{
		}

		[Test]
		public override void TestFilterIgnoredMessages(
			[Values(SELECTOR_TEST_QUEUE /*, SELECTOR_TEST_TOPIC*/)]
			string testDestRef)
		{
			base.TestFilterIgnoredMessages(testDestRef);
		}

		[Test]
		public override void TestFilterIgnoredMessagesSlowConsumer(
			[Values(SELECTOR_TEST_QUEUE /*, SELECTOR_TEST_TOPIC*/)]
			string testDestRef)
		{
			base.TestFilterIgnoredMessagesSlowConsumer(testDestRef);
		}


		[Test]
		public override void TestInvalidSelector(
			[Values(SELECTOR_TEST_QUEUE)]
			string testDestRef)
		{
			base.TestInvalidSelector(testDestRef);
		}

		[Test]
		public void TestSelectByMessageId(
			[Values(SELECTOR_TEST_QUEUE)]
			string testDestRef)
		{
			using(IConnection connection = CreateConnection())
			{
				connection.Start();
				using(ISession session = connection.CreateSession())
				{
					IDestination destination = GetClearDestinationByNodeReference(session, testDestRef);

                    using(IMessageProducer producer = session.CreateProducer(destination))
                    {
                        ITextMessage message = null;

                        int COUNT = 5;
                        for(int i = 1; i <= COUNT; i++)
                        {
                            message = session.CreateTextMessage("MessageSelectorTest - TestSelectByMessageId: " + i.ToString());
                            producer.Send(message);
                        }

					    using(IQueueBrowser browser = session.CreateBrowser((IQueue)destination))
					    {
                            int i = 0;
                            foreach(IMessage message0 in browser)
                            {
                                if(++i == COUNT / 2)
                                {
                                    message = message0 as ITextMessage;
                                    break;
                                }
                            }
						}

                        string selector = "NMSMessageId = '" + message.NMSMessageId + "'";

					    using(IMessageConsumer consumer = session.CreateConsumer(destination, selector))
					    {
							ITextMessage msg = consumer.Receive(TimeSpan.FromMilliseconds(2000)) as ITextMessage;
							Assert.IsNotNull(msg);
							Assert.AreEqual(msg.Text, message.Text);
							Assert.AreEqual(msg.NMSMessageId, message.NMSMessageId);

							msg = consumer.Receive(TimeSpan.FromMilliseconds(2000)) as ITextMessage;
							Assert.IsNull(msg);
						}
					}
				}
			}
		}

		[Test]
		public void TestSelectByLookupId(
			[Values(SELECTOR_TEST_QUEUE)]
			string testDestRef)
		{
			using(IConnection connection = CreateConnection())
			{
				connection.Start();
				using(ISession session = connection.CreateSession())
				{
					IDestination destination = GetClearDestinationByNodeReference(session, testDestRef);

                    using(IMessageProducer producer = session.CreateProducer(destination))
                    {
                        ITextMessage message = null;

                        int COUNT = 5;
                        for(int i = 1; i <= COUNT; i++)
                        {
                            message = session.CreateTextMessage("MessageSelectorTest - TestSelectByLookupId: " + i.ToString());
                            producer.Send(message);
                        }

					    using(IQueueBrowser browser = session.CreateBrowser((IQueue)destination))
					    {
                            int i = 0;
                            foreach(IMessage message0 in browser)
                            {
                                if(++i == COUNT / 2)
                                {
                                    message = message0 as ITextMessage;
                                    break;
                                }
                            }
						}

                        long lookupId = (long)(message.Properties["LookupId"]);
                        string selector = "LookupId = " + lookupId.ToString(CultureInfo.InvariantCulture);

					    using(IMessageConsumer consumer = session.CreateConsumer(destination, selector))
					    {
							ITextMessage msg = consumer.Receive(TimeSpan.FromMilliseconds(2000)) as ITextMessage;
							Assert.IsNotNull(msg);
							Assert.AreEqual(msg.Text, message.Text);
							Assert.AreEqual(msg.Properties["LookupId"], lookupId);

							msg = consumer.Receive(TimeSpan.FromMilliseconds(2000)) as ITextMessage;
							Assert.IsNull(msg);
						}
					}
				}
			}
		}
	}
}
