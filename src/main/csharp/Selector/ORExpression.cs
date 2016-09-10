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

namespace Apache.NMS.Selector
{
    /// <summary>
    /// A filter performing a logical OR combination of two expressions.
    /// </summary>
    public class ORExpression : LogicExpression
    {
        protected override string ExpressionSymbol
        {
            get { return "OR"; }
        }

        public ORExpression(IBooleanExpression left, IBooleanExpression right)
            : base(left, right)
        {
        }

        public override object Evaluate(MessageEvaluationContext message)
        {
            object lvalue = Left.Evaluate(message);
            if(lvalue != null && (bool)lvalue) return true;

            object rvalue = Right.Evaluate(message);
            return rvalue == null ? null : rvalue;
        }
    }
}
