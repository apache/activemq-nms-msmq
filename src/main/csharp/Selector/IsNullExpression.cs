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
using System.Text;

namespace Apache.NMS.Selector
{
    /// <summary>
    /// A boolean expression which checks if an expression value is null.
    /// </summary>
    public class IsNullExpression : BooleanUnaryExpression
    {
        private bool notNot;

        protected override string ExpressionSymbol
        {
            get { return notNot ? "IS NULL" : "IS NOT NULL"; }
        }

        public IsNullExpression(IExpression right, bool notNot)
            : base(right)
        {
            this.notNot = notNot;
        }

        public override object Evaluate(MessageEvaluationContext message)
        {
            object rvalue = Right.Evaluate(message);

            bool answer = (rvalue == null || rvalue == ConstantExpression.NULL);

            return notNot ? answer : !answer;
        }

        public override string ToString()
        {
            StringBuilder answer = new StringBuilder();
            answer.Append(Right);
            answer.Append(" ");
            answer.Append(ExpressionSymbol);
            return answer.ToString();
        }
    }
}
