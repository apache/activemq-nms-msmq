﻿using System;
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
    /// A filter performing a logical combination of two objects.
    /// </summary>
    public abstract class LogicExpression : BinaryExpression, IBooleanExpression
    {
        public LogicExpression(IBooleanExpression left, IBooleanExpression right)
            : base(left, right)
        {
        }

        public bool Matches(MessageEvaluationContext message)
        {
            object value = Evaluate(message);
            return value != null && (bool)value;            
        }

        public static IBooleanExpression CreateOR(IBooleanExpression left, IBooleanExpression right)
        {
            return new ORExpression(left, right);
        }

        public static IBooleanExpression CreateAND(IBooleanExpression left, IBooleanExpression right)
        {
            return new ANDExpression(left, right);
        }
    }
}
