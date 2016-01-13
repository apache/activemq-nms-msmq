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
using System.Collections;
using System.Messaging;
using Apache.NMS;
using Apache.NMS.Util;

namespace Apache.NMS.MSMQ
{
	public class QueueBrowser : Apache.NMS.IQueueBrowser
	{
		private bool closed = false;
		private bool disposed = false;

        private readonly Session session;
        private MessageQueue messageQueue;

		public QueueBrowser(Session session, MessageQueue messageQueue)
		{
            this.session = session;
            this.messageQueue = messageQueue;
            if(null != this.messageQueue)
            {
                this.messageQueue.MessageReadPropertyFilter.SetAll();
            }

		}

		~QueueBrowser()
		{
			Dispose(false);
		}

		#region IDisposable Members

		///<summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if(disposed)
			{
				return;
			}

			if(disposing)
			{
				// Dispose managed code here.
			}

			try
			{
				Close();
			}
			catch
			{
				// Ignore errors.
			}

			disposed = true;
		}

		#endregion

		public void  Close()
		{
            if(messageQueue != null)
            {
                messageQueue.Dispose();
                messageQueue = null;
            }
			closed = true;
		}

		public string MessageSelector
		{
			get { throw new NotSupportedException(); }
		}

		public IQueue Queue
		{
			get { return new Queue(this.messageQueue.Path); }
		}

		internal class Enumerator : IEnumerator
		{
			private readonly Session session;
			private readonly MessageEnumerator innerEnumerator;

			public Enumerator(Session session, MessageQueue messageQueue)
			{
				this.session = session;
				this.innerEnumerator = messageQueue.GetMessageEnumerator2();
			}

			public object Current
			{
				get
				{
					return this.session.MessageConverter.ToNmsMessage(this.innerEnumerator.Current);
				}
			}

			public bool MoveNext()
			{
				return this.innerEnumerator.MoveNext();
			}

			public void Reset()
			{
				this.innerEnumerator.Reset();
			}
		}

		public IEnumerator GetEnumerator()
		{
			return new Enumerator(this.session, this.messageQueue);
		}
	}
}
