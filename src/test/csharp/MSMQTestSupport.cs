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
using System.Xml;
using Apache.NMS.Test;
using Apache.NMS.Util;
using NUnit.Framework;

using Apache.NMS.MSMQ;

namespace Apache.NMS.MSMQ.Test
{
	/// <summary>
	/// Useful class for test cases support.
	/// </summary>
	public class MSMQTestSupport : NMSTestSupport
	{
		/// <summary>
		/// Gets the environment variable name for the configuration file path.
		/// </summary>
		/// <returns>Environment variable name</returns>
		public override string GetConfigEnvVarName()
		{
			return "MSMQTESTCONFIGPATH";
		}

		/// <summary>
		/// Gets the default name for the configuration filename.
		/// </summary>
		/// <returns>Default name of the configuration filename</returns>
		public override string GetDefaultConfigFileName()
		{
			return "msmqprovider-test.config";
		}
	}
}
