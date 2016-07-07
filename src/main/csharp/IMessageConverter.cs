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
using System.Messaging;

namespace Apache.NMS.MSMQ
{
	public interface IMessageConverter
	{
        /// <summary>
        /// Converts the specified NMS message to an equivalent MSMQ message.
        /// </summary>
        /// <param name="message">NMS message to be converted.</param>
        /// <result>Converted MSMQ message.</result>
		Message ToMsmqMessage(IMessage message);

        /// <summary>
        /// Converts the specified MSMQ message to an equivalent NMS message
        /// (including its message body).
        /// </summary>
        /// <param name="message">MSMQ message to be converted.</param>
        /// <result>Converted NMS message.</result>
		IMessage ToNmsMessage(Message message);

        /// <summary>
        /// Converts an NMS destination to the equivalent MSMQ destination
        /// (ie. queue).
        /// </summary>
        /// <param name="destination">NMS destination.</param>
        /// <result>MSMQ queue.</result>
		MessageQueue ToMsmqDestination(IDestination destination);
	}
}
