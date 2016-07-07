using System;
using Apache.NMS;
/**
 *
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Apache.NMS.Selector
{
    /// <summary>
    /// MessageEvaluationContext is used to cache selection results.
    /// 
    /// A message usually has multiple selectors applied against it. Some selector
    /// have a high cost of evaluating against the message. Those selectors may whish
    /// to cache evaluation results associated with the message in the
    /// MessageEvaluationContext.
    /// </summary>
    public class MessageEvaluationContext
    {
        private IMessage nmsMessage;
        public IMessage Message
        {
            get { return nmsMessage; }
            set { nmsMessage = value; }
        }

        public MessageEvaluationContext(IMessage message)
        {
            nmsMessage = message;
        }

        public object GetProperty(string name)
        {
            if(name.Length > 3 && 
               string.Compare(name.Substring(0, 3), "JMS", true) == 0)
            {
                if(string.Compare(name, "JMSCorrelationID", true) == 0)
                {
                    return nmsMessage.NMSCorrelationID;
                }
                if(string.Compare(name, "JMSMessageID", true) == 0)
                {
                    return nmsMessage.NMSMessageId;
                }
                if(string.Compare(name, "JMSPriority", true) == 0)
                {
                    return nmsMessage.NMSPriority;
                }
                if(string.Compare(name, "JMSTimestamp", true) == 0)
                {
                    return nmsMessage.NMSTimestamp;
                }
                if(string.Compare(name, "JMSType", true) == 0)
                {
                    return nmsMessage.NMSType;
                }
                if(string.Compare(name, "JMSDeliveryMode", true) == 0)
                {
                    return nmsMessage.NMSDeliveryMode;
                }
            }
            return nmsMessage.Properties[name];
        }
    }
}