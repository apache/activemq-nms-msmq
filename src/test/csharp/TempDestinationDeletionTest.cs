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
using NUnit.Framework;

namespace Apache.NMS.Test
{
	//[TestFixture]
	public class TempDestinationDeletionTest : NMSTest
	{
		protected TempDestinationDeletionTest(NMSTestSupport testSupport)
			: base(testSupport)
		{
		}

		//[Test]
		public virtual void TestTempDestinationDeletion(
			//[Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode,
			//[Values(DELETION_TEST_QUEUE, DELETION_TEST_TOPIC, DELETION_TEST_TEMP_QUEUE, DELETION_TEST_TEMP_TOPIC)]
			string testDestRef)
		{
			using(IConnection connection1 = CreateConnection(GetTestClientId()))
			{
				connection1.Start();
				using(ISession session = connection1.CreateSession(AcknowledgementMode.AutoAcknowledge))
				{
					const int MaxNumDestinations = 100;

					for(int index = 1; index <= MaxNumDestinations; index++)
					{
						IDestination destination = GetClearDestinationByNodeReference(session, testDestRef);

						using(IMessageProducer producer = session.CreateProducer(destination))
						using(IMessageConsumer consumer = session.CreateConsumer(destination))
						{
							producer.DeliveryMode = deliveryMode;

							IMessage request = session.CreateTextMessage("Hello World, Just Passing Through!");

							request.NMSType = "TEMP_MSG";
							producer.Send(request);
							IMessage receivedMsg = consumer.Receive(TimeSpan.FromMilliseconds(5000));
							Assert.IsNotNull(receivedMsg);
							Assert.AreEqual(receivedMsg.NMSType, "TEMP_MSG");
							
							// Ensures that Consumer closes out its subscription
							consumer.Close();
						}

						try
						{
							session.DeleteDestination(destination);
						}
						catch(NotSupportedException)
						{
							// Might as well not try this again.
							break;
						}
					}
				}
			}
		}
	}
}
