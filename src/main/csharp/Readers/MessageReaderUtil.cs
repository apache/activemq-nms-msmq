using System;
using System.Messaging;
using System.Globalization;
using System.Text.RegularExpressions;
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

namespace Apache.NMS.MSMQ.Readers
{
    /// <summary>
    /// Utility routines for creating MSMQ message readers.
    /// </summary>
	public static class MessageReaderUtil
	{
        private static Regex basicSelectorRegex =
            new Regex(@"^\s*" +
                      @"(NMSMessageId)\s*=\s*'([^']*)'|" +
                      @"(NMSCorrelationId)\s*=\s*'([^']*)'|" +
                      @"(LookupId)\s*=\s*([-+]{0,1}\d+)" +
                      @"\s*$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Creates a message reader for the specified message selector.
        /// </summary>
        /// <param name="messageQueue">The MSMQ message queue from which
        /// messages will be read.</param>
        /// <param name="messageConverter">A message converter for mapping
        /// MSMQ messages to NMS messages.</param>
        /// <param name="selector">The message selector.</param>
        /// <return>A reader for the specified selector.</return>
        public static IMessageReader CreateMessageReader(
            MessageQueue messageQueue, IMessageConverter messageConverter,
            string selector)
        {
            IMessageReader reader;

            if(string.IsNullOrEmpty(selector))
            {
                reader = new NonFilteringMessageReader(messageQueue,
                    messageConverter);
            }
            else
            {
                Match match = basicSelectorRegex.Match(selector);
                if(match.Success)
                {
                    if(!string.IsNullOrEmpty(match.Groups[1].Value))
                    {
                        reader = new ByIdMessageReader(messageQueue,
                            messageConverter, match.Groups[2].Value);
                    }
                    else if(!string.IsNullOrEmpty(match.Groups[3].Value))
                    {
                        reader = new ByCorrelationIdMessageReader(messageQueue,
                            messageConverter, match.Groups[4].Value);
                    }
                    else
                    {
                        Int64 lookupId = Int64.Parse(match.Groups[6].Value,
                            CultureInfo.InvariantCulture);

                        reader = new ByLookupIdMessageReader(messageQueue,
                            messageConverter, lookupId);
                    }
                }
                else
                {
                    reader = new BySelectorMessageReader(messageQueue,
                        messageConverter, selector);
                }
            }

            return reader;
        }
	}
}
