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
    public class MSMQTransactionTest : TransactionTest
    {
        protected static string TRANSACTION_TEST_QUEUE = "transactionTestQueue";

		public MSMQTransactionTest()
			: base(new MSMQTestSupport())
		{
		}

        [Test]
        public void TestSendRollback(
			[Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode)
        {
            base.TestSendRollback(deliveryMode, TRANSACTION_TEST_QUEUE);
        }

        [Test]
        public void TestSendSessionClose(
			[Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode)
        {
            base.TestSendSessionClose(deliveryMode, TRANSACTION_TEST_QUEUE);
        }

        [Test]
        public void TestReceiveRollback(
			[Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode)
        {
            base.TestReceiveRollback(deliveryMode, TRANSACTION_TEST_QUEUE);
        }


        [Test]
        public void TestReceiveTwoThenRollback(
			[Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode)
        {
            base.TestReceiveTwoThenRollback(deliveryMode, TRANSACTION_TEST_QUEUE);
        }

        [Test]
        public void TestSendCommitNonTransaction(
			[Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge)]
			AcknowledgementMode ackMode,
			[Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode)
        {
            base.TestSendCommitNonTransaction(ackMode, deliveryMode, TRANSACTION_TEST_QUEUE);
        }

        [Test]
        public void TestReceiveCommitNonTransaction(
			[Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge)]
			AcknowledgementMode ackMode,
			[Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode)
		{
            base.TestReceiveCommitNonTransaction(ackMode, deliveryMode, TRANSACTION_TEST_QUEUE);
        }

        [Test]
        public void TestReceiveRollbackNonTransaction(
			[Values(AcknowledgementMode.AutoAcknowledge, AcknowledgementMode.ClientAcknowledge)]
			AcknowledgementMode ackMode,
			[Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode)
		{
            base.TestReceiveRollbackNonTransaction(ackMode, deliveryMode, TRANSACTION_TEST_QUEUE);
        }

        [Test]
        public void TestRedispatchOfRolledbackTx(
			[Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode)
        {
            base.TestRedispatchOfRolledbackTx(deliveryMode, TRANSACTION_TEST_QUEUE);
        }

        [Test]
        public void TestRedispatchOfUncommittedTx(
			[Values(MsgDeliveryMode.Persistent, MsgDeliveryMode.NonPersistent)]
			MsgDeliveryMode deliveryMode)
        {
            base.TestRedispatchOfUncommittedTx(deliveryMode, TRANSACTION_TEST_QUEUE);
        }
    }
}


