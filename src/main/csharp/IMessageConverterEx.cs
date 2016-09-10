using System.Messaging;
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

namespace Apache.NMS.MSMQ
{
    /// <summary>
    /// Extended IMessageConverter interface supporting new methods for
    /// optimizing message selection through "selectors".
    /// The original IMessageConverter is maintained for compatibility
    /// reasons with existing clients implementing it.
    /// </summary>
	public interface IMessageConverterEx : IMessageConverter
	{
        /// <summary>
        /// Converts the specified MSMQ message to an equivalent NMS message.
        /// </summary>
        /// <param name="message">MSMQ message to be converted.</param>
        /// <param name="convertBody">true if message body should be converted.</param>
        /// <result>Converted NMS message.</result>
		IMessage ToNmsMessage(Message message, bool convertBody);

        /// <summary>
        /// Converts an MSMQ message body to the equivalent NMS message body.
        /// </summary>
        /// <param name="message">Source MSMQ message.</param>
        /// <param name="answer">Target NMS message.</param>
		void ConvertMessageBodyToNMS(Message message, IMessage answer);
	}
}
